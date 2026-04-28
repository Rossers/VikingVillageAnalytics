using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public struct VisPair
    {
        public Vector3 Origin;
        public Unit Target;
        public float Distance;
        public AIController Controller;
    }

    public class VisibilityManager : MonoBehaviour
    {
        private static VisibilityManager _instance;
        public static VisibilityManager Instance
        {
            get { return _instance; }
            set
            {
                if (_instance != null)
                {
                    Debug.LogWarning("Duplicate singleton created, destroying", value.gameObject);
                    Destroy(value.gameObject);
                    return;
                }

                _instance = value;
            }
        }

        [SerializeField] private int _rayCount = 256;
        [SerializeField] private int _minCommandsPerJob = 1;
        [SerializeField] private LayerMask _visibilityBlockingMask;
        [SerializeField] private bool _visualize = true;

        private NativeArray<RaycastHit> _results;
        private NativeArray<RaycastCommand> _commands;
        private RaycastHit[] _hits;

        private VisPair[] _pairs;
        private int _pairCount;
        private int _unclampedPairCount;
        private int _maxPairCount;

        private void Awake()
        {
            Instance = this;

            _results = new NativeArray<RaycastHit>(_rayCount, Allocator.Persistent);
            _commands = new NativeArray<RaycastCommand>(_rayCount, Allocator.Persistent);
            _hits = new RaycastHit[_rayCount];
            _pairs = new VisPair[_rayCount];
        }

        public void RegisterVisCheck(VisPair pair)
        {
            _unclampedPairCount++;
            _maxPairCount = Mathf.Max(_maxPairCount, _unclampedPairCount);
            if (_pairCount >= _rayCount) return;
            _pairs[_pairCount++] = pair; 
        }

        private void Update()
        {
            int count = Mathf.Min(_pairCount, _rayCount);
            for (int i = 0; i < count; i++)
            {
                VisPair pair = _pairs[i];
                Vector3 origin = pair.Origin;
                Vector3 direction = (pair.Target.Center - origin).normalized;
                _commands[i] = new RaycastCommand(origin, direction, pair.Distance, _visibilityBlockingMask);
                _hits[i] = new RaycastHit();
            }

            RaycastCommand.ScheduleBatch(_commands, _results, _minCommandsPerJob).Complete();
            for (int i = 0; i < _commands.Length; i++)
            {
                _hits[i] = _results[i];
            }

            for (int i = 0; i < count; i++)
            {
                if (_hits[i].collider == null)
                {
                    if(_visualize) Debug.DrawLine(_pairs[i].Origin, _pairs[i].Target.Center, Color.green);
                    _pairs[i].Controller.VisiblitySuccess(_pairs[i].Target);
                    continue;
                }

                if (_visualize) Debug.DrawLine(_pairs[i].Origin, _hits[i].point, Color.red);
            }

            _pairCount = 0;
            _unclampedPairCount = 0;
        }

        private void OnDestroy()
        {
            _results.Dispose();
            _commands.Dispose();

            Debug.Log("Visibility Manager: max simultaneous pair requests: " + _maxPairCount + "/" + _rayCount);
        }
    }
}