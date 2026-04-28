using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Pooling
{
    public class Pool
    {
        private Stack<PooledObject> _pool;
        private PooledObject _prefab;
        private Transform _parent;

        public Pool(PooledObject prefab, Transform root, int minCount)
        {
            _pool = new Stack<PooledObject>();
            _prefab = prefab;
            GameObject poolParent = new GameObject("Pool: " + prefab.gameObject.name);
            _parent = poolParent.transform;
            _parent.SetParent(root);
            _parent.transform.localPosition = Vector3.zero;
            
            for (int i = 0; i < minCount; i++)
            {
                AddObjectToPool();
            }
        }

        private void AddObjectToPool()
        {
            PooledObject newObject = Object.Instantiate(_prefab);
            newObject.Initialize(this);
            ReturnObject(newObject);
        }

        public PooledObject GetObject()
        {
            if(_pool.Count < 1) AddObjectToPool();
            PooledObject pooledObject = _pool.Pop();
            pooledObject.PrepareForUse();

            return pooledObject;
        }

        public void ReturnObject(PooledObject pooledObject)
        {
            pooledObject.PrepareForPool();
            pooledObject.transform.SetParent(_parent);
            pooledObject.transform.localPosition = Vector3.zero;
            _pool.Push(pooledObject);
        }
    }
}
