using Mirror;
using UnityEngine;

public class BerryBullet : Bullet {
    private GameObject shooterPlayer = null;

    [SerializeField] private float knockbackForce = 25f;
    [SerializeField] private float verticalForce = 5f;
    [SerializeField] private float stunTime = 0.5f;

    [SyncVar(hook = nameof(SetColor))] public string bulletColor = "";

    public void SetShooter(GameObject player) {
        shooterPlayer = player;
    }

    public void SetColor(string oldColor, string newColor) {
        GetComponent<Renderer>().material.color = PlayerColorOptions.options[newColor];
    }

    public override void OnHit(GameObject hitObject) {
        base.OnHit(hitObject);
        
        if (hitObject.TryGetComponent(out CaptureTarget captureTarget)) {
            captureTarget.SetOwner(shooterPlayer);
        }

        if (hitObject.TryGetComponent(out PlayerMovementComponent playerMovementComponent)) {
            playerMovementComponent.TargetKnockbackCharacter(transform.TransformDirection(new Vector3(0, verticalForce, knockbackForce)));
            StartCoroutine(playerMovementComponent.StunPlayer(stunTime));
        }
    }
}
