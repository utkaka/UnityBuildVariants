using System.Collections.Generic;
using System.Linq;
using BuildVariants.Model.BuildInfo.VariantCollection;

namespace BuildVariants.Model.BuildInfo {
    public class BuildInfo : IReadOnlyBuildInfo {
        public string ActiveVariantGuid { get; set; }
        public string SelectedVariantGuid { get; set; }
        public string ActiveVariantCollectionName { get; set; }
        public List<VariantCollection.VariantCollection> VariantCollections { get; set; }

        IEnumerable<IReadOnlyVariantCollection> IReadOnlyBuildInfo.VariantCollections {
            get { return VariantCollections.Cast<IReadOnlyVariantCollection>(); }
        }
    }
}