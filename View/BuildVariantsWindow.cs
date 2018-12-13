using BuildVariants.Controller;
using BuildVariants.Controller.BuildInfo;
using BuildVariants.Controller.BuildVariants;
using BuildVariants.Controller.ProjectSettings;
using BuildVariants.View.Explorer;
using BuildVariants.View.Inspector;
using UnityEditor;
using UnityEngine;

namespace BuildVariants.View {
    public class BuildVariantsWindow : EditorWindow {
        
        [MenuItem("Window/Build Variants")]
        public static void ShowWindow()
        {
            GetWindow(typeof(BuildVariantsWindow));
        }

        private BuildInfoController _buildInfoController;
        private BuildVariantsController _buildVariantsController;
        private ProjectSettingsController _projectSettingsController;
        
        private BuildVariantsExplorer _variantsExplorer;
        private BuildVariantInspector _variantInspector;

        private BuildTargetIcons _buildTargetIcons;

        private void Awake() {
            titleContent = new GUIContent("Build Variants");
        }

        private void OnFocus() {
            _buildTargetIcons = new BuildTargetIcons();
            
            var projectSettingsStorage = new ProjectSettingsStorage(BuildController.PluginFolder);
            _projectSettingsController = new ProjectSettingsController(projectSettingsStorage);
            
            var buildInfoStorage = new BuildInfoStorage(BuildController.PluginFolder);
            _buildInfoController = new BuildInfoController(buildInfoStorage, _projectSettingsController);
            
            var buildVariantsStorage = new BuildVariantsStorage(BuildController.PluginFolder);
            _buildVariantsController = new BuildVariantsController(buildVariantsStorage, _buildInfoController);
            
            _variantsExplorer = new BuildVariantsExplorer(_buildVariantsController, _buildInfoController, _buildTargetIcons, _projectSettingsController);
            _variantInspector = new BuildVariantInspector(_buildVariantsController, _buildInfoController, _projectSettingsController);
        }
    
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            _variantsExplorer.Draw();
            EditorGUILayout.LabelField("", GUI.skin.verticalSlider,  GUILayout.Height(position.height), GUILayout.Width(4.0f));
            _variantInspector.Draw();
            EditorGUILayout.EndHorizontal();
        }
    }
}
