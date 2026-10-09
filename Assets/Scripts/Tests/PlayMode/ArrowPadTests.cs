using System.Collections;
using Game.Core;
using Game.Runtime;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public sealed class ArrowPadTests
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
        public IEnumerator TappingAnArrowTurnsTheSnake()
        {
            GameInstaller installer = Install();
            yield return null;

            Snake snake = installer.Session.Game.Snake;
            Assert.That(snake.Direction, Is.EqualTo(Direction.Right), "The level starts the snake going right.");

            _rig.Tap(Direction.Up);

            for (int frame = 0; frame < 20 && snake.Direction != Direction.Up; frame++)
            {
                yield return null;
            }

            Assert.That(snake.Direction, Is.EqualTo(Direction.Up),
                "A tap has to survive from the pointer event to the tick that spends it.");
        }

        [UnityTest]
        public IEnumerator ATapThatWouldTurnTheSnakeIntoItselfIsRefused()
        {
            GameInstaller installer = Install();
            yield return null;

            Snake snake = installer.Session.Game.Snake;

            _rig.Tap(Direction.Left);

            for (int frame = 0; frame < 5; frame++)
            {
                yield return null;
            }

            Assert.That(snake.Direction, Is.EqualTo(Direction.Right), "Turning back into the body is refused.");
        }

        [UnityTest]
        public IEnumerator APadPointingTwoArrowsTheSameWayIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Arrows[(int)Direction.Left].Configure(_rig.Buttons[(int)Direction.Left], Direction.Up);

            LogAssert.Expect(LogType.Error, new Regex("two arrows at Up"));

            GameInstaller installer = _rig.Run();
            yield return null;

            Assert.That(installer.Session, Is.Null);
            Assert.That(installer.enabled, Is.False);
        }
    }
}
