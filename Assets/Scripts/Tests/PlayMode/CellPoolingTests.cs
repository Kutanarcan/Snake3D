using System.Collections;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    // One owner lends every cell in the scene, and it lends enough of them up
    // front that a run never stops to build one.
    public sealed class CellPoolingTests
    {
        // 16 x 16 board, a snake of 3 chasing a goal of 10, and room for both
        // apples at once -- a golden one leaves the plain one where it is:
        // 256 + 3 + 10 + 2, plus the prefab the rig itself holds.
        private const int Expected = 256 + 15 + 1;

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

        private int Instantiated()
        {
            return _rig.Host.GetComponentsInChildren<CellView>(true).Length;
        }

        private static CellView[] Drawn(Component view)
        {
            return view.GetComponentsInChildren<CellView>(false);
        }

        [UnityTest]
        public IEnumerator TheSnakeAndTheAppleDrawFromOneStock()
        {
            Install();

            yield return null;

            // Separate stocks would have to prewarm the snake and the apple
            // apart, and the count would not land here.
            Assert.That(Instantiated(), Is.EqualTo(Expected));
        }

        [UnityTest]
        public IEnumerator NothingIsBuiltOnceTheRunIsUnderWay()
        {
            Install();
            yield return null;

            _rig.Tap(Direction.Right);

            int atTheStart = Instantiated();

            for (int frame = 0; frame < 30; frame++)
            {
                yield return null;
            }

            Assert.That(Instantiated(), Is.EqualTo(atTheStart),
                "A run that instantiates while it is playing is a run that can hitch.");
        }

        [UnityTest]
        public IEnumerator ShowingTheSnakeAgainGivesBackThePartsItWasHolding()
        {
            GameInstaller installer = Install();
            yield return null;

            int before = Instantiated();
            Snake snake = installer.Session.Game.Snake;

            installer.Snake.Show(snake);

            Assert.That(Drawn(installer.Snake.Root).Length, Is.EqualTo(snake.Length));
            Assert.That(Instantiated(), Is.EqualTo(before),
                "Restarting must return parts to the stock rather than leak a new body.");
        }

        [UnityTest]
        public IEnumerator StartingTheRunOverBuildsNothingNew()
        {
            GameInstaller installer = Install();
            yield return null;
            _rig.Tap(Direction.Right);

            // The snake walks off the board, which is what puts the popup up and
            // makes its button answer at all.
            yield return new WaitForSeconds(GameRig.Interval * 12f);
            Assert.That(installer.Session.IsOver, Is.True, "The run never ended.");

            int before = Instantiated();

            _rig.PopupButton.OnPointerClick(null);

            Assert.That(Instantiated(), Is.EqualTo(before),
                "A retry must hand the old body back rather than cut a new one.");
        }

        [UnityTest]
        public IEnumerator TheAppleStaysOnItsCellForTheWholeRun()
        {
            GameInstaller installer = Install();
            yield return null;

            // The run waits for a direction, and an apple that is never eaten
            // is an apple that cannot go stale.
            _rig.Tap(Direction.Right);

            for (int frame = 0; frame < 30; frame++)
            {
                yield return null;

                FoodSpawner food = installer.Session.Game.Food;
                CellView[] apples = Drawn(installer.Food.Root);

                if (!food.IsOnTheBoard)
                {
                    Assert.That(apples.Length, Is.EqualTo(0));
                    continue;
                }

                // The apple is placed on an event rather than every frame, so a
                // missed event shows up here as a stale position.
                Assert.That(apples.Length, Is.EqualTo(1));
                Assert.That(apples[0].transform.localPosition.x, Is.EqualTo((float)food.Position.X).Within(0.001f));
                Assert.That(apples[0].transform.localPosition.z, Is.EqualTo((float)food.Position.Y).Within(0.001f));
            }
        }
    }
}
