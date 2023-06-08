using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MazeManager : NetworkBehaviour {
    [SerializeField] private MinigameHandler minigameHandler = null;
    [SerializeField] private float mazePerSec = 15f;
    [SerializeField] private GameObject[] mazePresets = null;

    private int mazeIndex = 0;

    private void Start() {
        if (!isServer) return;

        // Initial randomized maze
        //RpcReplaceMazePreset(0, Random.Range(0, mazePresets.Length));
        RandomizeMaze();
    }

    public void StartMazeIntervals() {
        // Timers for each random maze generation in intervals since game has started.
        for (int i = 1; i < minigameHandler.minigameDuration / mazePerSec; i++) {
            Timer timer = gameObject.AddComponent(typeof(Timer)) as Timer;
            timer.duration = mazePerSec * i;
            timer.onTimerEnd.AddListener(RandomizeMaze);
        }
    }

    public void RandomizeMaze() {
        int lastIndex = mazeIndex;
        mazeIndex = Random.Range(0, mazePresets.Length - 1);
        if (mazeIndex == lastIndex) mazeIndex = (mazeIndex + 1) % mazePresets.Length;

        RpcReplaceMazePreset(lastIndex, mazeIndex);
    }

    [ClientRpc]
    public void RpcReplaceMazePreset(int oldIndex, int newIndex) {
        mazePresets[oldIndex].SetActive(false);
        mazePresets[newIndex].SetActive(true);
    }
}
