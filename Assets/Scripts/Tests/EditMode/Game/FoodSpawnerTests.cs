using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class FoodSpawnerTests
    {
        private static readonly GridSize Board = new GridSize(4, 3);

        [Test]
        public void AMapAndARandomSourceAreBothRequired()
        {
            GridMap map = new GridMap(Board);

            Assert.Throws<ArgumentNullException>(() => new FoodSpawner(null, new ScriptedRandomSource()));
            Assert.Throws<ArgumentNullException>(() => new FoodSpawner(map, null));
        }

        [Test]
        public void FoodLandsOnTheFreeCellTheRandomSourcePicked()
        {
            GridMap map = new GridMap(Board);
            ScriptedRandomSource random = new ScriptedRandomSource { NextValue = 5 };
            FoodSpawner food = new FoodSpawner(map, random);
            GridPosition expected = map.FreeCellAt(5);

            Assert.That(food.TryRespawn(), Is.True);

            Assert.That(food.IsOnTheBoard, Is.True);
            Assert.That(food.Position, Is.EqualTo(expected));
            Assert.That(map[expected], Is.EqualTo(CellOccupantType.Food));
        }

        [Test]
        public void TheRandomSourceIsOfferedEveryFreeCell()
        {
            GridMap map = new GridMap(Board);
            map.SetGround(new GridPosition(1, 1), CellGroundType.Wall);
            ScriptedRandomSource random = new ScriptedRandomSource();
            FoodSpawner food = new FoodSpawner(map, random);

            food.TryRespawn();

            Assert.That(random.LastMaxExclusive, Is.EqualTo(Board.CellCount - 1),
                "A cell outside the offered range could never be chosen.");
        }

        [Test]
        public void FoodNeverLandsOnAnOccupiedCell()
        {
            GridMap map = new GridMap(Board);
            map.SetGround(new GridPosition(1, 1), CellGroundType.Wall);
            map.SetGround(new GridPosition(2, 2), CellGroundType.Wall);
            ScriptedRandomSource random = new ScriptedRandomSource();
            FoodSpawner food = new FoodSpawner(map, random);

            for (int round = 0; round < 20; round++)
            {
                random.NextValue = round % map.FreeCount;
                GridPosition target = map.FreeCellAt(random.NextValue);
                Assert.That(map[target], Is.EqualTo(CellOccupantType.Empty));

                food.TryRespawn();

                Assert.That(map[target], Is.EqualTo(CellOccupantType.Food));
                Assert.That(map.FreeCount, Is.EqualTo(Board.CellCount - 2 - 1),
                    "Two walls and exactly one piece of food should be standing on the board.");
            }
        }

        [Test]
        public void RespawningTakesTheUneatenFoodBack()
        {
            GridMap map = new GridMap(Board);
            ScriptedRandomSource random = new ScriptedRandomSource { NextValue = 3 };
            FoodSpawner food = new FoodSpawner(map, random);
            food.TryRespawn();
            GridPosition first = food.Position;

            random.NextValue = 0;
            food.TryRespawn();

            Assert.That(map[first], Is.EqualTo(CellOccupantType.Empty));
            Assert.That(food.Position, Is.Not.EqualTo(first));
            Assert.That(map.FreeCount, Is.EqualTo(Board.CellCount - 1));
        }

        [Test]
        public void EatenFoodIsNotTakenBack()
        {
            GridMap map = new GridMap(Board);
            ScriptedRandomSource random = new ScriptedRandomSource { NextValue = 3 };
            FoodSpawner food = new FoodSpawner(map, random);
            food.TryRespawn();
            GridPosition eaten = food.Position;

            // This is what a step that eats does to the cell.
            map.Set(eaten, CellOccupantType.Snake);

            random.NextValue = 0;
            food.TryRespawn();

            Assert.That(map[eaten], Is.EqualTo(CellOccupantType.Snake), "The spawner released a cell the snake had taken.");
        }

        [Test]
        public void DespawningReleasesTheCell()
        {
            GridMap map = new GridMap(Board);
            FoodSpawner food = new FoodSpawner(map, new ScriptedRandomSource { NextValue = 3 });
            food.TryRespawn();
            GridPosition standing = food.Position;

            food.Despawn();

            Assert.That(food.IsOnTheBoard, Is.False);
            Assert.That(map[standing], Is.EqualTo(CellOccupantType.Empty));
            Assert.That(map.FreeCount, Is.EqualTo(Board.CellCount));
        }

        [Test]
        public void DespawningAfterABiteLeavesTheCellToTheSnake()
        {
            GridMap map = new GridMap(Board);
            FoodSpawner food = new FoodSpawner(map, new ScriptedRandomSource { NextValue = 3 });
            food.TryRespawn();
            GridPosition eaten = food.Position;
            map.Set(eaten, CellOccupantType.Snake);

            food.Despawn();

            Assert.That(food.IsOnTheBoard, Is.False);
            Assert.That(map[eaten], Is.EqualTo(CellOccupantType.Snake));
        }

        [Test]
        public void AFullBoardCannotHoldFood()
        {
            GridMap map = new GridMap(Board);
            for (int index = 0; index < Board.CellCount; index++)
            {
                map.SetGround(Board.FromIndex(index), CellGroundType.Wall);
            }

            FoodSpawner food = new FoodSpawner(map, new ScriptedRandomSource());

            Assert.That(food.TryRespawn(), Is.False);
            Assert.That(food.IsOnTheBoard, Is.False);
        }
    }
}
