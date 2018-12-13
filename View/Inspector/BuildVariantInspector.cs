using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BuildVariants.Controller.BuildInfo;
using BuildVariants.Controller.BuildVariants;
using BuildVariants.Controller.ProjectSettings;
using BuildVariants.Model;
using BuildVariants.Model.BuildVariant;
using BuildVariants.Model.BuildVariant.Files;
using BuildVariants.Model.BuildVariant.Settings;
using UnityEditor;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace BuildVariants.View.Inspector {
    public class BuildVariantInspector {
        private enum SettingsCategory {
            ActualSettingsDiff,
            ActualRevertableSettingsDiff,
            VariantDiff
        }
        
        private const float PropertiesFieldWidth = 200.0f;
        private const float PropertiesSaveRevertButtonWidth = 46.0f;

        private readonly IBuildVariantsController _buildVariantsController;
        private readonly IBuildInfoController _buildInfoController;
        private readonly IProjectSettingsController _projectSettingsController;

        private Action _frameAction;
        private static Vector2 _scrollPosition;

        public BuildVariantInspector
        (IBuildVariantsController buildVariantsController, 
            IBuildInfoController buildInfoController, IProjectSettingsController projectSettingsController) {
            _buildVariantsController = buildVariantsController;
            _buildInfoController = buildInfoController;
            _projectSettingsController = projectSettingsController;
        }

        #region Main Window
        
        public void Draw() {
            _frameAction = () => { };
            var inspectedBuildVariant = _buildVariantsController.BuildVariants.
                FirstOrDefault(buildVariant => buildVariant.Guid == _buildInfoController.BuildInfo.SelectedVariantGuid);

            if (inspectedBuildVariant == null) return;
            var isInspectedVariantActive = inspectedBuildVariant.Guid == _buildInfoController.BuildInfo.ActiveVariantGuid;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !isInspectedVariantActive;
            if (GUILayout.Button("Make active")) {
                _buildInfoController.ActivateBuildVariant(inspectedBuildVariant);
            }
            if (GUILayout.Button("Remove")) {
                _frameAction += () => _buildVariantsController.RemoveVariant(inspectedBuildVariant);
            }
            GUI.enabled = true;
            if (GUILayout.Button("Create child")) {
                _buildVariantsController.CreateVariant(inspectedBuildVariant);
            }

            EditorGUILayout.EndHorizontal();
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.Space();
            inspectedBuildVariant.VariantName =  EditorGUILayout.TextField("Variant name:", inspectedBuildVariant.VariantName);
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("GUID:");
            EditorGUILayout.LabelField(inspectedBuildVariant.Guid);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            var possibleParents = _buildVariantsController.BuildVariants.Where(v => v != inspectedBuildVariant && 
                                                                                   v.Parent != inspectedBuildVariant).
                Select(v => new GUIContent(string.Format("{0} ({1})", v.VariantName, v.Guid), v.Guid)).ToList();
            possibleParents.Insert(0, new GUIContent("No parent", ""));
            var selectedParent = -1;
            for (var i = 0; i < possibleParents.Count; i++) {
                if (possibleParents[i].tooltip != 
                    (inspectedBuildVariant.Parent == null ? "" : inspectedBuildVariant.Parent.Guid)) continue;
                selectedParent = i;
                break;
            }
            var newSelectedParent = EditorGUILayout.Popup(new GUIContent("Parent:"), 
                selectedParent, 
                possibleParents.ToArray());
            if (newSelectedParent != selectedParent) {
                _buildVariantsController.SetVariantParent(inspectedBuildVariant, possibleParents[newSelectedParent].tooltip);
            }
               
            EditorGUILayout.Space();
            inspectedBuildVariant.BuildPath =  EditorGUILayout.TextField("Build path:", inspectedBuildVariant.BuildPath);
            
            EditorGUILayout.Space();
            inspectedBuildVariant.MakeZip = EditorGUILayout.Toggle("Make zip:", inspectedBuildVariant.MakeZip);
            
            EditorGUILayout.Space();
            var nonObsoleteBuildTargetFields = typeof (BuildTarget)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(fieldInfo => !Attribute.IsDefined(fieldInfo, typeof(ObsoleteAttribute)));
            var nonObsoleteEnumValues = nonObsoleteBuildTargetFields
                .Select(fieldInfo => ((BuildTarget)fieldInfo.GetValue(null)).ToString()).ToArray();
            var buildTargetIndex = Array.IndexOf(nonObsoleteEnumValues, Enum.GetName(typeof(BuildTarget), 
                inspectedBuildVariant.BuildTarget));
            var newBuildTargetIndex = EditorGUILayout.Popup("Build target", buildTargetIndex, nonObsoleteEnumValues);
            if (newBuildTargetIndex != buildTargetIndex) {
                inspectedBuildVariant.BuildTarget =
                    (BuildTarget) Enum.Parse(typeof(BuildTarget), nonObsoleteEnumValues[newBuildTargetIndex]);
            }
            if (newBuildTargetIndex != buildTargetIndex) {
                inspectedBuildVariant.BuildTarget =
                    (BuildTarget) Enum.Parse(typeof(BuildTarget), nonObsoleteEnumValues[newBuildTargetIndex]);
            }

            EditorGUILayout.Space();
            inspectedBuildVariant.BuildOptions =
                (BuildOptions) EditorGUILayout.EnumMaskPopup("Build options:", inspectedBuildVariant.BuildOptions);
            
            EditorGUILayout.Space();
            DrawFilesList(inspectedBuildVariant, inspectedBuildVariant.MoveFiles);
            
            EditorGUILayout.Space();
            DrawSettings(inspectedBuildVariant,
                isInspectedVariantActive ? SettingsCategory.ActualRevertableSettingsDiff : SettingsCategory.ActualSettingsDiff);
            
            EditorGUILayout.Space();
            DrawSettings(inspectedBuildVariant, SettingsCategory.VariantDiff);

            _frameAction();

            if (EditorGUI.EndChangeCheck()) {
                _buildVariantsController.SaveVariant(inspectedBuildVariant);
            }
            
            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Files list

        private void DrawFilesList(IBuildVariant buildVariant, ICollection<FileMoveInfo> filesList) {
            EditorGUILayout.BeginHorizontal();
            buildVariant.SetFieldExpanded("MoveFiles", 
                EditorGUILayout.Foldout(buildVariant.IsFieldExpanded("MoveFiles"), 
                    string.Format("{0} ({1})", "Move files",
                        filesList.Count)));
            if (GUILayout.Button("+", GUILayout.Width(30))) {
                filesList.Add(new FileMoveInfo());
            }
            EditorGUILayout.EndHorizontal();
            if (!buildVariant.IsFieldExpanded("MoveFiles")) return;
            EditorGUI.indentLevel++;
            foreach (var copyFileInfo in filesList) {
                DrawFile(filesList, copyFileInfo);
                EditorGUILayout.Space();
            }
            EditorGUI.indentLevel--;
        }

        private void DrawFile(ICollection<FileMoveInfo> filesList, FileMoveInfo fileMoveInfo) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("From:", GUILayout.Width(49));
            fileMoveInfo.From = EditorGUILayout.TextField("", fileMoveInfo.From);
            EditorGUILayout.LabelField("To:", GUILayout.Width(35));
            fileMoveInfo.To = EditorGUILayout.TextField("", fileMoveInfo.To);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            fileMoveInfo.PerformOnStage = (BuildStage)EditorGUILayout.EnumPopup("Perform on stage:", fileMoveInfo.PerformOnStage);
            if (fileMoveInfo.PerformOnStage == BuildStage.BeforeBuild) {
                fileMoveInfo.Revert = EditorGUILayout.Toggle("Revert", fileMoveInfo.Revert);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(30))) {
                _frameAction += () => filesList.Remove(fileMoveInfo);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Configuration setting
        
        private void DrawSettings(IBuildVariant inspectedBuildVariant, SettingsCategory category) {
            string categoryName;
            Action revertAction = null;
            Action saveAction = null;
            IEnumerable<IProjectSettingsFile> settingsFiles;
            
            EditorGUILayout.BeginHorizontal();
            if (category == SettingsCategory.ActualSettingsDiff || category == SettingsCategory.ActualRevertableSettingsDiff) {
                categoryName = "Actual project settings diff";
                settingsFiles = _projectSettingsController.GetDiffWithActualSettings(inspectedBuildVariant);
                saveAction = () => {
                    inspectedBuildVariant.Merge(settingsFiles);
                    _projectSettingsController.InvalidateDiffCache();
                };
                if (category == SettingsCategory.ActualRevertableSettingsDiff)
                    revertAction = () => _projectSettingsController.BuildAndApplyProjectSettings(inspectedBuildVariant);
            } else if (category == SettingsCategory.VariantDiff) {
                categoryName = "Variant settings";
                settingsFiles = inspectedBuildVariant.SettingsFileDiffs;
                revertAction = () => {
                    var currentDiff = _projectSettingsController.GetDiffWithActualSettings(inspectedBuildVariant).ToList();
                    inspectedBuildVariant.Revert();
                    _projectSettingsController.BuildAndApplyProjectSettings(inspectedBuildVariant, currentDiff);
                };
            } else {
                throw new ArgumentOutOfRangeException("category", category, null);
            }

            var expanded = inspectedBuildVariant.IsFieldExpanded(category.ToString());
            var projectSettingsFiles = settingsFiles as IProjectSettingsFile[] ?? settingsFiles.ToArray();
            expanded = EditorGUILayout.Foldout(expanded, string.Format("{0} ({1})", categoryName, projectSettingsFiles.Length));
            inspectedBuildVariant.SetFieldExpanded(category.ToString(), expanded);

            GUI.enabled = projectSettingsFiles.Length > 0;
            DrawSaveRevert(revertAction, saveAction);
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (!expanded) return;
            
            foreach (var settingsFileDiff in projectSettingsFiles) {
                var mappingNode = settingsFileDiff.RootNode as YamlMappingNode;
                if (mappingNode != null) {
                    foreach (var keyValue in mappingNode.Children) {
                        DrawNode(inspectedBuildVariant, settingsFileDiff, category, keyValue.Value, keyValue.Key);
                    }   
                } else {
                    DrawNode(inspectedBuildVariant, settingsFileDiff, category, settingsFileDiff.RootNode);
                }
            }
        }

        private void DrawNode(IBuildVariant inspectedBuildVariant,
            IProjectSettingsFile projectSettingsFile, SettingsCategory category, YamlNode node, YamlNode labelNode = null) {
            if (node == null) return;
            EditorGUI.indentLevel++;
            
            Action revertAction = null;
            Action saveAction = null;
            if (category == SettingsCategory.ActualSettingsDiff || category == SettingsCategory.ActualRevertableSettingsDiff) {
                saveAction = () => {
                    inspectedBuildVariant.Merge(projectSettingsFile, node);
                    _projectSettingsController.InvalidateDiffCache();
                };
                if (category == SettingsCategory.ActualRevertableSettingsDiff)
                    revertAction = () => {
                        _projectSettingsController.BuildAndApplyProjectSettings(inspectedBuildVariant,
                            _projectSettingsController.GetDiffWithActualSettings(inspectedBuildVariant),
                            projectSettingsFile, node);
                    };
            } else if (category == SettingsCategory.VariantDiff) {
                revertAction = () => {
                    var currentDiff = _projectSettingsController.GetDiffWithActualSettings(inspectedBuildVariant).ToList();
                    inspectedBuildVariant.Revert(projectSettingsFile, node);
                    _projectSettingsController.BuildAndApplyProjectSettings(inspectedBuildVariant, currentDiff);
                };
            }

            if (node.NodeType == YamlNodeType.Mapping) {
                EditorGUILayout.BeginHorizontal();
                var expanded = inspectedBuildVariant.IsFieldExpanded(string.Format("{0}_{1}_{2}",
                    category.ToString(), projectSettingsFile.FileName, labelNode));

                expanded = EditorGUILayout.Foldout(expanded, string.Format("{0} ({1})",
                    labelNode, ((YamlMappingNode) node).Children.Count));
                inspectedBuildVariant.SetFieldExpanded(string.Format("{0}_{1}_{2}",
                    category.ToString(), projectSettingsFile.FileName, labelNode), expanded);
                DrawSaveRevert(revertAction, saveAction);
                EditorGUILayout.EndHorizontal();
                if (expanded) {
                    foreach (var keyValue in ((YamlMappingNode) node).Children) {
                        DrawNode(inspectedBuildVariant, projectSettingsFile, category, keyValue.Value, keyValue.Key);
                    }
                }
            } else if (node.NodeType == YamlNodeType.Scalar || node.NodeType == YamlNodeType.Sequence) {
                EditorGUILayout.BeginHorizontal();
                if (labelNode != null) EditorGUILayout.LabelField(labelNode.ToString());
                EditorGUILayout.HelpBox(node.ToString(), MessageType.None);
                DrawSaveRevert(revertAction, saveAction);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUI.indentLevel--;
        }

        private void DrawSaveRevert(Action revertAction, Action saveAction) {
            if (saveAction != null && GUILayout.Button("Save", GUILayout.Width(PropertiesSaveRevertButtonWidth))) {
                _frameAction += saveAction;
            }
            if (revertAction != null && GUILayout.Button("Revert", GUILayout.Width(PropertiesSaveRevertButtonWidth))) {
                _frameAction += revertAction;
            }
        }
        
        #endregion
    }
}