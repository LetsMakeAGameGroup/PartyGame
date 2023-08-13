using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class NameValidator : MonoBehaviour {
    private TMP_InputField inputField;

    [Header("Settings")]
    [Tooltip("The maximum amount of characters permitted.")]
    [SerializeField] private float characterLimit = 16f;

    private void Awake() {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValidateInput += delegate (string input, int charIndex, char addedChar) { return ValidateChar(addedChar, charIndex); };
    }

    public char ValidateChar(char c, int index) {
        if ((char.IsLetter(c) || (c == ' ' && inputField.text[index - 1] != ' ' && (index == inputField.text.Length || inputField.text[index] != ' '))) && inputField.text.Length < characterLimit) {
            return c;
        } else {
            return '\0';
        }
    }
}