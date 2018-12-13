using System.Collections.Generic;

namespace BuildVariants.Model.BuildInfo.VariantCollection {
    public class VariantCollection : IReadOnlyVariantCollection {
        public string Name { get; set; }

        public List<string> BuildVariantGuids { get; set; }
        
        public IEnumerable<string> GetBuildVariantGuids() {
            return BuildVariantGuids;
        }

        public void AddBuildVariantGuid(string buildVariantGuid) {
            if (!BuildVariantGuids.Contains(buildVariantGuid)) BuildVariantGuids.Add(buildVariantGuid);
        }

        public void RemoveBuildVariantGuid(string buildVariantGuid) {
            if (BuildVariantGuids.Contains(buildVariantGuid)) BuildVariantGuids.Remove(buildVariantGuid);
        }
    }
}