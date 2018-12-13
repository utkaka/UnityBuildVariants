using System.Collections.Generic;
using System.IO;
using System.Linq;
using BuildVariants.Model.BuildVariant.Settings;
using UnityEditor;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace BuildVariants.Controller.ProjectSettings {
    public class ProjectSettingsStorage : IProjectSettingsStorage {
        private const string ProjectSettingsFileExtension = ".asset";
        
        private readonly string _projectSettingsPath;
        private readonly string _baseConfigsPath;
        
        public ProjectSettingsStorage(string pluginFolderName) {
            var projectPath = Path.Combine(Application.dataPath, "..");
            _projectSettingsPath = Path.Combine(projectPath, "ProjectSettings");
            _baseConfigsPath = Path.Combine(Path.Combine(projectPath, pluginFolderName), "Base");

            InitializeBaseConfigsFolder();
            RefreshAssets();
        }
        
        private void InitializeBaseConfigsFolder() {
            if (Directory.Exists(_baseConfigsPath)) return;
            Directory.CreateDirectory(_baseConfigsPath);
                
            foreach(var file in Directory.GetFiles(_projectSettingsPath, string.Format("*{0}", ProjectSettingsFileExtension)))
                File.Copy(file, Path.Combine(_baseConfigsPath, Path.GetFileName(file)));
        }
        
        private void RefreshAssets() {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        
        public void SaveProjectSettings(IEnumerable<IProjectSettingsFile> projectSettings) {
            foreach (var projectSetting in projectSettings) {
                File.WriteAllLines(Path.Combine(_projectSettingsPath, projectSetting.FileName + ProjectSettingsFileExtension),
                    File.ReadAllLines(Path.Combine(_projectSettingsPath, projectSetting.FileName + ProjectSettingsFileExtension)).Take(3).ToArray());
                
                var serializer = new SerializerBuilder().Build();
                File.AppendAllText(
                    Path.Combine(_projectSettingsPath, projectSetting.FileName + ProjectSettingsFileExtension),
                    serializer.Serialize(projectSetting.RootNode).Replace("&1", ""));
            }
            RefreshAssets();
        }

        public IEnumerable<IProjectSettingsFile> LoadProjectSettings() {
            return LoadSettings(_projectSettingsPath);
        }

        public IEnumerable<IProjectSettingsFile> LoadBaseSettings() {
            return LoadSettings(_baseConfigsPath);
        }

        private IEnumerable<IProjectSettingsFile> LoadSettings(string settingsFolder) {
            var result = new List<IProjectSettingsFile>();
            foreach (var file in Directory.GetFiles(settingsFolder, string.Format("*{0}", ProjectSettingsFileExtension))) {
                var yamlStream = new YamlStream();
                yamlStream.Load(new StringReader(File.ReadAllText(file)));
                result.Add(new ProjectSettingsFile {
                    FileName = Path.GetFileNameWithoutExtension(file),
                    RootNode = yamlStream.Documents[0].RootNode
                });
            }
            return result;
        }
    }
}