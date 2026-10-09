using System.Collections;
using System.Collections.Generic;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public sealed class PieceDrawingTests
    {
        private GameRig _rig;
        private GameInstaller _installer;

        [TearDown]
        public void TearDown()
        {
            if (_rig != null)
            {
                _rig.Destroy();
                _rig = null;
                _installer = null;
            }
        }

        private GameInstaller Install()
        {
            return Install(GameRig.ValidLevel);
        }

        private GameInstaller Install(string json)
        {
            _rig = new GameRig(json);
            _installer = _rig.Run();

            return _installer;
        }

        private CellView[] Drawn(Component view)
        {
            return view.GetComponentsInChildren<CellView>(false);
        }

        [UnityTest]
        public IEnumerator TheSnakeIsDrawnOnePartPerBodyCell()
        {
            GameInstaller installer = Install();

            yield return null;

            Assert.That(Drawn(_installer.Snake.Root).Length, Is.EqualTo(installer.Session.Game.Snake.Length));
        }

        [UnityTest]
        public IEnumerator PartsAreDrawnBetweenCellsRatherThanSnappingOntoThem()
        {
            Install();
            yield return null;
            _rig.Tap(Direction.Right);

            bool caughtMidStep = false;

            for (int frame = 0; frame < 20 && !caughtMidStep; frame++)
            {
                yield return null;

                Vector3 at = Drawn(_installer.Snake.Root)[0].transform.localPosition;
                caughtMidStep = Mathf.Abs(at.x - Mathf.Round(at.x)) > 0.01f
                    || Mathf.Abs(at.z - Mathf.Round(at.z)) > 0.01f;
            }

            Assert.That(caughtMidStep, Is.True,
                "Twenty frames of a " + GameRig.Interval + " second tick cannot all land exactly on a cell.");
        }

        [UnityTest]
        public IEnumerator EveryPartStaysWithinOneCellOfWhereTheBodyIs()
        {
            GameInstaller installer = Install();
            yield return null;

            _rig.Tap(Direction.Right);

            for (int frame = 0; frame < 20; frame++)
            {
                yield return null;

                CellView[] parts = Drawn(_installer.Snake.Root);
                Snake snake = installer.Session.Game.Snake;

                Assert.That(parts.Length, Is.EqualTo(snake.Length));

                for (int i = 0; i < parts.Length; i++)
                {
                    Assert.That(NearestCellDistance(parts[i].transform.localPosition, snake),
                        Is.LessThanOrEqualTo(1.001f),
                        "A part is drawn more than one cell away from any part of the body.");
                }
            }
        }

        [UnityTest]
        public IEnumerator TheLevelsGroundIsDrawnOneCellPerItem()
        {
            Install(GameRig.WalledLevel);

            yield return null;

            CellView[] walls = Drawn(_installer.Ground.Root);

            Assert.That(walls.Length, Is.EqualTo(3), "The level lays three walls, so three cells are drawn.");

            HashSet<GridPosition> drawn = new HashSet<GridPosition>();

            for (int i = 0; i < walls.Length; i++)
            {
                Vector3 at = walls[i].transform.localPosition;

                Assert.That(at.x, Is.EqualTo(Mathf.Round(at.x)).Within(0.001f), "Ground does not move, so it snaps to its cell.");
                Assert.That(at.z, Is.EqualTo(Mathf.Round(at.z)).Within(0.001f));

                drawn.Add(new GridPosition(Mathf.RoundToInt(at.x), Mathf.RoundToInt(at.z)));
            }

            Assert.That(drawn, Is.EquivalentTo(new[]
            {
                new GridPosition(0, 0),
                new GridPosition(6, 4),
                new GridPosition(7, 7)
            }), "Each wall is drawn on the cell the level named, with Y going up the board.");
        }

        // One prefab draws the board and the ground standing on it, so the prefab
        // cannot carry the difference in height. The visuals asset does.
        [UnityTest]
        public IEnumerator GroundStandsOnTheBoardRatherThanInsideIt()
        {
            GameInstaller installer = Install(GameRig.WalledLevel);

            yield return null;

            CellView[] tiles = Drawn(installer.Board.Root);
            CellView[] walls = Drawn(installer.Ground.Root);

            Assert.That(tiles.Length, Is.GreaterThan(0));
            Assert.That(walls.Length, Is.GreaterThan(0));

            for (int i = 0; i < tiles.Length; i++)
            {
                Assert.That(tiles[i].transform.localPosition.y, Is.EqualTo(_rig.Visuals.BoardHeight).Within(0.001f));
            }

            for (int i = 0; i < walls.Length; i++)
            {
                Assert.That(walls[i].transform.localPosition.y, Is.EqualTo(1f).Within(0.001f),
                    "The wall's row in the visuals asks for a height of one.");
            }
        }

        [UnityTest]
        public IEnumerator PiecesAreDrawnAtTheHeightTheVisualsAskFor()
        {
            GameInstaller installer = Install();

            yield return null;

            CellView[] parts = Drawn(installer.Snake.Root);
            CellView[] apples = Drawn(installer.Food.Root);

            for (int i = 0; i < parts.Length; i++)
            {
                Assert.That(parts[i].transform.localPosition.y, Is.EqualTo(_rig.Visuals.PieceHeight).Within(0.001f));
            }

            Assert.That(apples[0].transform.localPosition.y, Is.EqualTo(_rig.Visuals.PieceHeight).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator ATunnelTurnsTheSnakeInThePlayingGame()
        {
            GameInstaller installer = Install(GameRig.TunnelLevel);

            yield return null;

            // The head starts at (2,4) facing Right with a tunnel at (4,4), so
            // Right goes in and Up comes out.
            _rig.Tap(Direction.Right);

            for (int frame = 0; frame < 60 && installer.Session.Game.Snake.Direction == Direction.Right; frame++)
            {
                yield return null;
            }

            Assert.That(installer.Session.Game.Snake.Direction, Is.EqualTo(Direction.Up));
            Assert.That(installer.Session.Status, Is.EqualTo(GameStatus.Running), "A tunnel is not fatal.");
            Assert.That(Drawn(installer.Ground.Root).Length, Is.EqualTo(1), "The tunnel is drawn like any ground.");
        }

        [UnityTest]
        public IEnumerator AGoldenAppleIsDrawnThreeTimesTheSizeOfAPlainOne()
        {
            GameInstaller installer = Install(GameRig.GoldenAppleLevel);

            yield return null;

            _rig.Tap(Direction.Right);

            GoldenAppleSpawner golden = installer.Session.Game.Apples.Golden;

            for (int frame = 0; frame < 60 && !golden.IsOnTheBoard; frame++)
            {
                yield return null;
            }

            Assert.That(golden.IsOnTheBoard, Is.True, "One tick of cooldown and it should be there.");

            CellView[] drawn = Drawn(installer.GoldenApple.Root);

            Assert.That(drawn.Length, Is.EqualTo(1));
            Assert.That(drawn[0].transform.localScale.x,
                Is.EqualTo(_rig.Visuals.GoldenAppleScale).Within(0.001f));
            Assert.That(drawn[0].transform.localPosition.x, Is.EqualTo((float)golden.Position.X).Within(0.001f));
            Assert.That(drawn[0].transform.localPosition.z, Is.EqualTo((float)golden.Position.Y).Within(0.001f));

            Assert.That(Drawn(installer.Snake.Root)[0].transform.localScale.x, Is.EqualTo(1f).Within(0.001f),
                "The stock is shared, so a stretched cell must come back its own size.");
        }

        [UnityTest]
        public IEnumerator AnOpenBoardDrawsNoGround()
        {
            Install();

            yield return null;

            Assert.That(Drawn(_installer.Ground.Root).Length, Is.EqualTo(0),
                "The level lays nothing, so the ground pool is untouched.");
        }

        [UnityTest]
        public IEnumerator DrivingIntoAWallEndsTheRun()
        {
            GameInstaller installer = Install(GameRig.WalledLevel);

            yield return null;

            // The head starts at (4,4) facing Right, so setting the run off with
            // Right steers nothing: (5,4), then (6,4), which the level walled.
            _rig.Tap(Direction.Right);

            for (int frame = 0; frame < 60 && !installer.Session.IsOver; frame++)
            {
                yield return null;
            }

            Assert.That(installer.Session.Status, Is.EqualTo(GameStatus.Lost),
                "The wall is two cells ahead and the board edge is three, so a wall is what it met.");
        }

        [UnityTest]
        public IEnumerator TheFoodIsDrawnExactlyOnItsCell()
        {
            GameInstaller installer = Install();

            yield return null;

            FoodSpawner food = installer.Session.Game.Food;
            Assert.That(food.IsOnTheBoard, Is.True);

            CellView[] apples = Drawn(_installer.Food.Root);
            Assert.That(apples.Length, Is.EqualTo(1), "One apple is on the board, so one apple is drawn.");

            Vector3 at = apples[0].transform.localPosition;
            Assert.That(at.x, Is.EqualTo((float)food.Position.X).Within(0.001f));
            Assert.That(at.z, Is.EqualTo((float)food.Position.Y).Within(0.001f));
        }

        private static float NearestCellDistance(Vector3 drawn, Snake snake)
        {
            float nearest = float.MaxValue;

            for (int i = 0; i < snake.Length; i++)
            {
                GridPosition cell = snake[i];
                float distance = Mathf.Abs(drawn.x - cell.X) + Mathf.Abs(drawn.z - cell.Y);

                if (distance < nearest)
                {
                    nearest = distance;
                }
            }

            return nearest;
        }
    }
}
