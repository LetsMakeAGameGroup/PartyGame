using System.Collections.Generic;
using UnityEngine;

public class SpawnHolder : MonoBehaviour {
    [Header("References")]
    public GameObject playerPrefab;

    [HideInInspector] public List<GameObject> currentSpawns = new();

    private void Awake() {
        for (int i = 0; i < transform.childCount; i++) {
            currentSpawns.Add(transform.GetChild(i).gameObject);
        }
    }
}
