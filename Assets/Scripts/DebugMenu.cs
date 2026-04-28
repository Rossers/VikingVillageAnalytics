using System;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    [Serializable]
    private struct TimescalePair
    {
        public KeyCode Key;
        public float Scale;
    }

    [SerializeField]
    private TimescalePair[] _timescaleHotkeys =
    {
        new TimescalePair{Key = KeyCode.Keypad0, Scale = 0.1f},
        new TimescalePair{Key = KeyCode.Keypad1, Scale = 1},
        new TimescalePair{Key = KeyCode.Keypad2, Scale = 2f},
        new TimescalePair{Key = KeyCode.Keypad3, Scale = 4f},
        new TimescalePair{Key = KeyCode.Keypad4, Scale = 8f}
    };

    private void Update()
    {
        for (int i = 0; i < _timescaleHotkeys.Length; i++)
        {
            if (Input.GetKeyDown(_timescaleHotkeys[i].Key))
            {
                Time.timeScale = _timescaleHotkeys[i].Scale;
                Debug.Log("Debug Menu: Time.timeScale set to " + _timescaleHotkeys[i].Scale);
            }
        }
    }
}