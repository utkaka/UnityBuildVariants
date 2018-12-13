using System.Collections.Generic;

namespace BuildVariants.Model.BuildInfo.VariantCollection {
    public interface IReadOnlyVariantCollection {
        string Name { get; }
        IEnumerable<string> GetBuildVariantGuids();
    }
}