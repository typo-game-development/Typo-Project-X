using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CustomAssets.Utilities.Design
{
    public static class IconDrawer
    {
        public static void DrawLabelIcon(GameObject gameObject, int idx)
        {
            GUIContent[] largeIcons = GetTextures("sv_label_", string.Empty, 0, 8);
            GUIContent icon = largeIcons[idx];
            Type egu = typeof(EditorGUIUtility);
            BindingFlags flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
            object[] args = new object[] { gameObject, icon.image };
            MethodInfo setIcon = egu.GetMethod("SetIconForObject", flags, null, new Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
            setIcon.Invoke(null, args);
        }

        public static void DrawDotBigIcon(GameObject gameObject, int idx)
        {
            GUIContent[] largeIcons = GetTextures("sv_icon_dot", "_pix16_gizmo", 0, 16);
            GUIContent icon = largeIcons[idx];
            Type egu = typeof(EditorGUIUtility);
            BindingFlags flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
            object[] args = new object[] { gameObject, icon.image };
            MethodInfo setIcon = egu.GetMethod("SetIconForObject", flags, null, new Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
            setIcon.Invoke(null, args);
        }
        public static GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
        {
            GUIContent[] array = new GUIContent[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = EditorGUIUtility.IconContent(baseName + (startIndex + i) + postFix);
            }
            return array;
        }
    }
}
