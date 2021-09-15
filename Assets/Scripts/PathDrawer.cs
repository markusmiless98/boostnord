using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathDrawer : MonoBehaviour {

    public BoxCollider target;
    public LineRenderer line;
    public Text infoText;

    Node[] nodes;
    List<Node> path = new List<Node>();

    void Start() {
        nodes = transform.GetComponentsInChildren<Node>();
    }

    // Use this to check if there is a connection between two nodes
    // Checks if there is a connection between two nodes (Both ways)
    bool HasConnection(Node node1, Node node2) {
        return HasOriginatingConnection(node1, node2) || HasOriginatingConnection(node2, node1);
    }

    // Don't use this to check both way direction between nodes. This is only used by HasConnection()
    // Checks if one node is connected to the other (One way)
    bool HasOriginatingConnection(Node node1, Node node2) {
        return Array.IndexOf(node1.connectedNodes, node2) != -1;
    }

    Node GetClosestNode(Vector3 position) {
        Node closestNode = null;
        float closestDistance = -1;
        foreach (Node node in nodes) {
            float distance = Vector3.Distance(node.transform.position, position);
            if (closestDistance == -1 || closestDistance > distance) {
                closestNode = node;
                closestDistance = distance;
            }
        }
        return closestNode;
    }



    void Update() {
        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Raycast the big target to get the closest node to mouse
            if (target.Raycast(ray, out hit, 100f)) {
                Node targetNode = GetClosestNode(hit.point);

                // Add the start of the line when starting from the terminal
                if (path.Count == 0 && targetNode.nodeType == Node.NodeType.Terminal) {
                    path.Add(targetNode);
                }

                // Check that the target node and previous node has connection
                if (path.Count > 0 && HasConnection(targetNode, path[path.Count - 1])) {
                    // Check that the node is not the previous or the one before that
                    if ((path.Count == 1 || path[path.Count - 2] != targetNode)) {
                        if (path[path.Count - 1] != targetNode) {
                            path.Add(targetNode);
                        }
                    }
                }

                // Remove the last point if you hover over the next to last point.
                // It's the way to go back on the line
                if (path.Count >= 2 && targetNode == path[path.Count - 2]) {
                    path.RemoveAt(path.Count - 1);
                }

            }
        }

        // Set amount of path points for the line
        line.positionCount = path.Count;

        // Set the position for each path point for the line

        float tripDistance = 0;

        for (int i = 0; i < path.Count; i++) {
            Node node = path[i];

            if (i > 0) tripDistance += Vector3.Distance(node.transform.position, path[i - 1].transform.position);

            line.SetPosition(i, node.transform.position);
        }

        // Gradient for the line
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.red, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        line.colorGradient = gradient;

        // Display some info text
        infoText.text = "Distance: " + Math.Round(tripDistance) + "\n" +
              "Nodes: " + path.Count + "\nComplete: " + IsPathComplete() + "\nUses bike path: " + UsesBikePath();

    }

    bool UsesBikePath() {
        foreach (Node node in path) {
            if (node.nodeType == Node.NodeType.Bike) return true;
        }
        return false;
    }

    bool IsPathComplete() {
        return path.Count > 2 && path[path.Count - 1].nodeType == Node.NodeType.Terminal;
    }
}
