using System.Collections.Generic;
using BuildVariants.Model.BuildInfo.VariantCollection;

namespace BuildVariants.Model.BuildInfo {
    public interface IReadOnlyBuildInfo {
        string ActiveVariantGuid { get; }
        string SelectedVariantGuid { get; }
        string ActiveVariantCollectionName { get; }
        IEnumerable<IReadOnlyVariantCollection> VariantCollections { get; }
    }
}