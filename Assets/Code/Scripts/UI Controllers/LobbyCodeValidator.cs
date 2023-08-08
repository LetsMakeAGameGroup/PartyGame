using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class LobbyCodeValidator : MonoBehaviour {
    private TMP_InputField inputField;

    [Header("Settings")]
    [Tooltip("The maximum amount of characters permitted.")]
    [SerializeField] private int characterLimit = 6;

    private void Awake() {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValidateInput += delegate (string input, int charIndex, char addedChar) { return ValidateChar(addedChar); };
    }

    public char ValidateChar(char c) {
        if (char.IsLetterOrDigit(c) && inputField.text.Length < characterLimit) {
            return char.ToUpper(c);
        } else {
            return '\0';
        }
    }
}