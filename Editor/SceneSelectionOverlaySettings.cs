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

        [Header("Favorites")]
        [Tooltip("How Favorite Scenes should be displayed in Menu")]
        public ShowOption FavoriteScenesShowOption = ShowOption.Flat;
        [Tooltip("Favorite Scenes that should be displayed at the Top of the Menu")]
        public List<SceneAsset> FavoriteScenes = new List<SceneAsset>();

        [Header("Build Scenes")]
        [Tooltip("How Scenes that are Currently added added as Scenes in Build should be displayed in Menu")]
        public ShowOption BuildScenesShowOption = ShowOption.Nested;

        [Tooltip("Prefix Build Index in the Menu to the Scene Name")]
        public bool AddBuildIndex = true;

        [Header("Scene Groups")]
        [SerializeField]
        [Tooltip("Groups of Scenes that should be be added to Menu")]
        public List<SceneGroup> SceneGroups = new List<SceneGroup>();


        [Header("Other Scenes")]
        [Tooltip("How Scenes that don't match any filters in SceneGroups should be displayed in Menu")]
        public ShowOption UngroupedScenesShowOption = ShowOption.Nested;

        internal void Save()
        {
            Save(true);
        }

        public void AddFavoriteScene(SceneAsset sceneToAdd)
        {
            if (FavoriteScenes.Contains(sceneToAdd))
            {
                return;
            }
            FavoriteScenes.Add(sceneToAdd);
            Save();
        }

        public void RemoveFavoriteScene(SceneAsset sceneToAdd)
        {
            if (FavoriteScenes.Contains(sceneToAdd))
            {
                FavoriteScenes.Remove(sceneToAdd);
                Save();
            }
        }

        public enum ShowOption
        {
            Hide,
            Flat,
            Nested,
        }


        private const string _addFavoriteMenuItemPath = "Assets/Add to Favorite";
        private const int _favoriteMenuItemOrder = 2000;
        [MenuItem(_addFavoriteMenuItemPath, false, _favoriteMenuItemOrder)]
        private static void SetAsFavoriteMenuItem()
        {
            SceneAsset sceneAsset = Selection.activeObject as SceneAsset;

            SceneSelectionOverlaySettings.instance.AddFavoriteScene(sceneAsset);
        }

        [MenuItem(_addFavoriteMenuItemPath, true)]
        private static bool SetAsFavoriteMenuItemValidator()
        {
            SceneAsset sceneAsset = Selection.activeObject as SceneAsset;
            return sceneAsset != null && !SceneSelectionOverlaySettings.instance.FavoriteScenes.Contains(sceneAsset);
        }


        private const string _removeFavoriteMenuItemPath = "Assets/Remove from Favorite";

        [MenuItem(_removeFavoriteMenuItemPath, false, _favoriteMenuItemOrder + 1)]
        private static void RemoveFavoriteMenuItem()
        {
            SceneAsset sceneAsset = Selection.activeObject as SceneAsset;

            SceneSelectionOverlaySettings.instance.RemoveFavoriteScene(sceneAsset);
        }

        [MenuItem(_removeFavoriteMenuItemPath, true)]
        private static bool RemoveFavoriteMenuItemValidator()
        {
            SceneAsset sceneAsset = Selection.activeObject as SceneAsset;
            return sceneAsset != null && SceneSelectionOverlaySettings.instance.FavoriteScenes.Contains(sceneAsset);
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