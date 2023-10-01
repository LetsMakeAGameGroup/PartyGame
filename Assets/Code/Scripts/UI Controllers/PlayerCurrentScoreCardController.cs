using TMPro;
using UnityEngine;

public class PlayerCurrentScoreCardController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text scoreText;

    [HideInInspector] public int score = 0;

    [Header("Settings")]
    [Tooltip("The color of the text that belongs to the local player.")]
    [SerializeField] private Color selfCardTextColor;

    public void InitializeCard(string playerName) {
        playerNameText.text = playerName;
        scoreText.text = "0";
    }

    public void UpdateScore(int updatedScore) {
        score = updatedScore;
        scoreText.text = updatedScore.ToString();
        while (transform.GetSiblingIndex() > 0 && transform.parent.GetChild(transform.GetSiblingIndex() - 1).GetComponent<PlayerCurrentScoreCardController>().score < score) {
            transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);
        }
        while (transform.GetSiblingIndex() < transform.parent.childCount - 1 && transform.parent.GetChild(transform.GetSiblingIndex() + 1).GetComponent<PlayerCurrentScoreCardController>().score > score) {
            transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
        }
    }

    public void SetLocalTextColor() {
        playerNameText.color = selfCardTextColor;
        scoreText.color = selfCardTextColor;
    }
}
