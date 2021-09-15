using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

    public enum NodeType {
        Road, Bike, Terminal
    }

    public Dictionary<NodeType, Color> nodeColors = new Dictionary<NodeType, Color>() {
        { NodeType.Road, Color.red},
        { NodeType.Bike, Color.blue },
        { NodeType.Terminal, Color.yellow}};

    public Node[] connectedNodes;

    public NodeType nodeType;

    void Start() {

    }

    void Update() {

    }

    void OnDrawGizmos() {
        // Draw a yellow sphere at the transform's position

        Gizmos.color = nodeColors[nodeType];
        Gizmos.DrawSphere(transform.position, .5f);

        Gizmos.color = Color.yellow;
        foreach (Node node in connectedNodes) {
            Gizmos.DrawLine(transform.position, node.transform.position);
        }
    }
}
