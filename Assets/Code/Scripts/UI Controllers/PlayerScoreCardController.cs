using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerScoreCardController : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI pointsText;

    public void SetupScoreCard(string name, int points) {
        nameText.text = name;
        pointsText.text = '+' + points.ToString();
    }
}
