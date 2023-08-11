using TMPro;
using UnityEngine;

public class FinalScoreCardController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    public void SetupScoreCard(string name, int finalScore) {
        nameText.text = name;
        finalScoreText.text = finalScore.ToString();
    }
}
