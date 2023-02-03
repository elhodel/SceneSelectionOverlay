using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace elhodel.SceneSelectionOverlay
{
    public static class Extensions
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

        /// <summary>
        /// Order <paramref name="source"/> alphabetically using Natural Order Numerical Sorting
        /// Source: https://stackoverflow.com/a/11720793
        /// </summary>
        public static IOrderedEnumerable<T> OrderByAlphaNumeric<T>(this IEnumerable<T> source, Func<T, string> selector)
        {
            int max = source
                .SelectMany(i => Regex.Matches(selector(i), @"\d+").Cast<Match>().Select(m => (int?)m.Value.Length))
                .Max() ?? 0;

            return source.OrderBy(i => Regex.Replace(selector(i), @"\d+", m => m.Value.PadLeft(max, '0')));
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || list.Count() == 0;
        }
    }
}