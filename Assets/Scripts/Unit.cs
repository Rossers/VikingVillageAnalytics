using System;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;

namespace Assets.Scripts
{
    public class Unit : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private UnitData _data;
        [SerializeField] private int _team;

        [Header("Navigation")]
        [SerializeField] private float _stoppingDistance = 2f;

        [Header("Senses")]
        [SerializeField] private LayerMask _visibilityBlockingMask;

        [Header("Art")]
        [SerializeField] private SkinnedMeshRenderer _headMesh;

        public Vector3 Center => transform.position + Collider.center;
        public Vector3 OverheadPosition => transform.position + Vector3.up * _data.Height;
        public bool DestinationReached { get; private set; }
        public int Team => _team;
        public bool IsAlive => _health.IsAlive;
        public float AttackRange => _weapon.EffectiveRange;
        public bool Attacking { get; private set; }
        public CapsuleCollider Collider { get; private set; }
        public float HealthFill => _health.Current / _health.Max;
        public bool IsVisible => _headMesh.isVisible;

        private Health _health;
        private NavMeshAgent _navMeshAgent;
        private Vector3? _pathingDestination;
        private NavMeshPath _path;
        private Weapon _weapon;
        private Color _color;
        private WaitForSeconds _deathWait;

        public event Action<float> DamageTakenEvent; 
        public event Action<Unit> KilledEvent;
        public static event Action<Unit> UnitSpawnedEvent;

        // Telemetry 
        private TelemetryEmitter emitter;

        private void Awake()
        {
            Collider = GetComponent<CapsuleCollider>();
            _weapon = GetComponentInChildren<Weapon>();

            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.speed = _data.MoveSpeed;
            _navMeshAgent.angularSpeed = _data.TurnSpeed;

            _health = new Health();
            
            _path = new NavMeshPath();

            _deathWait = new WaitForSeconds(2f);

            DestinationReached = true;

            emitter = GetComponent<TelemetryEmitter>();
        }

        public void Initialize(UnitData data, int team, Color color)
        {
            _data = data;
            _team = team;

            _health.Initialize(data.MaxHealth);

            _color = color;
            _headMesh.material.SetColor("_EmissionColor", color);

            gameObject.name = _data.Name + ": " + team;

            UnitSpawnedEvent?.Invoke(this);
        }

        private void Update()
        {
            if (!IsAlive) return;

            if (transform.position.y < -20f) transform.position = new Vector3(transform.position.x, 10f, transform.position.z);

            if (!_pathingDestination.HasValue) return;

            if (Vector3.Distance(_pathingDestination.Value, transform.position) < _stoppingDistance) Stop();
        }

        public void MoveTo(Vector3 position)
        {
            if (!_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh) return;

            if (_navMeshAgent.CalculatePath(position, _path) && _path.status == NavMeshPathStatus.PathComplete)
            {
                _pathingDestination = position;
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(position);
                DestinationReached = false;
            }
        }

        public void Stop()
        {
            _pathingDestination = null;
            DestinationReached = true;

            if (!_navMeshAgent.enabled) return;
            _navMeshAgent.isStopped = true;
        }

        public void TurnTowards(Vector3 position)
        {
            Quaternion newRotation = Quaternion.LookRotation((position - transform.position).normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, _data.TurnSpeed * Time.deltaTime);
        }

        private Vector3[] _pathCorners = new Vector3[32];
        private void OnDrawGizmosSelected()
        {
            if (_pathingDestination.HasValue)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, _pathingDestination.Value);
            }

            if (_navMeshAgent != null && _navMeshAgent.hasPath)
            {
                Gizmos.color = Color.magenta;
                int cornercount = _navMeshAgent.path.GetCornersNonAlloc(_pathCorners);
                for (int i = 0; i < cornercount - 1; i++)
                {
                    Gizmos.DrawLine(_pathCorners[i], _pathCorners[i+1]);
                }
            }
        }

        private Ray _ray;
        public bool CanSee(Vector3 position)
        {
            _ray.origin = Center;
            _ray.direction = (position - Center).normalized;
            return !Physics.Raycast(_ray, _data.VisionDistance, _visibilityBlockingMask);
        }

        public void Attack(Vector3 targetCenter)
        {
            _weapon.ApplyFireInput(_team, _color, targetCenter);
            Attacking = true;

            // Telemetry Shoot
            emitter.EmitTelemetryRecord(AIAction.Shoot);
        }

        public void StopAttack()
        {
            Attacking = false;
        }

        public void Damage(float amount)
        {
            _health.Damage(amount);
            DamageTakenEvent?.Invoke(amount);

            // Telemetry Take Damage
            emitter.EmitTelemetryRecord(AIAction.TakeDamage);

            if (!IsAlive) Death();
        }

        private void Death()
        {
            KilledEvent?.Invoke(this);
            Collider.enabled = false;
            _navMeshAgent.enabled = false;
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            yield return _deathWait;

            Destroy(gameObject);
        }
    }
}