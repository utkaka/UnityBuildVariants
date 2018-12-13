using System.Collections.Generic;
using BuildVariants.Model.BuildVariant.Files;
using BuildVariants.Model.BuildVariant.Settings;
using UnityEditor;
using YamlDotNet.RepresentationModel;

namespace BuildVariants.Model.BuildVariant {
    public interface IBuildVariant {
        BuildTarget BuildTarget { get; set; }
        BuildOptions BuildOptions { get; set; }
        string Guid { get; }
        IBuildVariant Parent { get; }
        string ParentGuid { get; }
        string VariantName { get; set; }
        string BuildPath { get; set; }
        bool MakeZip { get; set; }
        IEnumerable<IProjectSettingsFile> SettingsFileDiffs { get; }

        bool IsFieldExpanded(string fieldName);
        void SetFieldExpanded(string fieldName, bool value);

        ICollection<FileMoveInfo> MoveFiles { get; }

        void Merge(IEnumerable<IProjectSettingsFile> projectSettingsFiles);
        void Merge(IProjectSettingsFile projectSettingsFile);
        void Merge(IProjectSettingsFile projectSettingsFile, YamlNode settingsNode);

        void Revert();
        void Revert(IProjectSettingsFile projectSettingsFile);
        void Revert(IProjectSettingsFile projectSettingsFile, YamlNode settingsNode);
        
        IEnumerable<IProjectSettingsFile> GetFinalProjectSettings();
        IEnumerable<FileMoveInfo> GetFinalMoveFiles();
    }
}