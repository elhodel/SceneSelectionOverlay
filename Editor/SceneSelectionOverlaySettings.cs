using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace elhodel.SceneSelectionOverlay
{
    [FilePath("ProjectSettings/SceneSelectionOverlaySettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class SceneSelectionOverlaySettings : ScriptableSingleton<SceneSelectionOverlaySettings>
    {
        #region Class and Enum Definitions
        public enum ShowOption
        {
            Hide,
            Flat,
            Nested,
        }

        [System.Serializable]
        public struct SceneGroup
        {
            [Tooltip("Name of Group. \nUse \"/\" to seperate Submenus")]
            public string Name;
            [Tooltip("Regex Filter for the File Path excluding the FileName. \n" +
                "Use \"/\" as Directory Seperator. Match is Case Insensitive \n" +
                "Use an exclamation Mark '!' as the first character to invert the Filter")]
            public string FolderFilter;
            [Tooltip("Regex Filter for the File Name. Match is Case Insensitive \n" +
                "Use an exclamation Mark '!' as the first character to invert the Filter")]
            public string FileNameFilter;

           

            public bool CheckFolderForMatch(string path)
            {
                return CheckForMatch(path, FolderFilter);

            }

            public bool CheckNameForMatch(string name)
            {
                return CheckForMatch(name, FileNameFilter);
            }

            private bool CheckForMatch(string stringToTest, string filter)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    return true;
                }

                if (IsFilterNegated(filter, out string cleanFilter))
                {
                    return !Regex.IsMatch(stringToTest, cleanFilter, RegexOptions.IgnoreCase);
                }

                return Regex.IsMatch(stringToTest, filter, RegexOptions.IgnoreCase);
            }

            private bool IsFilterNegated(string filter, out string cleanFilter)
            {
                if (filter.StartsWith('!'))
                {
                    cleanFilter = filter.TrimStart('!');
                    return true;
                }
                cleanFilter = null;
                return false;
            }

        }

        #endregion

        #region Serialized Fields

        [Header("Favorites")]
        [SerializeField]
        [Tooltip("How Favorite Scenes should be displayed in Menu")]
        private ShowOption _favoriteScenesShowOption = ShowOption.Flat;

        [Tooltip("Favorite Scenes that should be displayed at the Top of the Menu")]
        [SerializeField]
        private List<SceneAsset> _favoriteScenes = new List<SceneAsset>();

        [Header("Build Scenes")]
        [Tooltip("How Scenes that are Currently added added as Scenes in Build should be displayed in Menu")]
        [SerializeField]
        private ShowOption _buildScenesShowOption = ShowOption.Nested;

        [Tooltip("Prefix Build Index in the Menu to the Scene Name")]
        [SerializeField]
        private bool _doAddBuildIndex = true;

        [Header("Scene Groups")]
        [SerializeField]
        [Tooltip("Groups of Scenes that should be be added to Menu")]
        private List<SceneGroup> _sceneGroups = new List<SceneGroup>();

        [Header("Other Scenes")]
        [Tooltip("How Scenes that don't match any filters in SceneGroups should be displayed in Menu")]
        [SerializeField]
        private ShowOption _ungroupedScenesShowOption = ShowOption.Nested;

        #endregion

        #region Properties

        /// <summary>
        /// How Favorite Scenes should be displayed in Menu
        /// </summary>
        public ShowOption FavoriteScenesShowOption
        {
            get { return _favoriteScenesShowOption; }
            set
            {
                _favoriteScenesShowOption = value;
                Save();
            }
        }

        /// <summary>
        /// Get Favorite Scenes that should be displayed at the Top of the Menu (as ReadOnly)
        /// </summary>
        public IReadOnlyList<SceneAsset> FavoriteScenes => _favoriteScenes;

        /// <summary>
        /// How Scenes that are Currently added added as Scenes in Build should be displayed in Menu
        /// </summary>
        public ShowOption BuildScenesShowOption
        {
            get { return _buildScenesShowOption; }
            set
            {
                _buildScenesShowOption = value;
                Save();
            }
        }

        /// <summary>
        /// Whether to Prefix Build Index in the Menu to the Scene Name
        /// </summary>
        public bool DoAddBuildIndex
        {
            get { return _doAddBuildIndex; }
            set
            {
                _doAddBuildIndex = value;
                Save();
            }
        }

        /// <summary>
        /// Groups of Scenes that should be be added to Menu
        /// </summary>
        public IReadOnlyList<SceneGroup> SceneGroups => _sceneGroups;

        /// <summary>
        /// How Scenes that don't match any filters in SceneGroups should be displayed in Menu
        /// </summary>
        public ShowOption UngroupedScenesShowOption
        {
            get { return _ungroupedScenesShowOption; }
            set
            {
                _ungroupedScenesShowOption = value;
                Save();
            }
        }

        #endregion

        #region Public Methodes

        /// <summary>
        /// Add a Scene to the Favorite Scenes
        /// </summary>
        /// <param name="sceneToAdd">The SceneAsset of the Scene that should be Added</param>
        public void AddFavoriteScene(SceneAsset sceneToAdd)
        {
            if (_favoriteScenes.Contains(sceneToAdd))
            {
                return;
            }
            _favoriteScenes.Add(sceneToAdd);
            Save();
        }

        /// <summary>
        /// Remove a Scene from the Favorite Scenes
        /// </summary>
        /// <param name="sceneToAdd">The SceneAsset of the Scene that should be Removed</param>
        public void RemoveFavoriteScene(SceneAsset sceneToRemove)
        {
            if (_favoriteScenes.Contains(sceneToRemove))
            {
                _favoriteScenes.Remove(sceneToRemove);
                Save();
            }
        }

        /// <summary>
        /// Clear all Scenes from the Favorited Scenes
        /// </summary>
        public void ClearFavoriteScenes()
        {
            _favoriteScenes.Clear();
            Save();
        }

        /// <summary>
        /// Add a <see cref="SceneGroup"/> to create a new Group in the Menu.
        /// Duplicates are not allowed and are ignored
        /// </summary>
        /// <param name="sceneGroup"><see cref="SceneGroup"/> to add</param>
        public void AddSceneGroup(SceneGroup sceneGroup)
        {
            if (_sceneGroups.Contains(sceneGroup))
            {
                return;
            }
            _sceneGroups.Add(sceneGroup);
            Save();
        }

        /// <summary>
        /// Remove the <see cref="SceneGroup"/> with the given Name from the Menu
        /// </summary>
        /// <param name="sceneGroupName">Name of the <see cref="SceneGroup"/> to remove</param>
        public void RemoveSceneGroup(string sceneGroupName)
        {
            int indexToRemove = _sceneGroups.FindIndex(g => g.Name == sceneGroupName);
            if (indexToRemove < 0)
            {
                return;
            }
            _sceneGroups.RemoveAt(indexToRemove);
            Save();
        }

        /// <summary>
        /// Change a <see cref="SceneGroup"/> to a new one
        /// </summary>
        /// <param name="sceneGroupToEdit">>Name of the <see cref="SceneGroup"/> to remove</param>
        /// <param name="group"></param>
        public void ChangeSceneGroup(string sceneGroupToEdit, SceneGroup group)
        {
            int index = _sceneGroups.FindIndex(g => g.Name == sceneGroupToEdit);
            if (index < 0)
            {
                return;
            }
            _sceneGroups[index] = group;
            Save();
        }

        /// <summary>
        /// Clear all Scene Group
        /// </summary>
        public void ClearAllSceneGroups()
        {
            _sceneGroups.Clear();
            Save();
        }

        #endregion

        #region Internal and Private Methodes

        internal void EnableEditing()
        {
            /// Make ScriptableSingleton Editable, this is disabled by default
            /// https://forum.unity.com/threads/scriptablesingleton-disabled-serializedproperty-in-editor-window.1156826/#post-7421990
            hideFlags &= ~HideFlags.NotEditable;
        }

        internal void Save()
        {
            Save(true);
        }

        private void OnEnable()
        {
            EnableEditing();
        }

        #endregion
    }

    internal static class SceneSelectionOverlayContextMenus
    {
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
    internal class SceneSelectionOverlaySettingsEditor : Editor
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