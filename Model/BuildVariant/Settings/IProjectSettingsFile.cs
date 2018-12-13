using YamlDotNet.RepresentationModel;

namespace BuildVariants.Model.BuildVariant.Settings {
    public interface IProjectSettingsFile {
        string FileName { get; }
        YamlNode RootNode { get; }
        IProjectSettingsFile Merge(IProjectSettingsFile otherProjectSettingsFile);
        IProjectSettingsFile Diff(IProjectSettingsFile otherProjectSettingsFile);
    }
}