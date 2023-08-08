using TMPro;
using UnityEngine;

public class PlayerScoreCardController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI pointsText;

    public void SetupScoreCard(string name, int points) {
        nameText.text = name;
        pointsText.text = '+' + points.ToString();
    }
}
