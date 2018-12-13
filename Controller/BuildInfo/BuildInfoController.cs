using System;
using System.Collections.Generic;
using System.Linq;
using BuildVariants.Controller.ProjectSettings;
using BuildVariants.Model.BuildInfo;
using BuildVariants.Model.BuildInfo.VariantCollection;
using BuildVariants.Model.BuildVariant;

namespace BuildVariants.Controller.BuildInfo {
    public class BuildInfoController : IBuildInfoController {
        private readonly Model.BuildInfo.BuildInfo _buildInfo;
        private readonly BuildInfoStorage _buildInfoStorage;
        
        private IProjectSettingsController _projectSettingsController;
        private VariantCollection _activeCollection;

        public IReadOnlyBuildInfo BuildInfo {
            get { return _buildInfo; }
        }
        
        public BuildInfoController(BuildInfoStorage buildInfoStorage, IProjectSettingsController projectSettingsController) {
            _projectSettingsController = projectSettingsController;
            _buildInfoStorage = buildInfoStorage;
            
            _buildInfo = _buildInfoStorage.LoadBuildInfo();
            _activeCollection = _buildInfo.VariantCollections.Find(v => v.Name == _buildInfo.ActiveVariantCollectionName);
        }
        
        public void SelectBuildVariant(IBuildVariant buildVariant) {
            _buildInfo.SelectedVariantGuid = buildVariant != null ? buildVariant.Guid : null;
            _buildInfoStorage.SaveBuildInfo(_buildInfo);
        }

        public void ActivateBuildVariant(IBuildVariant buildVariant) {
            _buildInfo.ActiveVariantGuid = buildVariant != null ? buildVariant.Guid : null;
            _buildInfoStorage.SaveBuildInfo(_buildInfo);
            _projectSettingsController.BuildAndApplyProjectSettings(buildVariant);
        }

        public bool IsBuildVariantInActiveCollection(IBuildVariant buildVariant) {
            return _activeCollection.BuildVariantGuids.Contains(buildVariant.Guid);
        }

        public void ToggleBuildVariantInActiveCollection(IBuildVariant buildVariant, bool toggleValue) {            
            if (toggleValue && !IsBuildVariantInActiveCollection(buildVariant)) {
                _activeCollection.BuildVariantGuids.Add(buildVariant.Guid);
                _buildInfoStorage.SaveBuildInfo(_buildInfo);
            }

            if (!toggleValue && IsBuildVariantInActiveCollection(buildVariant)) {
                _activeCollection.BuildVariantGuids.Remove(buildVariant.Guid);
                _buildInfoStorage.SaveBuildInfo(_buildInfo);
            }
        }

        public void ActivateVariantCollection(string variantCollectionName) {
            _buildInfo.ActiveVariantCollectionName = variantCollectionName;
            _buildInfoStorage.SaveBuildInfo(_buildInfo);
            
            _activeCollection = _buildInfo.VariantCollections.Find(v => v.Name == _buildInfo.ActiveVariantCollectionName);
        }

        public void AddVariantCollection(string newVariantCollectionName) {
            CheckNewVariantName(newVariantCollectionName);
            _buildInfo.VariantCollections.Add(new VariantCollection{Name = newVariantCollectionName,
                BuildVariantGuids = new List<string>()});
            ActivateVariantCollection(newVariantCollectionName);
        }

        public void RemoveActiveVariantCollection() {
            if (_buildInfo.VariantCollections.Count == 1) {
                throw new Exception("At least one variant collection should exist");
            }

            _buildInfo.VariantCollections.Remove(
                _buildInfo.VariantCollections.Find(v => v.Name == _buildInfo.ActiveVariantCollectionName));

            ActivateVariantCollection(_buildInfo.VariantCollections[0].Name);
        }

        public void RenameActiveVariantCollection(string newVariantCollectionName) {
            CheckNewVariantName(newVariantCollectionName);
            foreach (var variantCollection in _buildInfo.VariantCollections) {
                if (variantCollection.Name != _buildInfo.ActiveVariantCollectionName) continue;
                variantCollection.Name = newVariantCollectionName;
                ActivateVariantCollection(newVariantCollectionName);
                break;
            }
        }

        private void CheckNewVariantName(string newVariantCollectionName) {
            if (string.IsNullOrEmpty(newVariantCollectionName) || 
                _buildInfo.VariantCollections.Any(v => v.Name == newVariantCollectionName)) {
                throw new Exception(string.Format("Incorrect collection name: {0}", newVariantCollectionName));
            }
        }
    }
}