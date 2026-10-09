using System;
using UnityEngine;

namespace Game.Runtime
{
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class EffectView : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particles;

        private Action<EffectView> _finished;

        public ParticleSystem Particles
        {
            get { return _particles; }
        }

        public void Initialize(Action<EffectView> finished)
        {
            if (finished == null)
            {
                throw new ArgumentNullException(nameof(finished));
            }

            _finished = finished;
        }

        public void Play(Vector3 position, float scale)
        {
            transform.localPosition = position;
            transform.localScale = new Vector3(scale, scale, scale);

            _particles.Play(true);
        }

        public void Stop()
        {
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnParticleSystemStopped()
        {
            if (_finished != null)
            {
                _finished(this);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(ParticleSystem particles)
        {
            _particles = particles;
        }
#endif
    }
}
