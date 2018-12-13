using System.Collections.Generic;
using BuildVariants.Model.BuildVariant.Settings;

namespace BuildVariants.Controller.ProjectSettings {
    public interface IProjectSettingsStorage {
        void SaveProjectSettings(IEnumerable<IProjectSettingsFile> projectSettings);
        IEnumerable<IProjectSettingsFile> LoadProjectSettings();
        IEnumerable<IProjectSettingsFile> LoadBaseSettings();
    }
}