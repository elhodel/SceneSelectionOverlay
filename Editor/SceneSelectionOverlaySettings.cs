using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace elhodel.SceneSelectionOverlay
{
    [FilePath("ProjectSettings/SceneSelectionOverlaySettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class SceneSelectionOverlaySettings : ScriptableSingleton<SceneSelectionOverlaySettings>
    {
        void OnEnable()
        {
            /// Make ScriptableSingleton Editable, this is disabled by default
            /// https://forum.unity.com/threads/scriptablesingleton-disabled-serializedproperty-in-editor-window.1156826/#post-7421990
            hideFlags &= ~HideFlags.NotEditable;
        }

        [System.Serializable]
        public struct SceneGroup
        {
            [Tooltip("Name of Group. \nUse \"/\" to seperate nested Menus")]
            public string Name;
            [Tooltip("Regex Filter for the File Path excluding the FileName. Use \"/\" as Directory Seperator. Match is Case Insensitive")]
            public string FolderFilter;
            [Tooltip("Regex Filter for the File Name. Match is Case Insensitive")]
            public string FileNameFilter;
        }

        [Tooltip("How Scenes that are Currently added added as Scenes in Build should be displayed in Menu")]
        public ShowOption BuildScenesShowOption = ShowOption.Nested;

        [Tooltip("How Scenes that don't match any filters in SceneGroups should be displayed in Menu")]
        public ShowOption UngroupedScenesShowOption = ShowOption.Nested;

        [SerializeField]
        [Tooltip("Groups of Scenes that should be be added to Menu")]
        public List<SceneGroup> SceneGroups = new List<SceneGroup>();

        [Tooltip("How Favorite Scenes should be displayed in Menu")]
        public ShowOption FavoriteScenesShowOption = ShowOption.Flat;
        public List<SceneAsset> FavoriteScenes = new List<SceneAsset>();


        internal void Save()
        {
            Save(true);
        }


        public enum ShowOption
        {
            Hide,
            Flat,
            Nested,
        }
    }


    [CustomEditor(typeof(SceneSelectionOverlaySettings))]
    public class SceneSelectionOverlaySettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var iterator = serializedObject.GetIterator();

            iterator.NextVisible(true);

            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator);
            }
        }
    }
}