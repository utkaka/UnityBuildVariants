using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BuildVariants.View.Explorer {
    public class VariantCollectionNameDialog : PopupWindowContent {
        private readonly IEnumerable<string> _existingNames;
        private string _collectionName;
        private Action<string> _closeAction;

        public VariantCollectionNameDialog(string collectionName, IEnumerable<string> existingNames, Action<string> closeAction) {
            _collectionName = collectionName;
            _existingNames = existingNames;
            _closeAction = closeAction;
        }

        public override void OnGUI(Rect rect) {
            EditorGUILayout.BeginHorizontal();
            _collectionName = EditorGUILayout.TextField(_collectionName);
            if (_existingNames.Contains(_collectionName)) {
                GUI.enabled = false;
            }
            if (GUILayout.Button("Ok")) {
                _closeAction(_collectionName);
                editorWindow.Close();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
    }
}