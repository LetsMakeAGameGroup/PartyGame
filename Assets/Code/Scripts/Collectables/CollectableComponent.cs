using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnCollectableDelegate();

//Anything that collects HAS to be ICollector, if not, it should NOT be able to collect. Follow this rule no mater what :). 
public interface ICollector
{
    bool CanCollect();
    void OnCollectableCollect(ICollectable collected);
    GameObject GetCollectorGameObject { get; }
}

//If the game object doesnt use the CollectableComponent (which dont think would be a case at all), you can use ICollectable to make your own.
public interface ICollectable 
{
    public void OnTriggerEnter(Collider other);
    public void OnCollect(ICollector collector);
}

public class CollectableComponent : MonoBehaviour, ICollectable
{
    bool hasBeenCollected;
    public OnCollectableDelegate onCollectDelegate;

    public void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected)
        {
            return;
        }

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

    public void OnCollect(ICollector collector)
    {
        if (onCollectDelegate != null)
        {
            onCollectDelegate();
        }

        hasBeenCollected = true;
        gameObject.SetActive(false);

    }

}
