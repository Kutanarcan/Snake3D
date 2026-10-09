using System.Collections.Generic;
using Game.Authoring;
using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    // The board drawn the way the camera shows it: BoardToGrid decides which
    // cell a row and column mean, and this only paints rectangles and reads the
    // mouse. Nothing here knows what a brush does.
    internal static class GridPainter
    {
        private const float CellSize = 52f;

        internal static void Draw(LevelDraft draft, LevelBrush brush)
        {
            BoardToGrid grid = new BoardToGrid(draft.Board);

            Rect area = GUILayoutUtility.GetRect(grid.Columns * CellSize + 1f, grid.Rows * CellSize + 1f,
                GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

            if (Event.current.type == EventType.Repaint)
            {
                PaintCells(area, grid, draft);
            }

            ReadMouse(area, grid, draft, brush);
        }

        // The lines are one rectangle under everything and each cell is inset
        // into it, so the gaps between cells belong to the board rather than to
        // whatever the editor happens to be painted with.
        private static void PaintCells(Rect area, BoardToGrid grid, LevelDraft draft)
        {
            EditorGUI.DrawRect(area, LevelColors.Lines);

            HashSet<GridPosition> body = BodyCells(draft);

            for (int row = 0; row < grid.Rows; row++)
            {
                for (int column = 0; column < grid.Columns; column++)
                {
                    GridPosition cell = grid.Cell(row, column);

                    EditorGUI.DrawRect(
                        new Rect(area.x + column * CellSize + 1f, area.y + row * CellSize + 1f,
                            CellSize - 1f, CellSize - 1f),
                        ColorOf(draft, cell, body));
                }
            }
        }

        // The body is drawn as well as the head, because a start that no longer
        // fits is the tool's most common complaint and the cells it needs are
        // otherwise invisible.
        private static HashSet<GridPosition> BodyCells(LevelDraft draft)
        {
            HashSet<GridPosition> cells = new HashSet<GridPosition>();
            GridPosition backwards = draft.StartDirection.Opposite().ToOffset();
            GridPosition cell = draft.Start;

            for (int i = 0; i < draft.SnakeLength && i < draft.Board.CellCount; i++)
            {
                cells.Add(cell);
                cell += backwards;
            }

            return cells;
        }

        private static Color ColorOf(LevelDraft draft, GridPosition cell, HashSet<GridPosition> body)
        {
            LevelItemData item = draft.Items.ItemAt(cell);

            if (item != null)
            {
                return LevelColors.Of(TypeOf(item));
            }

            if (cell == draft.Start)
            {
                return LevelColors.Head;
            }

            return body.Contains(cell) ? LevelColors.Body : LevelColors.Empty;
        }

        private static CellGroundType TypeOf(LevelItemData item)
        {
            CellGroundType type;

            return System.Enum.TryParse(item.Type, true, out type) ? type : CellGroundType.Open;
        }

        private static void ReadMouse(Rect area, BoardToGrid grid, LevelDraft draft, LevelBrush brush)
        {
            Event current = Event.current;

            if (current.button != 0 || !area.Contains(current.mousePosition))
            {
                return;
            }

            if (current.type != EventType.MouseDown && current.type != EventType.MouseDrag)
            {
                return;
            }

            int column = (int)((current.mousePosition.x - area.x) / CellSize);
            int row = (int)((current.mousePosition.y - area.y) / CellSize);

            if (row < 0 || row >= grid.Rows || column < 0 || column >= grid.Columns)
            {
                return;
            }

            brush.Apply(draft, grid.Cell(row, column));

            GUI.changed = true;
            current.Use();
        }
    }
}
