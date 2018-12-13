using System.Collections.Generic;
using System.Linq;
using BuildVariants.Model.BuildVariant;
using BuildVariants.Model.BuildVariant.Settings;
using BuildVariants.Utils;
using YamlDotNet.RepresentationModel;

namespace BuildVariants.Controller.ProjectSettings {
    public class ProjectSettingsController : IProjectSettingsController {
        private readonly IProjectSettingsStorage _projectSettingsStorage;
        private readonly IEnumerable<IProjectSettingsFile> _baseSettings;
        private IEnumerable<IProjectSettingsFile> _actualProjectSettings;
        private KeyValuePair<IBuildVariant, IEnumerable<IProjectSettingsFile>> _cachedDiff;

        public ProjectSettingsController(IProjectSettingsStorage projectSettingsStorage) {
            _projectSettingsStorage = projectSettingsStorage;
            _baseSettings = _projectSettingsStorage.LoadBaseSettings();
            _actualProjectSettings = _projectSettingsStorage.LoadProjectSettings();
        }
        
        public IEnumerable<IProjectSettingsFile> GetDiffWithActualSettings(IBuildVariant buildVariant) {
            if (_cachedDiff.Key == buildVariant) return _cachedDiff.Value;
            _cachedDiff = new KeyValuePair<IBuildVariant, IEnumerable<IProjectSettingsFile>>(buildVariant,
                _baseSettings.Concat(buildVariant.GetFinalProjectSettings()).Diff(_actualProjectSettings));
            return _cachedDiff.Value;
        }

        public void InvalidateDiffCache() {
            _cachedDiff = new KeyValuePair<IBuildVariant, IEnumerable<IProjectSettingsFile>>();
        }

        public void BuildAndApplyProjectSettings(IBuildVariant buildVariant, 
            IEnumerable<IProjectSettingsFile> additionalSettings = null,
            IProjectSettingsFile ignoreAdditionalProjectSettingsFile = null,
            YamlNode ignoreAdditionalNode = null) {
            if (additionalSettings == null) {
                additionalSettings = Enumerable.Empty<IProjectSettingsFile>();
            } else {
                if (ignoreAdditionalProjectSettingsFile != null) {
                    additionalSettings = additionalSettings.Where(p => p.FileName != ignoreAdditionalProjectSettingsFile.FileName);
                    if (ignoreAdditionalNode != null) {
                        additionalSettings = additionalSettings.Append(
                            new ProjectSettingsFile {
                                FileName = ignoreAdditionalProjectSettingsFile.FileName,
                                RootNode =
                                    ignoreAdditionalProjectSettingsFile.RootNode.
                                        Diff(ignoreAdditionalProjectSettingsFile.RootNode.GetChildBranch(ignoreAdditionalNode))
                            });
                    }
                }
            }
            _projectSettingsStorage.SaveProjectSettings(
                _baseSettings.Concat(buildVariant.GetFinalProjectSettings()).
                    Concat(additionalSettings).Merge().ToList());
            _actualProjectSettings = _projectSettingsStorage.LoadProjectSettings();
            InvalidateDiffCache();
        }
    }
}