using System.Collections.Generic;
using BuildVariants.Model.BuildVariant;
using BuildVariants.Model.BuildVariant.Settings;
using YamlDotNet.RepresentationModel;

namespace BuildVariants.Controller.ProjectSettings {
    public interface IProjectSettingsController {
        IEnumerable<IProjectSettingsFile> GetDiffWithActualSettings(IBuildVariant buildVariant);
        void InvalidateDiffCache();

        void BuildAndApplyProjectSettings(IBuildVariant buildVariant, 
            IEnumerable<IProjectSettingsFile> additionalSettings = null,
            IProjectSettingsFile ignoreAdditionalProjectSettingsFile = null,
            YamlNode ignoreNode = null);
    }
}