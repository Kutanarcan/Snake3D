using System;
using Game.Core;
using Game.Input;

namespace Game.Runtime
{
    public sealed class GameRun
    {
        private readonly Player_Input _actions;
        private readonly RunPieces _pieces;
        private readonly LevelInformationView _information;
        private readonly LevelEndPopupView _popup;
        private readonly ISoundPlayer _sound;
        private readonly RunTimeline _timeline;
        private readonly Action _revealPopup;

        private GameSession _session;

        public GameRun(Player_Input actions, RunPieces pieces, LevelInformationView information,
            LevelEndPopupView popup, ISoundPlayer sound, RunTimeline timeline)
        {
            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            if (pieces == null)
            {
                throw new ArgumentNullException(nameof(pieces));
            }

            if (information == null)
            {
                throw new ArgumentNullException(nameof(information));
            }

            if (popup == null)
            {
                throw new ArgumentNullException(nameof(popup));
            }

            if (sound == null)
            {
                throw new ArgumentNullException(nameof(sound));
            }

            if (timeline == null)
            {
                throw new ArgumentNullException(nameof(timeline));
            }

            _actions = actions;
            _pieces = pieces;
            _information = information;
            _popup = popup;
            _sound = sound;
            _timeline = timeline;
            _revealPopup = RevealPopup;
        }

        public GameSession Session
        {
            get { return _session; }
        }

        public void Begin(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            _session = session;
            _session.Ate += OnAte;
            _session.Deflected += OnDeflected;
            _session.Turned += OnTurned;
            _session.Won += OnFinished;
            _session.Lost += OnFinished;
            _actions.Enable();

            _session.StartRun();

            _pieces.Show(_session.Game.Snake);
            _pieces.FollowApple(_session.Game.Apples.Plain);
            _information.Show(_session.FoodEaten, _session.FoodGoal);
            _popup.Hide();

            Draw();
        }

        public void End()
        {
            _session.Ate -= OnAte;
            _session.Deflected -= OnDeflected;
            _session.Turned -= OnTurned;
            _session.Won -= OnFinished;
            _session.Lost -= OnFinished;
            _timeline.Kill();
            _actions.Disable();

            _pieces.Hide();
            _popup.Hide();
        }

        public void Tick(float deltaTime)
        {
            if (_session.IsOver)
            {
                return;
            }

            _session.Tick(deltaTime);
            Draw();
        }

        private void OnFinished()
        {
            _actions.Disable();
            _timeline.Play(_session.Status, _session.Game, _revealPopup);
        }

        private void RevealPopup()
        {
            _popup.Show(_session.Status);
        }

        private void OnAte()
        {
            _pieces.Eaten(_session.Game);
            _information.Show(_session.FoodEaten, _session.FoodGoal);
        }

        private void OnDeflected()
        {
            _sound.Play(SoundId.TunnelEnter);
        }

        private void OnTurned()
        {
            _sound.Play(SoundId.SnakeTurn);
        }

        private void Draw()
        {
            _pieces.Draw(_session.Game, _session.NormalizedTick);
        }
    }
}
