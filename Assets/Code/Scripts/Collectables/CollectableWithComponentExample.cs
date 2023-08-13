using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableWithComponentExample : MonoBehaviour {
    CollectableComponent collectableComponent;

    private void Awake() {
        collectableComponent = GetComponent<CollectableComponent>();

        if (collectableComponent != null) {
            collectableComponent.onCollectDelegate += OnCollect;
        }
    }

    public void OnCollect() {
        Debug.Log("Collected a coin!");
        //Logic here

        gameObject.SetActive(false);
    }
}
