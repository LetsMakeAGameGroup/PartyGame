using UnityEngine;

public class BerryBlaster : RangedWeapon {
    [Header("Berry Blaster References")]
    [SerializeField] private Renderer ammoRenderer;

    private void Start() {
        ammoRenderer.material.color = PlayerColorOptions.options[playerController.playerColor];
    }
}
