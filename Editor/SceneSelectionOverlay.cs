using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEditor.Overlays;
using System.Linq;
using System.IO;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;
using System;

namespace elhodel.SceneSelectionOverlay
{
    [Overlay(typeof(SceneView), "Scene Selection")]
    [Icon(IconPath)]
    public class SceneSelectionOverlay : ToolbarOverlay
    {
        public const string IconPath = "Packages/ch.elhodel.scene-selection-overlay/Editor/Icons/SceneAssetIcon.png";
        public const string ProIconPath = "Packages/ch.elhodel.scene-selection-overlay/Editor/Icons/d_SceneAssetIcon.png";

        public static string SkinnedIconPath => EditorGUIUtility.isProSkin ? ProIconPath : IconPath;

        SceneSelectionOverlay() : base(SceneSelectionDropdownToggle.Id)
        { }

        [EditorToolbarElement(Id, typeof(SceneView))]
        class SceneSelectionDropdownToggle : EditorToolbarDropdown, IAccessContainerWindow
        {
            public const string Id = "SceneSelectionOverlay/DropdownToggle";
            public const string OtherSceneFolderName = "Others";
            public const string BuildSceneFolderName = "Build";

            private class SceneMenuItem
            {
                public string ScenePath { get; private set; }
                public string MenuItemPath { get; private set; }
                public string SceneName { get; private set; }

                public string UniqueMenuItemName { get; set; }

                public string CompleteMenuPath => MenuItemPath + UniqueMenuItemName;
                public SceneMenuItem(string scenePath, string menuItemPath)
                {
                    ScenePath = scenePath;
                    MenuItemPath = menuItemPath;
                    SceneName = Path.GetFileNameWithoutExtension(scenePath);
                    UniqueMenuItemName = SceneName;
                }
            }

            private class SceneMenuData
            {
                private Dictionary<string, List<SceneMenuItem>> _menuPathSceneMap = new();

                public void AddItem(string key, SceneMenuItem sceneMenuItem)
                {
                    if (!_menuPathSceneMap.ContainsKey(key))
                    {
                        _menuPathSceneMap.Add(key, new List<SceneMenuItem>());
                    }

                    _menuPathSceneMap[key].Add(sceneMenuItem);
                }

                public void AddItem(string key, string scenePath, string menuItemPath)
                {
                    SceneMenuItem sceneMenuItem = new SceneMenuItem(scenePath, menuItemPath);
                    AddItem(key, sceneMenuItem);
                }

                public void GenerateUniqueMenuPaths()
                {
                    foreach (var kvp in _menuPathSceneMap)
                    {
                        var duplicates = kvp.Value.GroupBy(e => e.SceneName).Where(g => g.Count() > 1);
                        foreach (var duplicateGroup in duplicates)
                        {
                            var list = duplicateGroup.ToList();
                            for (int i = 0; i < list.Count; i++)
                            {
                                SceneMenuItem item = list[i];
                                item.UniqueMenuItemName = item.ScenePath.Replace('/', '\\');
                                list[i] = item;
                            }
                        }
                    }
                }

                private void SortScenes()
                {
                    _menuPathSceneMap = _menuPathSceneMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.OrderBy(v => v.SceneName).ToList());
                }

                private void CreateMenuEntry(GenericMenu menu, string key, string currentScenePath, Action<string> onClickCallback, bool addSparatorBefore, bool addSeparatorAfter)
                {
                    if (!_menuPathSceneMap.ContainsKey(key))
                    {
                        return;
                    }

                    if (addSparatorBefore)
                    {
                        menu.AddSeparator("");

                    }

                    foreach (var scene in _menuPathSceneMap[key])
                    {
                        bool isCurrentScene = currentScenePath == scene.ScenePath;
                        menu.AddItem(scene.CompleteMenuPath, isCurrentScene, () => onClickCallback(scene.ScenePath), !isCurrentScene);
                    }
                    if (addSeparatorAfter)
                    {

                        menu.AddSeparator("");
                    }


                }


