using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameHandler : MonoBehaviour {
    public List<GameObject> winners = new();

    private bool isRunning = false;

    private void Start() {
        StartCoroutine(CountdownTillStart());
    }

    IEnumerator CountdownTillStart() {
        yield return new WaitForSeconds(3);
        isRunning = true;
    }

    // Adds player the winners list accordingly.
    public void AddWinner(GameObject player) {
        if (!isRunning) return;

        foreach(GameObject winner in winners) {
            if (winner == player) return;
        }

        winners.Add(player);

        if (winners.Count == 3 || winners.Count == CustomNetworkManager.Instance.players.Count) {
            EndMinigame();
        }
    }

    // Called when ready to end the minigame and start the next round.
    public void EndMinigame() {
        if (!isRunning) return;

        // Assign points to winners accordingly
        int assignPoints = 3;
        foreach(GameObject winner in winners) {
            winner.GetComponent<PlayerController>().points += assignPoints;
            assignPoints--;
        }

        Debug.Log($"Winner: {winners[0].GetComponent<PlayerController>().playerName} with now {winners[0].GetComponent<PlayerController>().points} points.");  // Change this to show winners on screen

        GameManager.Instance.StartNextRound();
    }
}
