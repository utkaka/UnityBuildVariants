using System;
using System.Linq;
using BuildVariants.Controller;
using BuildVariants.Controller.BuildInfo;
using BuildVariants.Controller.BuildVariants;
using BuildVariants.Controller.ProjectSettings;
using BuildVariants.Model.BuildVariant;
using UnityEditor;
using UnityEngine;

namespace BuildVariants.View.Explorer {
    public class BuildVariantsExplorer {
        private readonly IBuildVariantsController _buildVariantsController;
        private readonly IBuildInfoController _buildInfoController;
        private readonly IBuildTargetIcons _buildTargetIcons;
        private readonly IProjectSettingsController _projectSettingsController;
        
        private Vector2 _scrollPosition;
        private GUIStyle _configurationsHeaderStyle;
        private GUIStyle _platformIconStyle;
        private GUIStyle _configurationStyle;
        private GUIStyle _selectedConfigurationStyle;
        
        private Action _frameAction;
        private Rect _variantCollectionRect;

        public BuildVariantsExplorer(IBuildVariantsController buildVariantsController, 
            IBuildInfoController buildInfoController, IBuildTargetIcons buildTargetIcons,
            IProjectSettingsController projectSettingsController) {
            _buildVariantsController = buildVariantsController;
            _buildInfoController = buildInfoController;
            _buildTargetIcons = buildTargetIcons;
            _projectSettingsController = projectSettingsController;
        }

        public void Draw() {
            _frameAction = () => { };
            //styles
            if (_configurationsHeaderStyle == null) {
                _configurationsHeaderStyle = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold};
            
                _platformIconStyle = new GUIStyle(GUI.skin.label);
                _configurationStyle = new GUIStyle(GUI.skin.label);
                _selectedConfigurationStyle =
                    new GUIStyle(GUI.skin.label) {normal = {textColor = new Color(0.37f, 0.52f, 0.94f)}};    
            }
            
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(300));
            EditorGUILayout.Space();

            DrawBuildControls();
            
            DrawVariantCollections();
            
            //Configurations list
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Variants:", _configurationsHeaderStyle);
            if (GUILayout.Button(new GUIContent("+", "Create new variant from current project settings"),
                GUILayout.Width(30))) {
                _buildVariantsController.CreateVariant();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            DrawVariants(null, 0);
            
            EditorGUILayout.EndScrollView(); 
            
            _frameAction();
        }

        private void DrawBuildControls() {
            EditorGUILayout.LabelField("Build:", _configurationsHeaderStyle);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("All")) {
                _frameAction += () => BuildController.BuildAll(_buildInfoController, 
                    _buildVariantsController, _projectSettingsController);
            }

            if (GUILayout.Button("Selected collection")) {
                _frameAction += () => BuildController.BuildColleciton(_buildInfoController, _buildVariantsController, _projectSettingsController,
                    _buildInfoController.BuildInfo.ActiveVariantCollectionName);
            }

            if (GUILayout.Button("Selected variant")) {
                _frameAction += () => BuildController.BuildVariant(_buildInfoController,
                    _buildVariantsController, _projectSettingsController,
                    _buildInfoController.BuildInfo.SelectedVariantGuid);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void DrawVariantCollections() {
            var popupVariants = _buildInfoController.BuildInfo.VariantCollections.Select(v => v.Name).ToList();
            var activeCollectionIndex =
                popupVariants.IndexOf(_buildInfoController.BuildInfo.ActiveVariantCollectionName);
            var lastCollection = popupVariants.Count == 1;
            popupVariants.Add("/");
            popupVariants.Add("Add");
            popupVariants.Add("Rename");
            if (!lastCollection) popupVariants.Add("Remove");
            
            EditorGUILayout.LabelField("Variant collection:", _configurationsHeaderStyle);
            var selectedOption = EditorGUILayout.Popup(activeCollectionIndex, popupVariants.ToArray());
            if (activeCollectionIndex != selectedOption) {
                var selectedOptionName = popupVariants[selectedOption]; 
                switch (selectedOptionName) {
                    case "Add":
                        PopupWindow.Show(_variantCollectionRect, 
                            new VariantCollectionNameDialog(_buildInfoController.BuildInfo.ActiveVariantCollectionName,
                                popupVariants,
                                name => _buildInfoController.AddVariantCollection(name)));
                        break;
                    case "Rename":
                        PopupWindow.Show(_variantCollectionRect,
                            new VariantCollectionNameDialog(_buildInfoController.BuildInfo.ActiveVariantCollectionName,
                                popupVariants,
                                name => _buildInfoController.RenameActiveVariantCollection(name)));
                        break;
                    case "Remove":
                        if (EditorUtility.DisplayDialog("Remove variant collection", string.Format(
                                "Do want to remove {0}?",
                                popupVariants[activeCollectionIndex]),
                            "Yes", "No")) {
                            _buildInfoController.RemoveActiveVariantCollection();
                        }
                        break;
                    default:
                        _buildInfoController.ActivateVariantCollection(selectedOptionName);
                        break;
                }
            }

            if (Event.current.type == EventType.Repaint) {
                _variantCollectionRect = GUILayoutUtility.GetLastRect();
            }
            EditorGUILayout.Space();
        }

        private void DrawVariants(IBuildVariant parent, int indentLevel) {
            foreach (var variant in _buildVariantsController.BuildVariants) {
                if (variant.Parent != parent) continue;
                DrawVariant(variant, indentLevel);
            }
        }

        private void DrawVariant(IBuildVariant buildVariant, int indentLevel) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indentLevel * 16.0f + 8.0f);
            var isVariantInActiveCollection = _buildInfoController.IsBuildVariantInActiveCollection(buildVariant);
            var newToggleValue = EditorGUILayout.Toggle("", isVariantInActiveCollection, GUILayout.Width(20));
            if (newToggleValue != isVariantInActiveCollection) {
                _buildInfoController.ToggleBuildVariantInActiveCollection(buildVariant, newToggleValue);
            }
            var isVariantActive = buildVariant.Guid == _buildInfoController.BuildInfo.ActiveVariantGuid;
            var isVariantSelected = buildVariant.Guid == _buildInfoController.BuildInfo.SelectedVariantGuid;
            if (GUILayout.Button(buildVariant.VariantName + (isVariantActive ? " (Active)" : ""),
                isVariantSelected ? _selectedConfigurationStyle : _configurationStyle)) {
                _buildInfoController.SelectBuildVariant(buildVariant);
            }
            GUILayout.FlexibleSpace();
            var icon = _buildTargetIcons.GetIconForBuildTarget(buildVariant.BuildTarget);
            GUILayout.Label(icon != null ? EditorGUIUtility.IconContent(icon) : new GUIContent(""),
                _platformIconStyle);
            EditorGUILayout.EndHorizontal();

            DrawVariants(buildVariant, indentLevel + 1);
        }
    }
}