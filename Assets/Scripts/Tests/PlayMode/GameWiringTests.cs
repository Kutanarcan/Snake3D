using System.Collections;
using System.Text.RegularExpressions;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    // Everything a person fills in by hand -- the level file and the Inspector
    // fields -- is reported the same way: one error naming the field, and an
    // installer that switches itself off rather than running half built.
    public sealed class GameWiringTests
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

        private IEnumerator Reports(string expected)
        {
            LogAssert.Expect(LogType.Error, new Regex(expected));

            GameInstaller installer = _rig.Run();
            yield return null;

            Assert.That(installer.Session, Is.Null);
            Assert.That(installer.enabled, Is.False,
                "A disabled installer is one whose Update can never hit a null.");
        }

        [UnityTest]
        public IEnumerator ABrokenLevelFileIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel.Replace(
                @"""StartDirection"": ""Right""", @"""StartDirection"": ""Sideways"""));

            yield return Reports("Sideways");
        }

        [UnityTest]
        public IEnumerator AMissingLevelFileIsReported()
        {
            _rig = new GameRig(null);

            yield return Reports("no level file");
        }

        [UnityTest]
        public IEnumerator AMissingVisualsAssetIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Installer.Configure(_rig.LevelFiles, GameRig.Interval,
                new SceneViews(null, _rig.Framing, _rig.Pad, _rig.Information, _rig.Popup, _rig.SafeArea,
                    _rig.Audio, _rig.Endings, _rig.Effects, _rig.Shaker));

            yield return Reports("no board visuals");
        }

        [UnityTest]
        public IEnumerator AnUnassignedCellPrefabIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Visualise(null, _rig.CellPrefab, _rig.DarkMesh, _rig.LightMesh, _rig.GroundTable,
                _rig.PieceMesh, _rig.PieceMesh);

            yield return Reports("no environment prefab");
        }

        [UnityTest]
        public IEnumerator AnUnassignedMeshIsReportedRatherThanDrawingNothing()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Visualise(_rig.CellPrefab, _rig.CellPrefab, null, _rig.LightMesh, _rig.GroundTable,
                _rig.PieceMesh, _rig.PieceMesh);

            yield return Reports("no dark board mesh");
        }

        // A ground type with no mesh would be drawn as nothing and still kill the
        // player, so the table is checked against the enum at startup rather than
        // against whichever levels happen to be loaded.
        [UnityTest]
        public IEnumerator AGroundTypeWithoutAMeshIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Visualise(_rig.CellPrefab, _rig.CellPrefab, _rig.DarkMesh, _rig.LightMesh,
                new GroundVisual[0], _rig.PieceMesh, _rig.PieceMesh);

            yield return Reports("no mesh for Wall");
        }

        [UnityTest]
        public IEnumerator AMissingCameraIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Framing.Configure(null, 0.1f, 0.25f);

            yield return Reports("no camera");
        }

        [UnityTest]
        public IEnumerator AnUnassignedSnakeMeshIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Visualise(_rig.CellPrefab, _rig.CellPrefab, _rig.DarkMesh, _rig.LightMesh,
                _rig.GroundTable, null, _rig.PieceMesh);

            yield return Reports("no snake head mesh");
        }

        [UnityTest]
        public IEnumerator AnUnassignedFoodMeshIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Visualise(_rig.CellPrefab, _rig.CellPrefab, _rig.DarkMesh, _rig.LightMesh,
                _rig.GroundTable, _rig.PieceMesh, null);

            yield return Reports("no food mesh");
        }

        [UnityTest]
        public IEnumerator AnUnassignedFoodAmountLabelIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Information.Configure(null);

            yield return Reports("no food amount label");
        }

        [UnityTest]
        public IEnumerator AnEndingWithoutATitleIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.PopupSettings.Configure(
                new LevelEndingInformation(GameRig.WonTitle, _rig.WonStar, GameRig.WonButton),
                new LevelEndingInformation("   ", _rig.LostStar, GameRig.LostButton));

            yield return Reports("lost ending has no title");
        }

        [UnityTest]
        public IEnumerator AnEndingWithoutAStarIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.PopupSettings.Configure(
                new LevelEndingInformation(GameRig.WonTitle, null, GameRig.WonButton),
                new LevelEndingInformation(GameRig.LostTitle, _rig.LostStar, GameRig.LostButton));

            yield return Reports("won ending has no star icon");
        }

        [UnityTest]
        public IEnumerator AnUnassignedSafeAreaIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.SafeArea.Configure(null);

            yield return Reports("no area assigned");
        }

        [UnityTest]
        public IEnumerator AMissingAudioManagerIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Installer.Configure(_rig.LevelFiles, GameRig.Interval,
                new SceneViews(_rig.Visuals, _rig.Framing, _rig.Pad, _rig.Information, _rig.Popup,
                    _rig.SafeArea, null, _rig.Endings, _rig.Effects, _rig.Shaker));

            yield return Reports("no audio manager");
        }

        // A sound nobody authored is silence, but a row that names a moment and
        // then names no clip is a slip the Inspector cannot show.
        [UnityTest]
        public IEnumerator ACatalogRowWithoutAClipIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Catalog.Configure(new[] { new SoundEntry(SoundId.AppleEat, null, 1f) });

            yield return Reports("no clip for AppleEat");
        }

        [UnityTest]
        public IEnumerator AMissingEndingSettingsIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Installer.Configure(_rig.LevelFiles, GameRig.Interval,
                new SceneViews(_rig.Visuals, _rig.Framing, _rig.Pad, _rig.Information, _rig.Popup,
                    _rig.SafeArea, _rig.Audio, null, _rig.Effects, _rig.Shaker));

            yield return Reports("no ending settings");
        }

        [UnityTest]
        public IEnumerator TwoRowsForOneEndingAreReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Endings.Configure(GameRig.Instant(SoundId.Win),
                new[] { GameRig.Instant(EndingId.WallHit), GameRig.Instant(EndingId.WallHit) });

            yield return Reports("two deaths for WallHit");
        }

        [UnityTest]
        public IEnumerator AMissingEffectCatalogIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Installer.Configure(_rig.LevelFiles, GameRig.Interval,
                new SceneViews(_rig.Visuals, _rig.Framing, _rig.Pad, _rig.Information, _rig.Popup,
                    _rig.SafeArea, _rig.Audio, _rig.Endings, null, _rig.Shaker));

            yield return Reports("no effect catalog");
        }

        [UnityTest]
        public IEnumerator AnEffectPrefabThatNeverCallsBackIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);

            ParticleSystem.MainModule main = _rig.EffectPrefab.Particles.main;
            main.stopAction = ParticleSystemStopAction.None;

            yield return Reports("Stop Action");
        }

        [UnityTest]
        public IEnumerator AMissingScreenShakerIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Installer.Configure(_rig.LevelFiles, GameRig.Interval,
                new SceneViews(_rig.Visuals, _rig.Framing, _rig.Pad, _rig.Information, _rig.Popup,
                    _rig.SafeArea, _rig.Audio, _rig.Endings, _rig.Effects, null));

            yield return Reports("no screen shaker");
        }

        [UnityTest]
        public IEnumerator AShakerWithoutATargetIsReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Shaker.Configure(null);

            yield return Reports("no target assigned");
        }

        [UnityTest]
        public IEnumerator PanelsThatCoverTheWholeScreenAreReported()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _rig.Framing.Configure(_rig.Camera, 0.6f, 0.6f);

            yield return Reports("no room for the board");
        }
    }
}
