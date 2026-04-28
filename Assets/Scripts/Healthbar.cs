using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Healthbar : PooledObject
    {
        [SerializeField] private Image _healthbar;
        [SerializeField] private Vector2 _offset = new Vector2(0f, 20f);

        private Unit _target;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void SetTarget(Unit target)
        {
            _target = target;
            _target.DamageTakenEvent += OnDamageTaken;
            _target.KilledEvent += OnKilled;

            _healthbar.fillAmount = _target.HealthFill;
        }

        private void OnDamageTaken(float amount)
        {
            _healthbar.fillAmount = _target.HealthFill;
        }

        private void OnKilled(Unit obj)
        {
            ReturnToPool();
        }

        public override void PrepareForPool()
        {
            base.PrepareForPool();

            if (_target == null) return;
            _target.DamageTakenEvent -= OnDamageTaken;
            _target.KilledEvent -= OnKilled;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector2 overheadPosition = (Vector2) Camera.main.WorldToScreenPoint(_target.OverheadPosition) + _offset;
            if (!_target.IsVisible) overheadPosition = Vector2.negativeInfinity;
            _rectTransform.anchoredPosition = overheadPosition;
        }
    }
}
