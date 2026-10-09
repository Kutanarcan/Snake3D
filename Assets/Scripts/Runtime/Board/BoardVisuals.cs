using Game.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Runtime
{  
    [CreateAssetMenu(fileName = "BoardVisuals", menuName = "Snake/Board Visuals")]
    public sealed class BoardVisuals : ScriptableObject
    {
        [SerializeField] private CellView _environmentPrefab;
        [SerializeField] private CellView _organicPrefab;

        [SerializeField] private Mesh _boardDark;
        [SerializeField] private Mesh _boardLight;
        [SerializeField] private float _boardHeight;
        [SerializeField] private GroundVisual[] _ground;

        [SerializeField] private Mesh _snakeHead;
        [SerializeField] private Mesh _snakeBody;
        [SerializeField] private Mesh _snakeTail;
        [SerializeField] private Mesh _food;
        [SerializeField] private Mesh _goldenApple;
        [SerializeField, Min(0.1f)] private float _goldenAppleScale = 3f;
        [SerializeField] private float _goldenAppleHeight = 1f;
        [SerializeField] private float _pieceHeight = 1f;

        [SerializeField, Range(0f, 1f)] private float _stepEase = 0.5f;
        [SerializeField, Min(0f)] private float _appleAppear = 0.25f;

        public CellView EnvironmentPrefab { get { return _environmentPrefab; } }
        public CellView OrganicPrefab { get { return _organicPrefab; } }

        public Mesh BoardDark { get { return _boardDark; } }
        public Mesh BoardLight { get { return _boardLight; } }
            
        public float BoardHeight { get { return _boardHeight; } }
        public float PieceHeight { get { return _pieceHeight; } }

        public Mesh SnakeHead { get { return _snakeHead; } }
        public Mesh SnakeBody { get { return _snakeBody; } }
        public Mesh SnakeTail { get { return _snakeTail; } }
        public Mesh Food { get { return _food; } }

        // A golden apple is the same fruit three times the size, so a level that
        // never assigned its own mesh borrows the plain one rather than refusing
        // to start.
        public Mesh GoldenApple { get { return _goldenApple == null ? _food : _goldenApple; } }

        // Its own scale and its own height: blowing a mesh up around its pivot
        // moves where it sits on the board, so the two are authored together
        // rather than borrowing the height every other piece uses.
        public float GoldenAppleScale { get { return _goldenAppleScale; } }

        public float GoldenAppleHeight { get { return _goldenAppleHeight; } }

        public float StepEase { get { return _stepEase; } }
        public float AppleAppear { get { return _appleAppear; } }

        public GroundVisual GroundFor(CellGroundType type)
        {
            if (_ground == null)
            {
                return null;
            }

            for (int i = 0; i < _ground.Length; i++)
            {
                if (_ground[i] != null && _ground[i].Type == type)
                {
                    return _ground[i];
                }
            }

            return null;
        }

        public Result Validate()
        {
            Result assets = FirstFailure(
                Assigned(_environmentPrefab, "environment prefab"),
                Assigned(_organicPrefab, "organic prefab"),
                Assigned(_boardDark, "dark board mesh"),
                Assigned(_boardLight, "light board mesh"),
                Assigned(_snakeHead, "snake head mesh"),
                Assigned(_snakeBody, "snake body mesh"),
                Assigned(_snakeTail, "snake tail mesh"),
                Assigned(_food, "food mesh"));

            return assets.IsSuccess ? ValidateGround() : assets;
        }

        private Result ValidateGround()
        {
            CellGroundType[] types = (CellGroundType[])System.Enum.GetValues(typeof(CellGroundType));

            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == CellGroundType.Open)
                {
                    continue;
                }

                GroundVisual visual = GroundFor(types[i]);

                if (visual == null || visual.Mesh == null)
                {
                    return Result.Failure("BoardVisuals has no mesh for " + types[i] + ".");
                }
            }

            return Result.Success();
        }

        private static Result Assigned(Object asset, string field)
        {
            return asset == null
                ? Result.Failure("BoardVisuals has no " + field + " assigned.")
                : Result.Success();
        }

        private static Result FirstFailure(params Result[] results)
        {
            for (int i = 0; i < results.Length; i++)
            {
                if (!results[i].IsSuccess)
                {
                    return results[i];
                }
            }

            return Result.Success();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void ConfigureAppleAppear(float seconds)
        {
            _appleAppear = seconds;
        }

        public void Configure(CellView environmentPrefab, CellView organicPrefab, Mesh boardDark,
            Mesh boardLight, GroundVisual[] ground, Mesh snakeHead, Mesh snakeBody, Mesh snakeTail, Mesh food)
        {
            _environmentPrefab = environmentPrefab;
            _organicPrefab = organicPrefab;
            _boardDark = boardDark;
            _boardLight = boardLight;
            _ground = ground;
            _snakeHead = snakeHead;
            _snakeBody = snakeBody;
            _snakeTail = snakeTail;
            _food = food;
        }
#endif
    }
}
