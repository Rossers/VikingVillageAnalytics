using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Assets.Scripts
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Time")]
        [SerializeField] private float _startTime = 0.1f;
        [SerializeField] private float _progressRate = 0.1f;
        [SerializeField] private float _manualMultiplier = 10f;
        [SerializeField] private AnimationCurve _progressModifier;

        [Header("Directional Light")]
        [SerializeField] private Light _light;
        [SerializeField] private Gradient _color;
        [SerializeField] private AnimationCurve _xDir;
        [SerializeField] private AnimationCurve _yDir;
        [SerializeField] private AnimationCurve _zDir;

        [Header("Skybox")]
        [SerializeField] private Material _dayMaterial;
        [SerializeField] private Material _nightMaterial;

        [Header("Reflection Probes")]
        [SerializeField] private bool _updateProbes = true;
        [SerializeField] private ReflectionProbe[] _dayProbes;
        [SerializeField] private ReflectionProbe[] _nightProbes;

        [Header("Post Processing")]
        [SerializeField] private PostProcessVolume _dayPPVolume;
        [SerializeField] private PostProcessVolume _nightPPVolume;

        [Header("Hotkeys")]
        [SerializeField] private KeyCode _toggleProgression = KeyCode.Alpha0;
        [SerializeField] private KeyCode _advanceTime = KeyCode.Equals;
        [SerializeField] private KeyCode _rewindTime = KeyCode.Minus;

        [Header("Debug")]
        [SerializeField] private float _progress;
        [SerializeField] private float _lightProgress;
        [SerializeField] private float _skyboxProgress;

        private bool _enabled = false;
        private float _totalTime;

        private void Start()
        {
            RenderSettings.skybox = new Material(_dayMaterial);
            _totalTime = _startTime;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleProgression)) _enabled = !_enabled;
            if (Input.GetKey(_advanceTime)) _totalTime += _progressRate * Time.deltaTime * _manualMultiplier;
            if (Input.GetKey(_rewindTime)) _totalTime -= _progressRate * Time.deltaTime * _manualMultiplier;

            if (_enabled) _totalTime += _progressRate * Time.deltaTime;
            _progress = _progressModifier.Evaluate(_totalTime - Mathf.FloorToInt(_totalTime));
            _lightProgress = (_progress > 0.5f ? _progress - 0.5f : _progress) * 2f;

            Vector3 lightDirection = new Vector3(_xDir.Evaluate(_lightProgress), _yDir.Evaluate(_lightProgress), _zDir.Evaluate(_lightProgress));
            _light.transform.rotation = Quaternion.LookRotation(lightDirection);
            _light.color = _color.Evaluate(_progress);

            _skyboxProgress = _progress < 0.5f ? Mathf.Abs(1f - 2 * _progress - 0.5f) : -2f * Mathf.Abs(_progress - 0.75f) + 1f;
            RenderSettings.skybox.Lerp(_dayMaterial, _nightMaterial, _skyboxProgress);

            _dayPPVolume.weight = 1f - _skyboxProgress;
            _nightPPVolume.weight = _skyboxProgress;

            if (!_updateProbes) return;

            for (int i = 0; i < _dayProbes.Length; i++)
            {
                if(_dayProbes[i] != null) _dayProbes[i].intensity = 1f - _skyboxProgress;
            }

            for (int i = 0; i < _nightProbes.Length; i++)
            {
                if (_nightProbes[i] != null) _nightProbes[i].intensity = _skyboxProgress;
            }
        }
    }
}
