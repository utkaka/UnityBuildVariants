using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BuildVariants.Controller.BuildInfo;
using BuildVariants.Controller.BuildVariants;
using BuildVariants.Controller.ProjectSettings;
using BuildVariants.Model;
using BuildVariants.Model.BuildVariant;
using Ionic.Zip;
using UnityEditor;
using UnityEngine;

namespace BuildVariants.Controller {
    public static class BuildController {
        public const string PluginFolder = "BuildVariants";
        
        private static ProjectSettingsController _projectSettingsController;
        private static BuildInfoController _buildInfoController;
        private static BuildVariantsController _buildVariantsController;

        private static void Initialize() {
            var projectSettingsStorage = new ProjectSettingsStorage(PluginFolder);
            _projectSettingsController = new ProjectSettingsController(projectSettingsStorage);
            
            var buildInfoStorage = new BuildInfoStorage(PluginFolder);
            _buildInfoController = new BuildInfoController(buildInfoStorage, _projectSettingsController);
            
            var buildVariantsStorage = new BuildVariantsStorage(PluginFolder);
            _buildVariantsController = new BuildVariantsController(buildVariantsStorage, _buildInfoController);
        }

        public static void BuildAll() {
            Initialize();
            BuildAll(_buildInfoController, _buildVariantsController, _projectSettingsController);
        }
        
        public static void BuildAll(IBuildInfoController buildInfoController, 
            IBuildVariantsController buildVariantsController, 
            IProjectSettingsController projectSettingsController) {
            CheckForUnsavedSettings(buildInfoController, buildVariantsController, projectSettingsController);
            foreach (var buildVariant in buildVariantsController.BuildVariants) {
                BuildVariant(projectSettingsController, buildVariant);
            }
            RollBack(buildInfoController, buildVariantsController, projectSettingsController);
        }

        public static void BuildCollection() {
            Initialize();
            BuildColleciton(_buildInfoController, _buildVariantsController, _projectSettingsController, 
                GetCommandLineArg("-collection"));
        }

        public static void BuildColleciton(IBuildInfoController buildInfoController, 
            IBuildVariantsController buildVariantsController, IProjectSettingsController projectSettingsController, 
            string setName) {
            CheckForUnsavedSettings(buildInfoController, buildVariantsController, projectSettingsController);
            var set = buildInfoController.BuildInfo.VariantCollections.First(s => s.Name == setName);
            foreach (var buildVariant in buildVariantsController.BuildVariants) {
                if (set.GetBuildVariantGuids().Contains(buildVariant.Guid)) {
                    BuildVariant(projectSettingsController, buildVariant);
                }
            }
            RollBack(buildInfoController, buildVariantsController, projectSettingsController);
        }

        public static void BuildVariant() {
            Initialize();
            BuildVariant(_buildInfoController, _buildVariantsController, _projectSettingsController, 
                GetCommandLineArg("-variant"));
        }

        public static void BuildVariant(IBuildInfoController buildInfoController, 
            IBuildVariantsController buildVariantsController, IProjectSettingsController projectSettingsController, 
            string variantGuid) {
            CheckForUnsavedSettings(buildInfoController, buildVariantsController, projectSettingsController);
            var buildVariant = buildVariantsController.BuildVariants.First(b => b.Guid == variantGuid);
            BuildVariant(projectSettingsController, buildVariant);
            RollBack(buildInfoController, buildVariantsController, projectSettingsController);
        }

        private static void CheckForUnsavedSettings(IBuildInfoController buildInfoController, 
            IBuildVariantsController buildVariantsController, IProjectSettingsController projectSettingsController) {
            if (projectSettingsController.GetDiffWithActualSettings(buildVariantsController.BuildVariants.First(v =>
                v.Guid == buildInfoController.BuildInfo.ActiveVariantGuid)).Any()) {
                throw new Exception("Active variant has unsaved settings!");
            }
        }

