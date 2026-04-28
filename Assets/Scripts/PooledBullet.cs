using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Pooling;
using UnityEngine;

namespace Assets.Scripts
{
    public class PooledBullet : PooledObject
    {
        [SerializeField] private float _velocity = 30f;
        [SerializeField] private float _lifeTime = 2f;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private PooledParticleSystem _damageParticles;

        private Rigidbody _rigidbody;
        private MeshRenderer _meshRenderer;
        private int _team;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        public void Initialize(int team, Color color)
        {
            _team = team;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = transform.forward * _velocity;
            _meshRenderer.material.SetColor("_EmissionColor", color);
        }

        private void Update()
        {
            if(InUse && CurrentLifetime >= _lifeTime) ReturnToPool();
        }

        private void OnTriggerEnter(Collider other)
        {
            Unit hitUnit = other.gameObject.GetComponent<Unit>();
            if (hitUnit != null)
            {
                if (hitUnit.Team == _team) return;

                hitUnit.Damage(_damage);
                PooledParticleSystem particles = Pooler.GetPooledObject(_damageParticles, 20) as PooledParticleSystem;
                if (particles != null)
                {
                    particles.transform.position = transform.position;
                    particles.transform.rotation = Quaternion.LookRotation(-transform.forward);
                    particles.transform.SetParent(Hierarchy.GetParent(Hierarchy.HierarchyCategory.FX));
                    particles.Play();
                }
            }

            ReturnToPool();
        }

        public override void PrepareForPool()
        {
            base.PrepareForPool();

            _rigidbody.isKinematic = true;
        }
    }
}
