using BuildVariants.Model.BuildInfo;
using BuildVariants.Model.BuildVariant;

namespace BuildVariants.Controller.BuildInfo {
    public interface IBuildInfoController {
        IReadOnlyBuildInfo BuildInfo { get; }
        void SelectBuildVariant(IBuildVariant buildVariant);
        void ActivateBuildVariant(IBuildVariant buildVariant);

        bool IsBuildVariantInActiveCollection(IBuildVariant buildVariant);
        void ToggleBuildVariantInActiveCollection(IBuildVariant buildVariant, bool toggleValue);

        void ActivateVariantCollection(string variantCollectionName);
        void AddVariantCollection(string newVariantCollectionName);
        void RemoveActiveVariantCollection();
        void RenameActiveVariantCollection(string newVariantCollectionName);
    }
}