        private static void RollBack(IBuildInfoController buildInfoController, 
            IBuildVariantsController buildVariantsController, IProjectSettingsController projectSettingsController) {
            projectSettingsController.BuildAndApplyProjectSettings(buildVariantsController.BuildVariants.
                First(v => v.Guid == buildInfoController.BuildInfo.ActiveVariantGuid));
        }

        private static void BuildVariant(IProjectSettingsController projectSettingsController, IBuildVariant buildVariant) {
            var tempPath = Path.Combine(Path.Combine(PluginFolder, "Temp"), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            Action restoreFiles = () => { };

            restoreFiles = buildVariant.MoveFiles.Where(f => f.PerformOnStage == BuildStage.BeforeBuild).
                Aggregate(restoreFiles, (current, fileCopyInfo) => 
                    current + MoveFile(fileCopyInfo.From, fileCopyInfo.To, fileCopyInfo.Revert ? tempPath : null));
            projectSettingsController.BuildAndApplyProjectSettings(buildVariant);
            var buildPlayerOptions = new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = buildVariant.BuildPath,
                target = buildVariant.BuildTarget,
                options = buildVariant.BuildOptions
            };
            

            typeof(BuildPipeline).GetMethod("BuildPlayer", new [] {typeof(BuildPlayerOptions)}).
                Invoke(null, new object[] { buildPlayerOptions } );
            
            restoreFiles();

            foreach (var fileMoveInfo in buildVariant.MoveFiles.Where(f => f.PerformOnStage == BuildStage.AfterBuild)) {
                MoveFile(fileMoveInfo.From, fileMoveInfo.To);
            }

            if (buildVariant.MakeZip) {
                var folderToCompressPath = Path.HasExtension(buildVariant.BuildPath)
                    ? Path.GetDirectoryName(buildVariant.BuildPath)
                    : buildVariant.BuildPath;
                var folderName = Path.GetFileName(folderToCompressPath);
                if (folderName != null) {
                    using (var zip = new ZipFile())
                    {
                        zip.AddDirectory(folderToCompressPath);
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                        zip.Save(Path.Combine(Directory.GetParent(folderToCompressPath).FullName, folderName + ".zip"));   
                    }   
                }
            }
            
            Directory.Delete(tempPath);
            
            foreach (var fileMoveInfo in buildVariant.MoveFiles.Where(f => f.PerformOnStage == BuildStage.AfterAll)) {
                MoveFile(fileMoveInfo.From, fileMoveInfo.To);
            }
        }

        private static Action MoveFile(string from, string to, string tempPath = null) {
            var isFromDirectory = (File.GetAttributes(from) & FileAttributes.Directory) == FileAttributes.Directory;
            var toFileExists = isFromDirectory ? Directory.Exists(to) : File.Exists(to);
            
            Action restoreToFile = () =>{ };

            if (toFileExists) {
                var isToDirectory = (File.GetAttributes(to) & FileAttributes.Directory) == FileAttributes.Directory;
                if (isFromDirectory != isToDirectory) throw new Exception("'From' and 'To' paths must be of the same type (directory or file)!");
                var tempName = Guid.NewGuid().ToString();
                if (isFromDirectory) {
                    if (tempPath != null) {
                        Directory.Move(to, Path.Combine(tempPath, tempName));
                        restoreToFile += () => Directory.Move(Path.Combine(tempPath, tempName), to);   
                    } else {
                        Directory.Delete(to, true);
                    }
                } else {
                    if (tempPath != null) {
                        File.Move(to, Path.Combine(tempPath, tempName));
                        restoreToFile += () => File.Move(Path.Combine(tempPath, tempName), to);
                    } else {
                        File.Delete(to);
                    }
                }
            } 
            if (isFromDirectory) {
                Directory.Move(from, to);
                return () => {
                    Directory.Move(from, to);
                    restoreToFile();
                };
            }

            File.Move(from, to);
            return () => {
                File.Move(from, to);
                restoreToFile();
            };
        }
        
        private static string GetCommandLineArg(string name) {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++) {
                if (args[i] == name && args.Length > i + 1) {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}