using UnityEngine;
using UnityEditor;

namespace elhodel.SceneSelectionOverlay
{
    public static class GenericMenuExtensions
    {
        public static void AddItem(this GenericMenu menu, string path, bool on, GenericMenu.MenuFunction func, bool isEnabled)
        {
            if (isEnabled)
            {
                menu.AddItem(new GUIContent(path), on, func);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(path), on);
            }
        }

    }
}