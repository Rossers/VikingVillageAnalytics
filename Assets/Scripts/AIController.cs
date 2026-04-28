using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] private float _wanderDistance = 10f;
        [SerializeField] private float _retargetTime = 0.5f;

        private Unit _possessed;
        private float _lastRetargetTime;
        private Unit _target;
        private float _attackDistanceMultiplier;
        private bool _selected;

        protected void Start()
        {
            _possessed = GetComponent<Unit>();
            _attackDistanceMultiplier = Random.Range(0.5f, 1f);
        }

        private void Update()
        {
            if (!_possessed.IsAlive) return;

            if (Time.timeSinceLevelLoad > _lastRetargetTime + _retargetTime)
            {
                _lastRetargetTime = Time.timeSinceLevelLoad;
                FindTarget();
            }

            if (_target != null && _target.IsAlive)
            {
                if (Vector3.Distance(_possessed.Center, _target.Center) < _possessed.AttackRange * _attackDistanceMultiplier)
                {
                    _possessed.Stop();
                    _possessed.TurnTowards(_target.Center);
                    _possessed.Attack(_target.Center);
                    return;
                }

                if (_possessed.DestinationReached)
                {
                    _possessed.MoveTo(_target.transform.position);
                    _possessed.StopAttack();
                }
            }

            if (_target == null && _possessed.DestinationReached)
            {
                Vector3 patrolPoint = UnitSpawner.Instance.GetPatrolDestination(_possessed.Team);
                Vector2 pointOnCircle = Random.insideUnitCircle * _wanderDistance;
                Vector3 wanderPoint = patrolPoint + new Vector3(pointOnCircle.x, 0f, pointOnCircle.y);
                _possessed.MoveTo(wanderPoint);
                _possessed.StopAttack();
            }

            _selected = false;
        }

        private void FindTarget()
        {
            for (int i = 0; i < UnitSpawner.Instance.NotTeams[_possessed.Team].Count; i++)
            {
                Unit unit = UnitSpawner.Instance.NotTeams[_possessed.Team][i];
                if (unit == null || !unit.IsAlive || unit.Team == _possessed.Team) continue;

                float distance = Vector3.Distance(_possessed.Center, unit.Center);
                if (distance > _possessed.AttackRange) continue;
                if (_selected) Debug.DrawLine(_possessed.Center, unit.Center, Color.yellow);

                VisibilityManager.Instance.RegisterVisCheck(new VisPair()
                {
                    Controller = this,
                    Distance = distance,
                    Origin = _possessed.Center,
                    Target = unit
                });
            }
        }

        private void OnDrawGizmosSelected()
        {
            _selected = true;

            if (_target == null) return;

            if (Vector3.Distance(_possessed.Center, _target.Center) < _possessed.AttackRange)
            {
                Debug.DrawLine(_possessed.Center, _target.Center, Color.red);
                return;
            }

            Debug.DrawLine(_possessed.Center, _target.Center, Color.yellow);
        }

        public void VisiblitySuccess(Unit target)
        {
            _target = target;
        }
    }
}