                public GenericMenu CreateMenu(string currentScenePath, Action<string> onClickCallback)
                {
                    GenericMenu menu = new GenericMenu();

                    SortScenes();

                    CreateMenuEntry(menu, BuildSceneFolderName, currentScenePath, onClickCallback, false, true);

                    foreach (string sceneGroupName in SceneSelectionOverlaySettings.instance.SceneGroups.Select(g => g.Name))
                    {
                        CreateMenuEntry(menu, sceneGroupName, currentScenePath, onClickCallback, false, false);
                    }

                    CreateMenuEntry(menu, OtherSceneFolderName, currentScenePath, onClickCallback, true, false);

                    return menu;
                }
            }

            public EditorWindow containerWindow { get; set; }

            SceneSelectionDropdownToggle()
            {
                text = "Scene Selection";
                tooltip = "Switch to the selected Scene";

                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(SkinnedIconPath);

                clicked += ShowSceneMenu;
            }

            private SceneMenuData BuildSceneMenuData()
            {
                SceneMenuData sceneMenuData = new();

                var menu = new GenericMenu();
                if (SceneSelectionOverlaySettings.instance.BuildScenesShowOption != SceneSelectionOverlaySettings.ShowOption.Hide)
                {
                    foreach (var builderScene in EditorBuildSettings.scenes)
                    {
                        string menuItemPath = "";

                        if (SceneSelectionOverlaySettings.instance.BuildScenesShowOption == SceneSelectionOverlaySettings.ShowOption.Nested)
                        {
                            menuItemPath = BuildSceneFolderName + "/";
                        }

                        sceneMenuData.AddItem(BuildSceneFolderName, builderScene.path, menuItemPath);
                    }
                }


                var scenePaths = AssetDatabase.FindAssets("t:Scene").Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();

                foreach (var scenePath in scenePaths)
                {

                    var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                    var pathWithoutName = Path.GetDirectoryName(scenePath);
                    /// Convert DirectorySeperator to forward Slash so the Regex filters can be Platform independent.
                    pathWithoutName = pathWithoutName.Replace(Path.DirectorySeparatorChar, '/');
                    bool isInGroup = false;
                    foreach (var sceneGroup in SceneSelectionOverlaySettings.instance.SceneGroups)
                    {
                        bool pathMatches = string.IsNullOrEmpty(sceneGroup.FolderFilter) || Regex.IsMatch(pathWithoutName, sceneGroup.FolderFilter, RegexOptions.IgnoreCase);

                        bool nameMatches = string.IsNullOrEmpty(sceneGroup.FileNameFilter) || Regex.IsMatch(sceneName, sceneGroup.FileNameFilter, RegexOptions.IgnoreCase);
                        if (pathMatches && nameMatches)
                        {

                            string menuPath = $"{sceneGroup.Name}/";

                            sceneMenuData.AddItem(sceneGroup.Name, scenePath, menuPath);

                            isInGroup = true;
                        }

                    }

                    if (!isInGroup && SceneSelectionOverlaySettings.instance.UngroupedScenesShowOption != SceneSelectionOverlaySettings.ShowOption.Hide)
                    {
                        string menuPath = "";

                        if (SceneSelectionOverlaySettings.instance.UngroupedScenesShowOption == SceneSelectionOverlaySettings.ShowOption.Nested)
                        {
                            menuPath = OtherSceneFolderName + "/";
                        }

                        sceneMenuData.AddItem(OtherSceneFolderName, scenePath, menuPath);
                    }
                }

                return sceneMenuData;
            }


            // When the dropdown button is clicked, this method will create a popup menu at the mouse cursor position.
            private void ShowSceneMenu()
            {
                var currentScenePath = EditorSceneManager.GetActiveScene().path;

                var sceneMenuData = BuildSceneMenuData();

                sceneMenuData.GenerateUniqueMenuPaths();

                GenericMenu menu = sceneMenuData.CreateMenu(currentScenePath, OpenScene);

                menu.ShowAsContext();
            }

            private void OpenScene(string scenePath)
            {
                var currentScene = EditorSceneManager.GetActiveScene();
                if (currentScene.isDirty)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scenePath);

                    }
                }
                else
                {
                    EditorSceneManager.OpenScene(scenePath);
                }

            }
        }



    }
}