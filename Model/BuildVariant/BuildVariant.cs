using System.Collections.Generic;
using System.Linq;
using BuildVariants.Model.BuildVariant.Files;
using BuildVariants.Model.BuildVariant.Settings;
using BuildVariants.Utils;
using UnityEditor;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace BuildVariants.Model.BuildVariant {
    public class BuildVariant : IBuildVariant {
        public BuildOptions BuildOptions { get; set; }

        [YamlIgnore]
        public string Guid { get; set; }
        [YamlIgnore]
        public IBuildVariant Parent { get; set; }
        
        public BuildTarget BuildTarget { get; set; }
        public string ParentGuid { get; set; }
        public string VariantName { get; set; }
        public string BuildPath { get; set; }
        public bool MakeZip { get; set; }

        public List<ProjectSettingsFile> ProjectSettingsFiles { get; set; }
        public List<FileMoveInfo> MoveFiles { get; set; }
        public Dictionary<string, bool> ExpandedFields { get; set; }
        
        IEnumerable<IProjectSettingsFile> IBuildVariant.SettingsFileDiffs {
            get { return ProjectSettingsFiles.Cast<IProjectSettingsFile>(); }
        }
        
        public BuildVariant() {
            ParentGuid = "";
            ProjectSettingsFiles = new List<ProjectSettingsFile>();
            MoveFiles = new List<FileMoveInfo>();
            ExpandedFields = new Dictionary<string, bool>();
        }

        public bool IsFieldExpanded(string fieldName) {
            bool result;
            ExpandedFields.TryGetValue(fieldName, out result);
            return result;
        }

        public void SetFieldExpanded(string fieldName, bool value) {
            ExpandedFields[fieldName] = value;
        }
        
        ICollection<FileMoveInfo> IBuildVariant.MoveFiles {
            get { return MoveFiles; }
        }

        public void Merge(IEnumerable<IProjectSettingsFile> projectSettingsFiles) {
            ProjectSettingsFiles = ProjectSettingsFiles.Cast<IProjectSettingsFile>()
                .Concat(projectSettingsFiles).Merge().Cast<ProjectSettingsFile>().ToList();
        }

        public void Merge(IProjectSettingsFile projectSettingsFile) {
            ProjectSettingsFiles = ProjectSettingsFiles.Cast<IProjectSettingsFile>().Append(projectSettingsFile).Merge()
                .Cast<ProjectSettingsFile>().ToList();
        }
        
        public void Merge(IProjectSettingsFile projectSettingsFile, YamlNode settingsNode) {
            ProjectSettingsFiles = ProjectSettingsFiles.Cast<IProjectSettingsFile>().Append(new ProjectSettingsFile {
                FileName = projectSettingsFile.FileName,
                RootNode = projectSettingsFile.RootNode.GetChildBranch(settingsNode)
            }).Merge()
                .Cast<ProjectSettingsFile>().ToList();
        }

        public void Revert() {
            ProjectSettingsFiles.Clear();
        }

        public void Revert(IProjectSettingsFile projectSettingsFile) {
            ProjectSettingsFiles = ProjectSettingsFiles.Where(p => p != projectSettingsFile).ToList();
        }

        public void Revert(IProjectSettingsFile projectSettingsFile, YamlNode settingsNode) {
            Revert(projectSettingsFile);
            var rootNode = projectSettingsFile.RootNode.Diff(projectSettingsFile.RootNode.GetChildBranch(settingsNode));
            if (rootNode != null) {
                ProjectSettingsFiles = ProjectSettingsFiles.Cast<IProjectSettingsFile>().Append(new ProjectSettingsFile {
                        FileName = projectSettingsFile.FileName,
                        RootNode = rootNode
                    }).
                    Merge().Cast<ProjectSettingsFile>().ToList();   
            }
        }

        public IEnumerable<IProjectSettingsFile> GetFinalProjectSettings() {
            return GetBuildVariantsChain().SelectMany(b => b.SettingsFileDiffs).Merge();
        }

        public IEnumerable<FileMoveInfo> GetFinalMoveFiles() {
            return GetBuildVariantsChain().SelectMany(b => b.MoveFiles); 
        }
        
        private IEnumerable<IBuildVariant> GetBuildVariantsChain() {
            var result = new List<IBuildVariant>();
            IBuildVariant nextVariantInChain = this;
            while (nextVariantInChain != null) {
                result.Insert(0, nextVariantInChain);
                nextVariantInChain = nextVariantInChain.Parent;
            }
            return result;   
        }
    }
}