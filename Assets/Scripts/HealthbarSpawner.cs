using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Pooling;
using UnityEngine;

namespace Assets.Scripts
{
    public class HealthbarSpawner : MonoBehaviour
    {
        [SerializeField] private Healthbar _healthbarPrefab;

        private void Awake()
        {
            Unit.UnitSpawnedEvent += OnUnitSpawned;
        }

        private void OnUnitSpawned(Unit unit)
        {
            Healthbar newHealthbar = Pooler.GetPooledObject(_healthbarPrefab, 10) as Healthbar;
            if (newHealthbar != null)
            {
                newHealthbar.transform.SetParent(transform);
                newHealthbar.SetTarget(unit);
            }
        }
    }
}
