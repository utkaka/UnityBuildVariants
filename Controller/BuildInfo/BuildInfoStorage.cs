using System.Collections.Generic;
using System.IO;
using BuildVariants.Model.BuildInfo.VariantCollection;
using UnityEngine;
using YamlDotNet.Serialization;

namespace BuildVariants.Controller.BuildInfo {
    public class BuildInfoStorage : IBuildInfoStorage {
        private const string BuildInfoFilename = "build.yaml";
        
        private readonly string _buildInfoPath;
        private readonly string _pluginPath;

        public BuildInfoStorage(string pluginFolderName) {
            var projectPath = Path.Combine(Application.dataPath, "..");
            _pluginPath = Path.Combine(projectPath, pluginFolderName);
            _buildInfoPath = Path.Combine(_pluginPath, BuildInfoFilename);

            InitializeBuildInfoFolder();
        }
        
        private void InitializeBuildInfoFolder() {
            if (!Directory.Exists(_pluginPath)) {
                Directory.CreateDirectory(_pluginPath);
            }
        }

        public Model.BuildInfo.BuildInfo LoadBuildInfo() {
            if (!File.Exists(_buildInfoPath)) return new Model.BuildInfo.BuildInfo {
                ActiveVariantCollectionName = "Default",
                VariantCollections = new List<VariantCollection> {
                    new VariantCollection{BuildVariantGuids = new List<string>(), Name = "Default"}
                }
            };
            var input = new StringReader(File.ReadAllText(_buildInfoPath));
            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<Model.BuildInfo.BuildInfo>(input);
        }

        public void SaveBuildInfo(Model.BuildInfo.BuildInfo buildInfo) {
            var serializer = new SerializerBuilder().Build();
            File.WriteAllText(_buildInfoPath, serializer.Serialize(buildInfo));
        }
    }
}