using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pooling
{
    public class PooledParticleSystem : PooledObject
    {
        [SerializeField] private float _duration = 2f;

        private ParticleSystem _particleSystem;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        public override void PrepareForPool()
        {
            _particleSystem.Stop(true);
            _particleSystem.Clear(true);

            base.PrepareForPool();
        }

        public void Play()
        {
            _particleSystem.Play(true);
        }

        private void Update()
        {
            if (InUse && CurrentLifetime >= _duration) ReturnToPool();
        }
    }
}
