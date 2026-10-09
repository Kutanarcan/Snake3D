using Game.Core;
using Game.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests.PlayMode
{
    public sealed class GameRig
    {
        public const string ValidLevel = @"{
            ""Width"": 16,
            ""Height"": 16,
            ""StartX"": 8,
            ""StartY"": 8,
            ""StartDirection"": ""Right"",
            ""SnakeLength"": 3,
            ""FoodGoal"": 10
        }";

        public const string SecondLevel = @"{
            ""Width"": 12,
            ""Height"": 12,
            ""StartX"": 6,
            ""StartY"": 6,
            ""StartDirection"": ""Right"",
            ""SnakeLength"": 3,
            ""FoodGoal"": 10
        }";

        // An 8x8 board with three walls: two corners, and one two cells ahead of
        // a head that starts at (4,4) facing Right. The body trails back to
        // (2,4), so none of the three sits under it.
        public const string WalledLevel = @"{
            ""Width"": 8,
            ""Height"": 8,
            ""StartX"": 4,
            ""StartY"": 4,
            ""StartDirection"": ""Right"",
            ""SnakeLength"": 3,
            ""FoodGoal"": 5,
            ""Items"": [
                { ""Type"": ""Wall"", ""X"": 0, ""Y"": 0 },
                { ""Type"": ""Wall"", ""X"": 6, ""Y"": 4 },
                { ""Type"": ""Wall"", ""X"": 7, ""Y"": 7 }
            ]
        }";

        // A tunnel two cells ahead of a head that starts at (2,4) facing Right.
        // The body trails back to (0,4), clear of it.
        public const string TunnelLevel = @"{
            ""Width"": 8,
            ""Height"": 8,
            ""StartX"": 2,
            ""StartY"": 4,
            ""StartDirection"": ""Right"",
            ""SnakeLength"": 3,
            ""FoodGoal"": 5,
            ""Items"": [
                { ""Type"": ""Tunnel"", ""X"": 4, ""Y"": 4 }
            ]
        }";

        // Cooldowns short enough that a test does not have to wait: one tick to
        // arrive, and long enough on the board to be caught being drawn.
        public const string GoldenAppleLevel = @"{
            ""Width"": 8,
            ""Height"": 8,
            ""StartX"": 4,
            ""StartY"": 4,
            ""StartDirection"": ""Right"",
            ""SnakeLength"": 3,
            ""FoodGoal"": 5,
            ""GoldenApple"": { ""SpawnAfterSeconds"": 0.05, ""StaysForSeconds"": 5.0 }
        }";

        public const float Interval = 0.05f;
        public const string WonTitle = "Level Complete";
        public const string LostTitle = "Game Over";
        public const string WonButton = "Next";
        public const string LostButton = "Try Again";

        public readonly GameObject Host;
        public readonly TextAsset[] LevelFiles;
        public readonly CellView CellPrefab;
        public readonly Mesh DarkMesh;
        public readonly Mesh LightMesh;
        public readonly Mesh PieceMesh;
        public readonly Mesh WallMesh;
        public readonly Mesh TunnelMesh;
        public readonly GroundVisual[] GroundTable;
        public readonly BoardVisuals Visuals;
        public readonly Camera Camera;
        public readonly BoardCamera Framing;
        public readonly ArrowPadView Pad;
        public readonly ArrowView[] Arrows;
        public readonly ActionButtonView[] Buttons;
        public readonly LevelInformationView Information;
        public readonly TMP_Text FoodAmount;
        public readonly LevelEndPopupView Popup;
        public readonly LevelEndPopupSettings PopupSettings;
        public readonly TMP_Text PopupTitle;
        public readonly TMP_Text PopupButtonLabel;
        public readonly Image PopupStar;
        public readonly ActionButtonView PopupButton;
        public readonly Texture2D StarTexture;
        public readonly Sprite WonStar;
        public readonly Sprite LostStar;
        public readonly SafeAreaView SafeArea;
        public readonly SoundCatalog Catalog;
        public readonly AudioManager Audio;
        public readonly EndingSettings Endings;
        public readonly EffectView EffectPrefab;
        public readonly EffectCatalog Effects;
        public readonly ScreenShakerView Shaker;
        public readonly SceneViews Views;
        public readonly GameInstaller Installer;

        public GameRig(params string[] jsons)
        {
            LevelFiles = NewLevelFiles(jsons);

            Host = new GameObject("installer");
            Host.SetActive(false);

            DarkMesh = new Mesh { name = "dark" };
            LightMesh = new Mesh { name = "light" };
            PieceMesh = new Mesh { name = "piece" };
            WallMesh = new Mesh { name = "wall" };
            TunnelMesh = new Mesh { name = "tunnel" };

            CellPrefab = NewPart<CellView>("cell");
            CellPrefab.Configure(CellPrefab.gameObject.AddComponent<MeshFilter>());
            CellPrefab.gameObject.SetActive(false);

            GroundTable = new[]
            {
                new GroundVisual(CellGroundType.Wall, WallMesh, 1f),
                new GroundVisual(CellGroundType.Tunnel, TunnelMesh, 1f)
            };
            Visuals = ScriptableObject.CreateInstance<BoardVisuals>();
            Visuals.ConfigureAppleAppear(0f);
            Visualise(CellPrefab, CellPrefab, DarkMesh, LightMesh, GroundTable, PieceMesh, PieceMesh);

            Framing = NewPart<BoardCamera>("camera");
            Framing.transform.rotation = Quaternion.Euler(30f, 135f, 0f);
            Camera = Framing.gameObject.AddComponent<Camera>();
            Framing.Configure(Camera, 0.1f, 0.25f);

            Pad = NewPart<ArrowPadView>("pad");
            Arrows = new ArrowView[4];
            Buttons = new ActionButtonView[4];

            for (int i = 0; i < Arrows.Length; i++)
            {
                Arrows[i] = NewPart<ArrowView>("arrow" + i);
                Buttons[i] = Arrows[i].gameObject.AddComponent<ActionButtonView>();
                Arrows[i].Configure(Buttons[i], (Direction)i);
            }

            Pad.Configure(Arrows);

            Information = NewPart<LevelInformationView>("information");
            FoodAmount = Information.gameObject.AddComponent<TextMeshProUGUI>();
            Information.Configure(FoodAmount);

            StarTexture = new Texture2D(1, 1);
            WonStar = NewStar("wonStar");
            LostStar = NewStar("lostStar");

            PopupSettings = ScriptableObject.CreateInstance<LevelEndPopupSettings>();
            PopupSettings.Configure(
                new LevelEndingInformation(WonTitle, WonStar, WonButton),
                new LevelEndingInformation(LostTitle, LostStar, LostButton));

            Popup = NewPart<LevelEndPopupView>("popup");
            Image dim = Popup.gameObject.AddComponent<Image>();

            PopupTitle = NewPart<TextMeshProUGUI>("popupTitle");
            PopupTitle.transform.SetParent(Popup.transform, false);

            PopupButtonLabel = NewPart<TextMeshProUGUI>("popupButtonLabel");
            PopupButtonLabel.transform.SetParent(Popup.transform, false);

            PopupStar = NewPart<Image>("popupStar");
            PopupStar.transform.SetParent(Popup.transform, false);

            PopupButton = Popup.gameObject.AddComponent<ActionButtonView>();

            Popup.Configure(PopupSettings, dim, Popup.transform, PopupTitle, PopupStar, PopupButtonLabel,
                PopupButton);

            SafeArea = NewPart<SafeAreaView>("safeArea");
            SafeArea.Configure(SafeArea.gameObject.AddComponent<RectTransform>());

            Catalog = ScriptableObject.CreateInstance<SoundCatalog>();
            Audio = NewPart<AudioManager>("audio");
            Audio.gameObject.AddComponent<AudioListener>();
            Audio.Configure(Catalog, Audio.gameObject.AddComponent<AudioSource>());

            Endings = ScriptableObject.CreateInstance<EndingSettings>();
            Endings.Configure(Instant(SoundId.Win), new[]
            {
                Instant(EndingId.WallHit), Instant(EndingId.SelfBite), Instant(EndingId.OutOfBounds)
            });

            EffectPrefab = NewEffectPrefab();
            Effects = ScriptableObject.CreateInstance<EffectCatalog>();
            Effects.Configure(new[]
            {
                new EffectEntry(EffectId.AppleEat, EffectPrefab, 2, 1f),
            });

            Shaker = NewPart<ScreenShakerView>("shaker");
            Shaker.Configure(Shaker.transform);

            Views = new SceneViews(Visuals, Framing, Pad, Information, Popup, SafeArea, Audio, Endings, Effects,
                Shaker);

            Installer = Host.AddComponent<GameInstaller>();
            Installer.Configure(LevelFiles, Interval, Views);
        }

        // Rewrites the visuals asset from the rig's own assets. A wiring test hands
        // in one null to spoil exactly one slot and leaves the rest standing.
        public void Visualise(CellView environmentPrefab, CellView organicPrefab, Mesh boardDark,
            Mesh boardLight, GroundVisual[] ground, Mesh snake, Mesh food)
        {
            Visuals.Configure(environmentPrefab, organicPrefab, boardDark, boardLight, ground,
                snake, snake, snake, food);
        }

        public Transform NewRoot(string name)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(Host.transform, false);

            return root;
        }

        public AppleView NewApple(CellViewPool pool, string name)
        {
            return new AppleView(pool, NewRoot(name), Visuals.Food, Visuals.PieceHeight, 1f, 0f);
        }

        public AppleView NewGoldenApple(CellViewPool pool, string name)
        {
            return new AppleView(pool, NewRoot(name), Visuals.GoldenApple, Visuals.GoldenAppleHeight,
                Visuals.GoldenAppleScale, 0f);
        }

        public GameInstaller Run()
        {
            Host.SetActive(true);
            return Installer;
        }

        // The rig lays the arrows out in enum order, so this is the button that
        // commands the direction asked for. A waiting run sets off on it.
        public void Tap(Direction direction)
        {
            Buttons[(int)direction].OnPointerDown(null);
        }

        public void Destroy()
        {
            Destroy(Host);
            DestroyLevelFiles();
            Destroy(DarkMesh);
            Destroy(LightMesh);
            Destroy(PieceMesh);
            Destroy(WallMesh);
            Destroy(TunnelMesh);
            Destroy(Visuals);
            Destroy(WonStar);
            Destroy(LostStar);
            Destroy(StarTexture);
            Destroy(PopupSettings);
            Destroy(Catalog);
            Destroy(Endings);
            Destroy(Effects);
        }

        public static WinEffect Instant(SoundId cheer)
        {
            return new WinEffect(cheer, 0f);
        }

        public static DeathEffect Instant(EndingId id)
        {
            return new DeathEffect(id, SoundId.WallHit, 0f, SoundId.Lose);
        }

        public EffectView NewEffectPrefab()
        {
            EffectView view = NewPart<EffectView>("effect");
            ParticleSystem particles = view.GetComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.stopAction = ParticleSystemStopAction.Callback;

            view.Configure(particles);
            view.gameObject.SetActive(false);

            return view;
        }

        private static TextAsset[] NewLevelFiles(string[] jsons)
        {
            if (jsons == null)
            {
                return null;
            }

            TextAsset[] files = new TextAsset[jsons.Length];

            for (int i = 0; i < jsons.Length; i++)
            {
                files[i] = jsons[i] == null ? null : new TextAsset(jsons[i]) { name = "level_0" + (i + 1) };
            }

            return files;
        }

        private void DestroyLevelFiles()
        {
            if (LevelFiles == null)
            {
                return;
            }

            for (int i = 0; i < LevelFiles.Length; i++)
            {
                Destroy(LevelFiles[i]);
            }
        }

        private Sprite NewStar(string name)
        {
            Sprite star = Sprite.Create(StarTexture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f);
            star.name = name;

            return star;
        }

        private static void Destroy(Object part)
        {
            if (part != null)
            {
                Object.Destroy(part);
            }
        }

        private T NewPart<T>(string name) where T : Component
        {
            T part = new GameObject(name).AddComponent<T>();
            part.transform.SetParent(Host.transform, false);

            return part;
        }
    }
}
