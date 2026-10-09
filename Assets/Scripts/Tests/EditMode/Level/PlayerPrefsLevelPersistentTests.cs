using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    // This writes to the same key the game does, so each test clears it
    // rather than working around it.
    public sealed class PlayerPrefsLevelPersistentTests
    {
        private PlayerPrefsLevelPersistent _levelPersistent;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(PlayerPrefsLevelPersistent.Key);
            _levelPersistent = new PlayerPrefsLevelPersistent();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(PlayerPrefsLevelPersistent.Key);
        }

        [Test]
        public void APlayerWhoHasNeverFinishedALevelStartsAtTheFirstOne()
        {
            Assert.That(_levelPersistent.Read(), Is.EqualTo(0));
        }

        [Test]
        public void WhatWasWrittenComesBack()
        {
            _levelPersistent.Write(3);

            Assert.That(_levelPersistent.Read(), Is.EqualTo(3));
        }

        [Test]
        public void TheLatestWriteIsTheOneThatCounts()
        {
            _levelPersistent.Write(3);
            _levelPersistent.Write(1);

            Assert.That(_levelPersistent.Read(), Is.EqualTo(1), "Losing ground has to be recordable too.");
        }

        [Test]
        public void AnotherReaderFindsTheSamePlace()
        {
            _levelPersistent.Write(2);

            Assert.That(new PlayerPrefsLevelPersistent().Read(), Is.EqualTo(2),
                "The place outlives the object that wrote it, which is the whole point.");
        }
    }
}
