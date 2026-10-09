using System;
using Game.Core;
using Game.Input;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class GameInstaller : MonoBehaviour
    {
        [SerializeField] private TextAsset[] _levelFiles;
        [SerializeField, Min(0.01f)] private float _tickInterval = 0.15f;
        [SerializeField] private SceneViews _views;

        private Player_Input _actions;
        private Playthrough _playthrough;
        private RunTimeline _timeline;

        private BoardView _board;
        private GroundView _ground;
        private SnakeView _snake;
        private AppleView _food;
        private AppleView _goldenApple;
        private EffectPlayer _effects;

        private void Awake()
        {
            Result createResult = Create();
            if (createResult.IsSuccess)
            {
                return;
            }

            Debug.LogError(createResult.Error, this);
            enabled = false;
        }

        private void Start()
        {
            _playthrough.Begin();
        }

        private void Update()
        {
            _playthrough.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_actions == null)
            {
                return;
            }

            _actions.Dispose();
            _actions = null;
        }

        private Result Create()
        {
            LevelSequence levels;
            Result loadResult = LevelSequenceLoader.TryLoad(_levelFiles, out levels);
            if (!loadResult.IsSuccess)
            {
                return loadResult;
            }

            Result viewsResult = _views.Validate();
            if (!viewsResult.IsSuccess)
            {
                return viewsResult;
            }

            _actions = new Player_Input();
            _timeline = new RunTimeline(_views.Endings, _views.Audio, _views.Shaker, gameObject);

            Draw(levels);

            GameRun run = new GameRun(_actions, new RunPieces(_snake, _food, _goldenApple, _views.Audio, _effects),
                _views.Information, _views.EndPopup, _views.Audio, _timeline);

            _playthrough = new Playthrough(levels, run,
                new BoardPresentation(_board, _ground, _views.Framing), _views.EndPopup, _tickInterval,
                new SystemRandomSource(Environment.TickCount), Reading(), new PlayerPrefsLevelPersistent(),
                _views.Audio);

            return Result.Success();
        }

        private void Draw(LevelSequence levels)
        {
            CellStock stock = CellStock.For(levels);
            BoardVisuals visuals = _views.Visuals;

            CellViewPool environment = new CellViewPool(visuals.EnvironmentPrefab, transform,
                stock.Environment + stock.Ground);
            CellViewPool organic = new CellViewPool(visuals.OrganicPrefab, transform, stock.Organic);

            _board = new BoardView(environment, Root("Board"), visuals);
            _ground = new GroundView(environment, Root("Ground"), visuals);
            _snake = new SnakeView(organic, Root("Snake"), visuals);
            _food = new AppleView(organic, Root("Food"), visuals.Food, visuals.PieceHeight, 1f,
                visuals.AppleAppear);
            _goldenApple = new AppleView(organic, Root("GoldenApple"), visuals.GoldenApple,
                visuals.GoldenAppleHeight, visuals.GoldenAppleScale, visuals.AppleAppear);

            _effects = new EffectPlayer(_views.Effects, Root("Effects"), visuals.PieceHeight);
        }

        private Transform Root(string name)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(transform, false);

            return root;
        }

        private IInputSource Reading()
        {
            ArrowInput arrows = new ArrowInput();
            _views.ArrowPad.Initialize(arrows);

            IInputSource[] sources =
            {
                arrows,
                new PlayerInputSource(_actions.Player.Move, new ScreenToBoard(_views.Framing.Yaw))
            };

            return new InputOrchestrator(sources);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public GameSession Session
        {
            get { return _playthrough == null ? null : _playthrough.Session; }
        }

        public int LevelIndex
        {
            get { return _playthrough == null ? -1 : _playthrough.LevelIndex; }
        }

        public BoardView Board { get { return _board; } }
        public GroundView Ground { get { return _ground; } }
        public SnakeView Snake { get { return _snake; } }
        public AppleView Food { get { return _food; } }
        public AppleView GoldenApple { get { return _goldenApple; } }

        public void Configure(TextAsset[] levelFiles, float tickInterval, SceneViews views)
        {
            _levelFiles = levelFiles;
            _tickInterval = tickInterval;
            _views = views;
        }
#endif
    }
}
