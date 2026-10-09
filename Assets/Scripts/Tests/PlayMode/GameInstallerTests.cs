using System.Collections;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public sealed class GameInstallerTests
    {
        private GameRig _rig;

        [TearDown]
        public void TearDown()
        {
            if (_rig != null)
            {
                _rig.Destroy();
                _rig = null;
            }
        }

        private GameInstaller Install()
        {
            _rig = new GameRig(GameRig.ValidLevel);

            return _rig.Run();
        }

        [UnityTest]
        public IEnumerator AValidFileBecomesASessionWaitingForItsFirstDirection()
        {
            GameInstaller installer = Install();

            yield return null;

            Assert.That(installer.Session, Is.Not.Null);
            Assert.That(installer.Session.Status, Is.EqualTo(GameStatus.Ready));
            Assert.That(installer.Session.Game.Snake.Length, Is.EqualTo(3));
            Assert.That(installer.Session.Game.Snake.Head, Is.EqualTo(new GridPosition(8, 8)));
            Assert.That(installer.Session.FoodGoal, Is.EqualTo(10));
        }

        [UnityTest]
        public IEnumerator FramesAloneMoveTheSnakeOnceItHasSetOff()
        {
            GameInstaller installer = Install();
            yield return null;

            GridPosition start = installer.Session.Game.Snake.Head;
            _rig.Tap(Direction.Right);

            yield return new WaitForSeconds(GameRig.Interval * 4f);

            Assert.That(installer.Session.Game.Snake.Head, Is.Not.EqualTo(start),
                "Update is the only thing driving the session, so passing time must be enough.");
        }

        // Eight steps to the right and the snake is off the board, so a little
        // over that long is a run that has ended and a popup that is up.
        [UnityTest]
        public IEnumerator ThePopupButtonIsWiredToStartTheRunOver()
        {
            GameInstaller installer = Install();
            yield return null;
            _rig.Tap(Direction.Right);

            yield return new WaitForSeconds(GameRig.Interval * 12f);
            Assert.That(installer.Session.Status, Is.EqualTo(GameStatus.Lost), "The run never ended.");

            _rig.PopupButton.OnPointerClick(null);

            Assert.That(installer.Session.Status, Is.EqualTo(GameStatus.Ready), "The next run waits for the player too.");
            Assert.That(installer.Session.Game.Snake.Head, Is.EqualTo(new GridPosition(8, 8)));
        }

        [UnityTest]
        public IEnumerator TheBoardIsFramedAgainWhenTheScreenChangesShape()
        {
            Install();
            yield return null;

            float before = _rig.Camera.orthographicSize;

            // A far narrower window: width now decides the fit, so a camera that
            // never looked again would leave the board cropped.
            _rig.Camera.aspect = 0.2f;
            yield return null;

            Assert.That(_rig.Camera.orthographicSize, Is.GreaterThan(before));
        }

        [UnityTest]
        public IEnumerator TheGroundIsAnAlternatingCheckerboard()
        {
            GameInstaller installer = Install();
            yield return null;

            CellView[] cells = installer.Board.Root.GetComponentsInChildren<CellView>(false);
            Assert.That(cells.Length, Is.EqualTo(16 * 16), "One cell per square, and none left over.");

            int dark = 0;
            for (int i = 0; i < cells.Length; i++)
            {
                Vector3 at = cells[i].transform.localPosition;
                bool shouldBeDark = (((int)at.x + (int)at.z) & 1) == 0;
                Mesh shown = cells[i].GetComponentInChildren<MeshFilter>().sharedMesh;

                Assert.That(shown, Is.EqualTo(shouldBeDark ? _rig.DarkMesh : _rig.LightMesh),
                    "The cell at " + at.x + ", " + at.z + " is showing the wrong mesh.");

                dark += shouldBeDark ? 1 : 0;
            }

            Assert.That(dark, Is.EqualTo(128), "An even board splits evenly between the two shades.");
        }

        [UnityTest]
        public IEnumerator EveryCornerOfTheBoardLandsInsideTheViewport()
        {
            Install();
            yield return null;

            AssertOnScreen(new Vector3(-0.5f, 0f, -0.5f));
            AssertOnScreen(new Vector3(15.5f, 0f, -0.5f));
            AssertOnScreen(new Vector3(-0.5f, 0f, 15.5f));
            AssertOnScreen(new Vector3(15.5f, 0f, 15.5f));
        }

        private void AssertOnScreen(Vector3 corner)
        {
            Vector3 viewport = _rig.Camera.WorldToViewportPoint(corner);

            Assert.That(viewport.z, Is.GreaterThan(0f), "The corner at " + corner + " is behind the camera.");
            Assert.That(viewport.x, Is.InRange(0f, 1f), "The corner at " + corner + " is off the side.");
            Assert.That(viewport.y, Is.InRange(0f, 1f), "The corner at " + corner + " is off the top or bottom.");
        }
    }
}
