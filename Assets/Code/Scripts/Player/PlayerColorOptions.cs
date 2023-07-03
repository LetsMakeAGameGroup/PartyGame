using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorOptions : MonoBehaviour {
    public static Dictionary<string, Color> options = new() {
        { "Red", Color.red },
        { "Blue", Color.blue },
        { "Green", Color.green },
        { "Pink", new Color(1f, 0.6f, 1f) },
        { "Orange", new Color(1f, 0.6f, 0f) },
        { "Yellow", Color.yellow },
        { "Purple", new Color(0.6f, 0f, 0.6f) },
        { "Cyan", Color.cyan }
    };
}
