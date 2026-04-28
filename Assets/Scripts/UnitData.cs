using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(menuName = "New Unit Data")]
    public class UnitData : ScriptableObject
    {
        [Header("Display")]
        public string Name = "Soldier";
        public float Height = 2f;

        [Header("Health")]
        public float MaxHealth = 100f;

        [Header("Movement")]
        public float MoveSpeed = 5f;
        public float TurnSpeed = 360f;
        public float StrafeSpeed = 5f;

        [Header("Senses")]
        public float VisionDistance = 50f;
    }
}
