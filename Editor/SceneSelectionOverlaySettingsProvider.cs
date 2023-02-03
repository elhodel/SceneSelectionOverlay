using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace elhodel.SceneSelectionOverlay
{
    internal class SceneSelectionOverlaySettingsProvider : SettingsProvider
    {
        public const string Path = "Tools/Scene Selection Overlay";

        private static Editor _editor;

        public SceneSelectionOverlaySettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            Undo.undoRedoPerformed += SaveUndo;
            SceneSelectionOverlaySettings.instance.EnableEditing();
        }

        private void SaveUndo()
        {
            if (EditorUtility.IsDirty(SceneSelectionOverlaySettings.instance))
            {
                SceneSelectionOverlaySettings.instance.Save();
            }
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            _editor.serializedObject.Update();

            _editor.OnInspectorGUI();

            if (_editor.serializedObject.ApplyModifiedProperties())
            {
                SceneSelectionOverlaySettings.instance.Save();
            }

        }



        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            _editor = Editor.CreateEditor(SceneSelectionOverlaySettings.instance);
            var keywords = GetSearchKeywordsFromSerializedObject(_editor.serializedObject);
            return new SceneSelectionOverlaySettingsProvider(Path, SettingsScope.Project, keywords);
        }

    }
}