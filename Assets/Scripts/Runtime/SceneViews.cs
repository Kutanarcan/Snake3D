using System;
using Game.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Runtime
{   
    [Serializable]
    public sealed class SceneViews
    {
        [SerializeField] private BoardVisuals _visuals;
        [SerializeField] private BoardCamera _boardCamera;
        [SerializeField] private ArrowPadView _arrowPad;
        [SerializeField] private LevelInformationView _levelInformation;
        [SerializeField] private LevelEndPopupView _levelEndPopup;
        [SerializeField] private SafeAreaView _safeArea;
        [SerializeField] private AudioManager _audio;
        [SerializeField] private EndingSettings _endings;
        [SerializeField] private EffectCatalog _effects;
        [SerializeField] private ScreenShakerView _shaker;

        public BoardVisuals Visuals { get { return _visuals; } }
        public BoardCamera Framing { get { return _boardCamera; } }
        public ArrowPadView ArrowPad { get { return _arrowPad; } }
        public LevelInformationView Information { get { return _levelInformation; } }
        public LevelEndPopupView EndPopup { get { return _levelEndPopup; } }
        public SafeAreaView SafeArea { get { return _safeArea; } }
        public AudioManager Audio { get { return _audio; } }
        public EndingSettings Endings { get { return _endings; } }
        public EffectCatalog Effects { get { return _effects; } }
        public ScreenShakerView Shaker { get { return _shaker; } }

        public Result Validate()
        {
            Result assigned = FirstFailure(
                Assigned(_visuals, "board visuals"),
                Assigned(_boardCamera, "board camera"),
                Assigned(_arrowPad, "arrow pad"),
                Assigned(_levelInformation, "level information view"),
                Assigned(_levelEndPopup, "level end popup"),
                Assigned(_safeArea, "safe area view"),
                Assigned(_audio, "audio manager"),
                Assigned(_endings, "ending settings"),
                Assigned(_effects, "effect catalog"),
                Assigned(_shaker, "screen shaker"));

            if (!assigned.IsSuccess)
            {
                return assigned;
            }

            return FirstFailure(
                _visuals.Validate(),
                _boardCamera.Validate(),
                _arrowPad.Validate(),
                _levelInformation.Validate(),
                _levelEndPopup.Validate(),
                _safeArea.Validate(),
                _audio.Validate(),
                _endings.Validate(),
                _effects.Validate(),
                _shaker.Validate());
        }

        private static Result Assigned(Object view, string field)
        {
            return view == null
                ? Result.Failure("GameInstaller has no " + field + " assigned.")
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
        public SceneViews()
        {
        }

        public SceneViews(BoardVisuals visuals, BoardCamera boardCamera, ArrowPadView arrowPad,
            LevelInformationView levelInformation, LevelEndPopupView levelEndPopup, SafeAreaView safeArea,
            AudioManager audio, EndingSettings endings, EffectCatalog effects, ScreenShakerView shaker)
        {
            _visuals = visuals;
            _boardCamera = boardCamera;
            _arrowPad = arrowPad;
            _levelInformation = levelInformation;
            _levelEndPopup = levelEndPopup;
            _safeArea = safeArea;
            _audio = audio;
            _endings = endings;
            _effects = effects;
            _shaker = shaker;
        }
#endif
    }
}
