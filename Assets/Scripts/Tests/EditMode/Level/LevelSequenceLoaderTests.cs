using System.Collections.Generic;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public sealed class LevelSequenceLoaderTests
    {
        private readonly List<TextAsset> _files = new List<TextAsset>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _files.Count; i++)
            {
                Object.DestroyImmediate(_files[i]);
            }

            _files.Clear();
        }

        private static string Json(int side)
        {
            return @"{ ""Width"": " + side + @", ""Height"": " + side + @",
                ""StartX"": 2, ""StartY"": 2, ""StartDirection"": ""Right"",
                ""SnakeLength"": 3, ""FoodGoal"": 5 }";
        }

        private TextAsset NewFile(string name, string json)
        {
            TextAsset file = new TextAsset(json) { name = name };
            _files.Add(file);

            return file;
        }

        [Test]
        public void ASequenceWithNoFilesIsReported()
        {
            LevelSequence sequence;

            Assert.That(LevelSequenceLoader.TryLoad(null, out sequence).Error, Does.Contain("no level file"));
            Assert.That(LevelSequenceLoader.TryLoad(new TextAsset[0], out sequence).Error,
                Does.Contain("no level file"));
            Assert.That(sequence, Is.Null);
        }

        [Test]
        public void AnUnassignedSlotIsReportedByItsPlaceInTheOrder()
        {
            TextAsset[] files = { NewFile("level_01", Json(6)), null, NewFile("level_03", Json(8)) };

            LevelSequence sequence;
            Result result = LevelSequenceLoader.TryLoad(files, out sequence);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("Level file 2"));
            Assert.That(sequence, Is.Null);
        }

        [Test]
        public void ABrokenFileIsReportedByItsName()
        {
            TextAsset[] files =
            {
                NewFile("level_01", Json(6)),
                NewFile("level_02", Json(6).Replace(@"""Right""", @"""Sideways"""))
            };

            LevelSequence sequence;
            Result result = LevelSequenceLoader.TryLoad(files, out sequence);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("level_02").And.Contain("Sideways"));
        }

        [Test]
        public void TheFilesBecomeASequenceInTheOrderTheyWereListed()
        {
            TextAsset[] files =
            {
                NewFile("level_01", Json(6)),
                NewFile("level_02", Json(9)),
                NewFile("level_03", Json(12))
            };

            LevelSequence sequence;
            Result result = LevelSequenceLoader.TryLoad(files, out sequence);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(sequence.Count, Is.EqualTo(3));
            Assert.That(sequence[0].Board.Width, Is.EqualTo(6));
            Assert.That(sequence[1].Board.Width, Is.EqualTo(9));
            Assert.That(sequence[2].Board.Width, Is.EqualTo(12));
            Assert.That(sequence.Current, Is.SameAs(sequence[0]));
        }
    }
}
