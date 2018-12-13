using System;
using System.IO;
using BuildVariants.Utils;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace BuildVariants.Model.BuildVariant.Settings {
    public class ProjectSettingsFile : IProjectSettingsFile {
        public string FileName { get; set; }
        public string RootNodeString {
            get {
                return new SerializerBuilder().Build().Serialize(RootNode);
            }
            set {
                var yamlStream = new YamlStream();
                yamlStream.Load(new StringReader(value));
                RootNode = yamlStream.Documents[0].RootNode;
            }
        }
        [YamlIgnore]
        public YamlNode RootNode { get; set; }

        public IProjectSettingsFile Merge(IProjectSettingsFile otherProjectSettingsFile) {
            ValidateSameFileName(otherProjectSettingsFile);
            return new ProjectSettingsFile {
                FileName = FileName,
                RootNode = RootNode != null
                    ? RootNode.Merge(otherProjectSettingsFile.RootNode)
                    : otherProjectSettingsFile.RootNode
            };
        }

        public IProjectSettingsFile Diff(IProjectSettingsFile otherProjectSettingsFile) {
            ValidateSameFileName(otherProjectSettingsFile);
            var rootNode = RootNode != null ? RootNode.Diff(otherProjectSettingsFile.RootNode) : null;
            return rootNode != null ? new ProjectSettingsFile { FileName = FileName, RootNode = rootNode } : null;
        }

        private void ValidateSameFileName(IProjectSettingsFile otherProjectSettingsFile) {
            if (FileName != otherProjectSettingsFile.FileName) {
                throw new Exception("Trying to merge different project files!");
            }
        }
    }
}