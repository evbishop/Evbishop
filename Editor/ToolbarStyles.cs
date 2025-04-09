using UnityEngine;

namespace Evbishop.Editor
{
    public static class ToolbarStyles
    {
        public static readonly GUIStyle CommandButtonStyle;

        static ToolbarStyles()
        {
            CommandButtonStyle = DefaultButtonStyle();
        }

        static GUIStyle DefaultButtonStyle()
        {
            return new GUIStyle("Command")
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Normal
            };
        }
    }
}
