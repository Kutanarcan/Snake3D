using Game.Authoring;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    // The row of brushes. It asks LevelBrush what there is to offer rather than
    // keeping a list of its own, so a new item type appears here on its own.
    internal static class PaletteBar
    {
        private const float SwatchWidth = 74f;
        private const float SwatchHeight = 22f;

        internal static LevelBrush Draw(LevelBrush chosen)
        {
            GUILayout.BeginHorizontal();

            foreach (LevelBrush brush in LevelBrush.All())
            {
                if (Swatch(brush, brush.MovesTheStart == chosen.MovesTheStart && brush.Type == chosen.Type))
                {
                    chosen = brush;
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            return chosen;
        }

        private static bool Swatch(LevelBrush brush, bool isChosen)
        {
            Rect at = GUILayoutUtility.GetRect(SwatchWidth, SwatchHeight, GUILayout.ExpandWidth(false));

            if (Event.current.type == EventType.Repaint)
            {
                Paint(at, brush, isChosen);
            }

            return GUI.Button(at, GUIContent.none, GUIStyle.none);
        }

        private static void Paint(Rect at, LevelBrush brush, bool isChosen)
        {
            EditorGUI.DrawRect(at, isChosen ? LevelColors.Selected : LevelColors.Lines);

            Rect inside = new Rect(at.x + 2f, at.y + 2f, at.width - 4f, at.height - 4f);
            EditorGUI.DrawRect(inside, Swatch(brush));

            GUI.Label(inside, brush.Name, LevelEditorStyles.CenteredLabel);
        }

        private static Color Swatch(LevelBrush brush)
        {
            if (brush.MovesTheStart)
            {
                return LevelColors.Head;
            }

            return brush.Erases ? LevelColors.Empty : LevelColors.Of(brush.Type);
        }
    }
}
