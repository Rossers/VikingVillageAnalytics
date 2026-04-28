using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class UnitAnimationController : MonoBehaviour
    {
        private Animator _animator;
        private NavMeshAgent _navMeshAgent;
        private Unit _unit;

        private int _paramForward;
        private int _paramShooting;
        private int _paramDead;

        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _unit = GetComponent<Unit>();

            _paramForward = Animator.StringToHash("Forward");
            _paramShooting = Animator.StringToHash("Shooting");
            _paramDead = Animator.StringToHash("Dead");
        }

        private void Update()
        {
            _animator.SetFloat(_paramForward, _navMeshAgent.velocity.magnitude / _navMeshAgent.speed);
            _animator.SetBool(_paramShooting, _unit.Attacking);
            _animator.SetBool(_paramDead, !_unit.IsAlive);
        }
    }
}