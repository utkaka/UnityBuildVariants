using System.Collections.Generic;
using BuildVariants.Model.BuildVariant;

namespace BuildVariants.Controller.BuildVariants {
    public interface IBuildVariantsStorage {
        List<BuildVariant> LoadVariants();
        void RemoveVariant(IBuildVariant buildVariant);
        void SaveVariant(IBuildVariant buildVariant);
    }
}