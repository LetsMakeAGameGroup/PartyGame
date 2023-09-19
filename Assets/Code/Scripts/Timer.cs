using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class Timer : NetworkBehaviour {
    [Header("References")]
    public UnityEvent onTimerEnd = new();
    public UnityEvent<string> onDisplayTime = new();

    [Header("Settings")]
    [Tooltip("How many seconds this timer will last.")]
    public float duration = 1f;
    [Tooltip("If this timer should invoke OnTimerEnd when this timer is destroyed prior to the time hitting zero.")]
    public bool detonateOnDestroy = false;

    public void Initialize(float _duration, bool _detonateOnDestroy, UnityEvent _onTimerEnd, UnityEvent<string> _onDisplayTime) {
        _duration = duration;
        _detonateOnDestroy = detonateOnDestroy;
        _onTimerEnd = onTimerEnd;
        _onDisplayTime = onDisplayTime;
    }

    private void Start() => StartCoroutine(StartTimer());

    private void OnDestroy() {
        if (detonateOnDestroy) onTimerEnd?.Invoke();
    }

    public void DetonateEarly() {
        onTimerEnd?.Invoke();
        StopAllCoroutines();
    }

    private IEnumerator StartTimer() {
        onDisplayTime?.Invoke(Mathf.CeilToInt(duration).ToString());

        float interval = duration % 1;
        float nextEventTime = Time.time + interval;
        while (Time.time < nextEventTime) {
            yield return null;
        }
        duration -= duration % 1;
        onDisplayTime?.Invoke(duration.ToString());

        interval = 1;
        nextEventTime = Time.time + interval;
        while (duration > 0f) {
            if (Time.time >= nextEventTime) {
                nextEventTime += interval;
                duration--;

                if (duration > 0) onDisplayTime?.Invoke(duration.ToString());
            }
            yield return null;
        }

        onTimerEnd?.Invoke();
    }
}
