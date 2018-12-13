using System.Collections.Generic;
using System.IO;
using BuildVariants.Model.BuildVariant;
using BuildVariants.Model.BuildVariant.Settings;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace BuildVariants.Controller.BuildVariants {
    public class BuildVariantsStorage : IBuildVariantsStorage {
        private const string VariantFileExtension = ".yaml";

        private readonly string _buildVariantsPath;

        public BuildVariantsStorage(string pluginFolderName) {
            var projectPath = Path.Combine(Application.dataPath, "..");
            var pluginPath = Path.Combine(projectPath, pluginFolderName);
            _buildVariantsPath = Path.Combine(pluginPath, "Variants");

            InitializeVariantsFolder();
        }
        
        private void InitializeVariantsFolder() {
            if (!Directory.Exists(_buildVariantsPath)) {
                Directory.CreateDirectory(_buildVariantsPath);
            }
        }

        public List<BuildVariant> LoadVariants() {
            var result = new List<BuildVariant>();
            var variantsDictionary = new Dictionary<string, BuildVariant>();
            var info = new DirectoryInfo(_buildVariantsPath);
            var files = info.GetFiles("*" + VariantFileExtension);
            foreach (var file in files) {
                var input = new StringReader(File.ReadAllText(file.FullName));
                var deserializer = new DeserializerBuilder().WithTagMapping("!BuildVariant", typeof(BuildVariant)).
                    WithTagMapping("!ProjectSettingsFile", typeof(ProjectSettingsFile)).
                    WithTagMapping("!YamlMappingNode", typeof(YamlMappingNode)).
                    WithTagMapping("!YamlScalarNode", typeof(YamlScalarNode)).Build();
                var variant = deserializer.Deserialize<BuildVariant>(input);
                variant.Guid = Path.GetFileNameWithoutExtension(file.FullName);                
                result.Add(variant);
                variantsDictionary.Add(variant.Guid, variant);
            }

            foreach (var buildVariant in result) {
                BuildVariant buildVariantParent;
                variantsDictionary.TryGetValue(buildVariant.ParentGuid, out buildVariantParent);
                buildVariant.Parent = buildVariantParent;
            }

            return result;
        }
        
        public void RemoveVariant(IBuildVariant buildVariant) {
            File.Delete(Path.Combine(_buildVariantsPath, buildVariant.Guid + VariantFileExtension));
        }

        public void SaveVariant(IBuildVariant buildVariant) {
            var serializer = new SerializerBuilder().WithTagMapping("!BuildVariant", typeof(BuildVariant)).
                WithTagMapping("!ProjectSettingsFile", typeof(ProjectSettingsFile)).
                WithTagMapping("!YamlMappingNode", typeof(YamlMappingNode)).
                WithTagMapping("!YamlScalarNode", typeof(YamlScalarNode)).
                EnsureRoundtrip().Build();
            File.WriteAllText(Path.Combine(_buildVariantsPath, buildVariant.Guid + VariantFileExtension), serializer.Serialize(buildVariant));
        }
    }
}