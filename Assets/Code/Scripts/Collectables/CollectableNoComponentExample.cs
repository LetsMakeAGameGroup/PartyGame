using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableNoComponentExample : MonoBehaviour, ICollectable
{
    public void OnCollect(ICollector collector)
    {
        Debug.Log("Boom!");

        PlayerController playerController = collector.GetCollectorGameObject.GetComponent<PlayerController>();

        if (playerController != null) 
        {
            Debug.Log("Launch Character");
            Vector3 launchDir = playerController.transform.position - transform.position;
            playerController.MovementComponent.LaunchCharacter(launchDir * 100);
        }

    }

    public void OnTriggerEnter(Collider other)
    {
        ICollector collector = other.GetComponent<ICollector>();

        if (collector != null) 
        {
            if (collector.CanCollect())
            {
                OnCollect(collector);
                collector.OnCollectableCollect(this);
            }
        }
    }
}
