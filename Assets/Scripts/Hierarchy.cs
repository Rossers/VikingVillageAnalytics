using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Hierarchy
    {
        public enum HierarchyCategory
        {
            Units,
            FX,
            Bullets,
        }

        private static Dictionary<HierarchyCategory, Transform> _parents = new Dictionary<HierarchyCategory, Transform>();

        public static Transform GetParent(HierarchyCategory category)
        {
            if (!_parents.ContainsKey(category))
            {
                CreateParent(category);
            }

            return _parents[category];
        }

        private static Transform CreateParent(HierarchyCategory category)
        {
            GameObject newParent = new GameObject(category.ToString());
            _parents.Add(category, newParent.transform);
            return newParent.transform;
        }
    }
}
