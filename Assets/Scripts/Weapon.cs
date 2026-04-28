using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Pooling;
using UnityEngine;

namespace Assets.Scripts
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private Vector3 _muzzleOffset;
        [SerializeField] private float _effectiveRange = 25f;
        [SerializeField] private float _RPM = 180f;
        [SerializeField] private PooledBullet _bulletPrefab;

        public float EffectiveRange => _effectiveRange;
        private Vector3 _muzzlePosition => transform.position + transform.TransformVector(_muzzleOffset);

        private float _fireCooldown;
        private float _lastFireTime;

        private void Awake()
        {
            _fireCooldown = 60f / _RPM;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_muzzlePosition, transform.forward);
        }

        public void ApplyFireInput(int team, Color color, Vector3 aimTarget)
        {
            if (Time.timeSinceLevelLoad < _lastFireTime + _fireCooldown) return;

            _lastFireTime = Time.timeSinceLevelLoad;

            Quaternion rotation = Quaternion.LookRotation((aimTarget - _muzzlePosition).normalized);
            PooledBullet pooledBullet = Pooler.GetPooledObject(_bulletPrefab, 50) as PooledBullet;
            if (pooledBullet != null)
            {
                pooledBullet.transform.SetParent(Hierarchy.GetParent(Hierarchy.HierarchyCategory.Bullets));
                pooledBullet.transform.position = _muzzlePosition;
                pooledBullet.transform.rotation = rotation;
                pooledBullet.Initialize(team, color);
            }
        }
    }
}
