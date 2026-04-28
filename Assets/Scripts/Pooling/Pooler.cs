using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pooling
{
    public class Pooler
    {
        private static Dictionary<PooledObject, Pool> _pools = new Dictionary<PooledObject, Pool>();
        private static Transform _root;
        private static Transform Root
        {
            get
            {
                if (_root == null)
                {
                    GameObject newRoot = new GameObject("PoolRoot");
                    _root = newRoot.transform;
                }
                return _root;
            }
        }

        public static PooledObject GetPooledObject(PooledObject prefab, int minCount = 5)
        {
            InitializePool(prefab, minCount);
            return _pools[prefab].GetObject();
        }

        public static void InitializePool(PooledObject prefab, int minCount = 5)
        {
            if (!_pools.ContainsKey(prefab))
            {
                _pools.Add(prefab, new Pool(prefab, Root, minCount));
            }
        }
    }
}