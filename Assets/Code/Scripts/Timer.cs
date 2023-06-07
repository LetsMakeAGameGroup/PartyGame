using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class Timer : NetworkBehaviour {
    [SerializeField] private float duration = 1f;
    [SerializeField] private bool detonateOnDestroy = false;
    [SerializeField] private UnityEvent onTimerEnd = new();
    [SerializeField] private UnityEvent<string> onDisplayTime = new();

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
