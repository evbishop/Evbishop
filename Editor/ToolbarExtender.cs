using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Evbishop.Editor
{
    [InitializeOnLoad]
    public static class ToolbarExtender
    {
        public static readonly List<Action> LeftToolbarGUI = new();
        public static readonly List<Action> MiddleToolbarGUI = new();
        public static readonly List<Action> RightToolbarGUI = new();

        static ToolbarExtender()
        {
            ToolbarCallback.OnToolbarGUILeft -= GUILeft;
            ToolbarCallback.OnToolbarGUILeft += GUILeft;

            ToolbarCallback.OnToolbarGUIMiddle -= GUIMiddle;
            ToolbarCallback.OnToolbarGUIMiddle += GUIMiddle;

            ToolbarCallback.OnToolbarGUIRight -= GUIRight;
            ToolbarCallback.OnToolbarGUIRight += GUIRight;
        }

        private static void GUILeft()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in LeftToolbarGUI)
            {
                handler?.Invoke();
            }
            GUILayout.EndHorizontal();
        }

        private static void GUIMiddle()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in MiddleToolbarGUI)
            {
                handler?.Invoke();
            }
            GUILayout.EndHorizontal();
        }

        private static void GUIRight()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in RightToolbarGUI)
            {
                handler?.Invoke();
            }
            GUILayout.EndHorizontal();
        }
    }
}
