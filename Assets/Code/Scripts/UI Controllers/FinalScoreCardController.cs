using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalScoreCardController : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    public void SetupScoreCard(string name, int finalScore) {
        nameText.text = name;
        finalScoreText.text = finalScore.ToString();
    }
}
