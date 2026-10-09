using Game.Authoring;
using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    // The fields around the grid. Every one of them writes straight onto the
    // draft: nothing is corrected here, because Validate is the only place the
    // tool is allowed to have an opinion.
    internal static class SettingsBar
    {
        private const float LabelWidth = 96f;
        private const float ArrowSize = 24f;

        internal static void Draw(LevelDraft draft)
        {
            EditorGUIUtility.labelWidth = LabelWidth;

            DrawBoard(draft);

            draft.SnakeLength = EditorGUILayout.IntField("Snake Length", draft.SnakeLength);
            draft.FoodGoal = EditorGUILayout.IntField("Food Goal", draft.FoodGoal);

            DrawStartDirection(draft);
            DrawGoldenApple(draft);
        }

        // Arrows in the positions they will actually be drawn in, rather than a
        // list of names. Direction names the board and the board is drawn a
        // quarter turn round, so a dropdown reading "Right" beside a snake
        // pointing up is correct and useless. The name is left in the tooltip
        // for anyone comparing the window against the file.
        private static void DrawStartDirection(LevelDraft draft)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Start Direction", GUILayout.Width(LabelWidth));

            Rect area = GUILayoutUtility.GetRect(ArrowSize * 3f, ArrowSize * 3f, GUILayout.ExpandWidth(false));

            Arrow(draft, area, 1, 0, Direction.Up);
            Arrow(draft, area, 2, 1, Direction.Right);
            Arrow(draft, area, 1, 2, Direction.Down);
            Arrow(draft, area, 0, 1, Direction.Left);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private static void Arrow(LevelDraft draft, Rect area, int column, int row, Direction drawnAs)
        {
            Direction heading = BoardToGrid.Heading(drawnAs);
            Rect at = new Rect(area.x + column * ArrowSize, area.y + row * ArrowSize, ArrowSize, ArrowSize);

            bool chosen = draft.StartDirection == heading;

            if (GUI.Toggle(at, chosen, new GUIContent(Glyph(drawnAs), heading.ToString()), EditorStyles.miniButton)
                && !chosen)
            {
                draft.StartDirection = heading;
                GUI.changed = true;
            }
        }

        private static string Glyph(Direction drawnAs)
        {
            switch (drawnAs)
            {
                case Direction.Up:
                    return "▲";
                case Direction.Right:
                    return "▶";
                case Direction.Down:
                    return "▼";
                default:
                    return "◀";
            }
        }

        private static void DrawBoard(LevelDraft draft)
        {
            int width = EditorGUILayout.IntField("Width", draft.Board.Width);
            int height = EditorGUILayout.IntField("Height", draft.Board.Height);

            if (width != draft.Board.Width || height != draft.Board.Height)
            {
                draft.Resize(width, height);
            }
        }

        private static void DrawGoldenApple(LevelDraft draft)
        {
            bool wanted = EditorGUILayout.Toggle("Golden Apple", draft.GoldenApple != null);

            if (!wanted)
            {
                draft.GoldenApple = null;
                return;
            }

            if (draft.GoldenApple == null)
            {
                draft.GoldenApple = new GoldenAppleData { SpawnAfterSeconds = 8f, StaysForSeconds = 5f };
            }

            EditorGUI.indentLevel++;
            draft.GoldenApple.SpawnAfterSeconds =
                EditorGUILayout.FloatField("Spawn After (s)", draft.GoldenApple.SpawnAfterSeconds);
            draft.GoldenApple.StaysForSeconds =
                EditorGUILayout.FloatField("Stays For (s)", draft.GoldenApple.StaysForSeconds);
            EditorGUI.indentLevel--;
        }
    }
}
