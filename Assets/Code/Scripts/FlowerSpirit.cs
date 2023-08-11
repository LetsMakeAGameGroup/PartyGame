using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;

public class FlowerSpirit : NetworkBehaviour {
    [Header("References")]
    [SerializeField] private Transform[] nodesTransforms;
    [SerializeField] private Transform currentNode;

    [Header("Settings")]
    [Tooltip("The initial speed this will move. When the speed is increased mid game, this variable will represent that.")]
    [SyncVar] public float speed = 1f;

    private void Awake() {
        GetComponent<Transform>().position = currentNode.position;
    }

    public IEnumerator MoveTowardsTrans() {
        GameObject[] currentNeighbors = currentNode.GetComponent<FlowerSpiritNode>().neighborNodes;
        Transform randomNeighborTrans = currentNeighbors[Random.Range(0, currentNeighbors.Count())].transform;

        Vector3 startingPos = transform.position;
        float elapsedPosition = 0f;

        while (Vector3.Distance(transform.position, randomNeighborTrans.position) > 0.001f) {
            transform.position = Vector3.MoveTowards(startingPos, randomNeighborTrans.position, elapsedPosition);
            elapsedPosition += speed * Time.deltaTime;
            yield return null;
        }

        transform.position = randomNeighborTrans.position;
        currentNode = randomNeighborTrans;

         StartCoroutine(MoveTowardsTrans());
    }
}
