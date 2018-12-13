using System.Collections.Generic;
using BuildVariants.Model.BuildVariant;

namespace BuildVariants.Controller.BuildVariants {
    public interface IBuildVariantsController {
        IEnumerable<IBuildVariant> BuildVariants { get; }
        
        void CreateVariant(IBuildVariant parentBuildVariant = null);
        void RemoveVariant(IBuildVariant buildVariant);
        void SetVariantParent(IBuildVariant buildVariant, string parentGuid);
        void SaveVariant(IBuildVariant buildVariant);
    }
}