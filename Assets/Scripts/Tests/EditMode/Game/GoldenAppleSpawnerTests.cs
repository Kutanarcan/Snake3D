using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class GoldenAppleSpawnerTests
    {
        private static readonly GridSize Board = new GridSize(4, 4);

        private static GoldenAppleSpawner NewSpawner(GridMap map, int spawnDelay, int stay)
        {
            return new GoldenAppleSpawner(map, new ScriptedRandomSource(), spawnDelay, stay);
        }

        private static void Tick(GoldenAppleSpawner spawner, int times)
        {
            for (int i = 0; i < times; i++)
            {
                spawner.Tick();
            }
        }

        private static void Fill(GridMap map)
        {
            for (int cell = 0; cell < map.Size.CellCount; cell++)
            {
                map.Set(map.Size.FromIndex(cell), CellOccupantType.Snake);
            }
        }

        [Test]
        public void AMapAndARandomSourceAreRequired()
        {
            GridMap map = new GridMap(Board);

            Assert.Throws<ArgumentNullException>(() => new GoldenAppleSpawner(null, new ScriptedRandomSource(), 1, 1));
            Assert.Throws<ArgumentNullException>(() => new GoldenAppleSpawner(map, null, 1, 1));
        }

        [Test]
        public void NothingAppearsBeforeTheSpawnCooldownIsUp()
        {
            GridMap map = new GridMap(Board);
            GoldenAppleSpawner spawner = NewSpawner(map, 3, 2);

            Tick(spawner, 2);

            Assert.That(spawner.IsOnTheBoard, Is.False, "Two of the three steps have gone by.");

            spawner.Tick();

            Assert.That(spawner.IsOnTheBoard, Is.True);
            Assert.That(map[spawner.Position], Is.EqualTo(CellOccupantType.GoldenApple));
        }

        [Test]
        public void ItLandsOnACellThatWasFreeAndTakesItOutOfTheFreeList()
        {
            GridMap map = new GridMap(Board);
            GoldenAppleSpawner spawner = NewSpawner(map, 1, 5);
            int freeBefore = map.FreeCount;

            spawner.Tick();

            Assert.That(map.IsFree(spawner.Position), Is.False);
            Assert.That(map.FreeCount, Is.EqualTo(freeBefore - 1));
        }

        [Test]
        public void ItLeavesOnceItHasStayedItsTime()
        {
            GridMap map = new GridMap(Board);
            GoldenAppleSpawner spawner = NewSpawner(map, 1, 3);

            spawner.Tick();
            GridPosition where = spawner.Position;

            Tick(spawner, 2);
            Assert.That(spawner.IsOnTheBoard, Is.True, "Two of the three steps have gone by.");

            spawner.Tick();

            Assert.That(spawner.IsOnTheBoard, Is.False);
            Assert.That(map[where], Is.EqualTo(CellOccupantType.Empty), "The cell was not given back.");
        }

        [Test]
        public void TheWaitStartsOverAndTheAppleComesBack()
        {
            GridMap map = new GridMap(Board);
            GoldenAppleSpawner spawner = NewSpawner(map, 2, 1);

            Tick(spawner, 3);

            Assert.That(spawner.IsOnTheBoard, Is.False, "Two steps to arrive, one to stay.");

            Tick(spawner, 2);

            Assert.That(spawner.IsOnTheBoard, Is.True, "The cycle runs again from the top.");
        }

        [Test]
        public void BeingEatenRestartsTheWaitRatherThanTheStay()
        {
            GridMap map = new GridMap(Board);
            GoldenAppleSpawner spawner = NewSpawner(map, 2, 20);

            Tick(spawner, 2);
            GridPosition where = spawner.Position;

            spawner.Despawn();

            Assert.That(map[where], Is.EqualTo(CellOccupantType.Empty));

            spawner.Tick();
            Assert.That(spawner.IsOnTheBoard, Is.False, "One of the two steps has gone by.");

            spawner.Tick();
            Assert.That(spawner.IsOnTheBoard, Is.True);
        }

        [Test]
        public void ALevelThatAsksForNoCooldownsNeverGetsAGoldenApple()
        {
            GridMap map = new GridMap(Board);
            GoldenAppleSpawner spawner = NewSpawner(map, 0, 0);

            Assert.That(spawner.IsEnabled, Is.False);

            Tick(spawner, 50);

            Assert.That(spawner.IsOnTheBoard, Is.False);
            Assert.That(map.FreeCount, Is.EqualTo(Board.CellCount));
        }

        [Test]
        public void AFullBoardIsTriedAgainOnTheNextStepInsteadOfSkippingTheTurn()
        {
            GridMap map = new GridMap(Board);
            GoldenAppleSpawner spawner = NewSpawner(map, 1, 5);
            Fill(map);

            spawner.Tick();

            Assert.That(spawner.IsOnTheBoard, Is.False, "There was nowhere to put it.");

            map.Set(new GridPosition(2, 2), CellOccupantType.Empty);
            spawner.Tick();

            Assert.That(spawner.IsOnTheBoard, Is.True);
            Assert.That(spawner.Position, Is.EqualTo(new GridPosition(2, 2)), "The only cell left is the one it took.");
        }
    }
}
