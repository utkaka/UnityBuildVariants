using System.Collections.Generic;
using System.Linq;
using BuildVariants.Controller.BuildInfo;
using BuildVariants.Model.BuildVariant;

namespace BuildVariants.Controller.BuildVariants {
    public class BuildVariantsController : IBuildVariantsController {
        private readonly BuildVariantsStorage _buildVariantsStorage;
        private readonly IBuildInfoController _buildInfoController;
        private readonly List<BuildVariant> _buildVariants;

        public IEnumerable<IBuildVariant> BuildVariants {
            get { return _buildVariants.Cast<IBuildVariant>(); }
        }

        public BuildVariantsController(BuildVariantsStorage buildVariantsStorage,
            IBuildInfoController buildInfoController) {
            _buildVariantsStorage = buildVariantsStorage;
            _buildInfoController = buildInfoController;

            _buildVariants = _buildVariantsStorage.LoadVariants();
        }

        public void CreateVariant(IBuildVariant parentBuildVariant = null) {
            var variant = new BuildVariant{
                VariantName = "New variant",
                Guid = System.Guid.NewGuid().ToString()
            };
            SetVariantParent(variant, parentBuildVariant == null ? "" : parentBuildVariant.Guid);
            _buildInfoController.ActivateBuildVariant(variant);
            _buildInfoController.SelectBuildVariant(variant);
            _buildVariants.Add(variant);
            
            SaveVariant(variant);
        }

        public void RemoveVariant(IBuildVariant buildVariant) {
            _buildVariants.Remove((BuildVariant)buildVariant);
            _buildVariantsStorage.RemoveVariant((BuildVariant)buildVariant);
        }

        public void SetVariantParent(IBuildVariant buildVariant, string parentGuid) {
            ((BuildVariant) buildVariant).ParentGuid = parentGuid;
            if (string.IsNullOrEmpty(parentGuid)) return;
            foreach (var parentVariant in _buildVariants) {
                if (parentVariant.Guid != parentGuid) continue;
                ((BuildVariant) buildVariant).Parent = parentVariant;
                SaveVariant(buildVariant);
                break;
            }
        }

        public void SaveVariant(IBuildVariant buildVariant) {
            _buildVariantsStorage.SaveVariant(buildVariant);
        }
    }
}