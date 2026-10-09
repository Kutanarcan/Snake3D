using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    // Styles the tool builds once. EditorStyles only exists while the GUI is
    // running, so these cannot be field initialisers.
    internal static class LevelEditorStyles
    {
        private static GUIStyle _centeredLabel;

        internal static GUIStyle CenteredLabel
        {
            get
            {
                if (_centeredLabel == null)
                {
                    _centeredLabel = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                return _centeredLabel;
            }
        }
    }
}
