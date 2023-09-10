using TMPro;
using UnityEngine;

public class PlayerScoreCardController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private TextMeshProUGUI standingText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI pointsText;

    private string[] standings = { "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th" };

    public void SetupScoreCard(int standing, string name, int points) {
        standingText.text = standings[standing];
        nameText.text = name;
        pointsText.text = '+' + points.ToString() + " Point" + (points == 1 ? " " : "s");
    }
}
