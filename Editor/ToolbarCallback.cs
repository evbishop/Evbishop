using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evbishop.Editor
{
    public static class ToolbarCallback
    {
        private static ScriptableObject currentToolbar;
        private static Type toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");

        public static Action OnToolbarGUILeft;
        public static Action OnToolbarGUIMiddle;
        public static Action OnToolbarGUIRight;

        static ToolbarCallback()
        {
            EditorApplication.update -= OnToolbarUpdate;
            EditorApplication.update += OnToolbarUpdate;
        }

        private static void OnToolbarUpdate()
        {
            if (currentToolbar == null)
            {
                var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
                currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;

                if (currentToolbar != null)
                {
                    FieldInfo root = currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    VisualElement visualElementRoot = root.GetValue(currentToolbar) as VisualElement;

                    RegisterCallback("ToolbarZoneLeftAlign", OnToolbarGUILeft);
                    RegisterPlayModeCallback(visualElementRoot, OnToolbarGUIMiddle);
                    RegisterCallback("ToolbarZoneRightAlign", OnToolbarGUIRight);
                }
            }
        }

        private static void RegisterPlayModeCallback(VisualElement root, Action action)
        {
            if (action == null) return;

            var toolbarZone = root.Q("ToolbarZonePlayMode");
            if (toolbarZone == null) return;

            var container = new IMGUIContainer();
            container.style.flexGrow = 0;
            container.style.flexShrink = 0;
            container.style.marginLeft = 10;
            container.onGUIHandler = () =>
            {
                var content = EditorGUIUtility.GetMainWindowPosition();
                using (new GUILayout.HorizontalScope())
                {
                    action.Invoke();
                }
            };

            toolbarZone.Add(container);
        }

        private static void RegisterCallback(string rootName, Action action)
        {
            if (action == null) return;

            VisualElement toolbarZone = currentToolbar.GetType()
                .GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(currentToolbar) as VisualElement;

            if (toolbarZone != null)
            {
                var zone = toolbarZone.Q(rootName);
                if (zone != null)
                {
                    var container = new IMGUIContainer();
                    container.style.flexGrow = 1;
                    container.onGUIHandler = () =>
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            action.Invoke();
                        }
                    };
                    zone.Add(container);
                }
            }
        }
    }
}