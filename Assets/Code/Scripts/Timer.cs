using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class Timer : NetworkBehaviour {
    public float duration = 1f;
    public bool detonateOnDestroy = false;
    public UnityEvent onTimerEnd = new();
    public UnityEvent<string> onDisplayTime = new();

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
        onDisplayTime?.Invoke(duration.ToString());

        yield return new WaitForSeconds(duration % 1);

        while (duration > 0f) {
            onDisplayTime?.Invoke(duration.ToString());

            duration--;
            yield return new WaitForSeconds(1f);
        }

        onTimerEnd?.Invoke();
    }
}
