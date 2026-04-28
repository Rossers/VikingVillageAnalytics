using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pooling
{
    public class PooledObject : MonoBehaviour
    {
        protected bool InUse { get; private set; }
        protected float CurrentLifetime => Time.timeSinceLevelLoad - _lastPullTime;

        private Pool _pool;
        private float _lastPullTime;

        protected void ReturnToPool()
        {
            _pool.ReturnObject(this);
        }

        public virtual void Initialize(Pool pool)
        {
            _pool = pool;
        }

        public virtual void PrepareForPool()
        {
            InUse = false;
        }

        public virtual void PrepareForUse()
        {
            transform.SetParent(null, true);
            _lastPullTime = Time.timeSinceLevelLoad;
            InUse = true;
        }
    }
}