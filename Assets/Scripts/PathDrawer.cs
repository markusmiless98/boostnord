using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathDrawer : MonoBehaviour {

    public VehicleManager vehicleManager;
    public BoxCollider target;
    public LineRenderer line;
    public Gradient lineGradient;
    public PackageManager pm;

    public GameObject drawingUI;
    public Text capacityDisplay;

    public float packagePickupRange;
    public float nodeTouchRange; // Amount of range that there can be between a mouse and a node to draw line
    public bool drawingLine = false;

    public AudioClip[] drawingSounds;
    public AudioSource audio;

    public int maxCapacity = 20;
    public int capacity = 0;
    int leftOver = 0;
    public bool canDraw = false;


    public enum DrawMode {
        Car, Bicycle
    }

    public DrawMode mode;

    Node[] nodes;
    List<Node> path = new List<Node>();
    List<Package> packagesForPickup = new List<Package>();

    public Image terminalSelect;

    public Color32 terminalHighlight, terminalReturn;


    void Start() {
        nodes = transform.GetComponentsInChildren<Node>();

        foreach (Node node in nodes) {
            foreach (Node connectedNode in node.connectedNodes) {
                if (Array.IndexOf(connectedNode.connectedNodes, node) == -1) {
                    Array.Resize(ref connectedNode.connectedNodes, connectedNode.connectedNodes.Length + 1);
                    connectedNode.connectedNodes[connectedNode.connectedNodes.Length - 1] = node;
                }
            }
        }

        pm.Init(nodes);
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

    // Gets a list of all packes within the packagePickupRange of a node.
    List<Package> PackagesInRangeOfNodes(Node node1, Node node2) {
        List<Package> results = new List<Package>();
        foreach (Package package in pm.packages)
            if (Vector3.Distance(package.transform.position, NearestPointOnLine(node1.transform.position, node2.transform.position, package.transform.position)) < packagePickupRange)
                results.Add(package);
        return results;
    }

    public int CapacityLeft() {
        int left = maxCapacity - capacity;
        if (left < 0) left = 0;
        return left;
    }

    public void PlayDrawingSound() {
        audio.clip = drawingSounds[UnityEngine.Random.Range(0, drawingSounds.Length)];
        audio.time = 0;
        audio.Play();
    }

    void Update() {

        terminalSelect.enabled = drawingLine || canDraw;
        terminalSelect.color = packagesForPickup.Count > 0 ? terminalReturn : terminalHighlight;

        drawingUI.SetActive(path.Count > 1);
        capacityDisplay.text = capacity + "/" + maxCapacity;

        if (!canDraw && drawingLine) ClearPath();

        if (Input.GetMouseButton(0) && canDraw) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Raycast the big target to get the closest node to mouse
            if (target.Raycast(ray, out hit, 100f)) {
                Node targetNode = GetClosestNode(hit.point);

                if (Vector3.Distance(targetNode.transform.position, hit.point) <= nodeTouchRange ||
                    drawingLine) {

                    drawingLine = true;

                    // Add the start of the line when starting from the terminal
                    if (path.Count == 0 && targetNode.nodeType == Node.NodeType.Terminal) {
                        path.Add(targetNode);
                        PlayDrawingSound();
                    }

                    // Check that the target node and previous node has connection
                    if (path.Count > 0 && HasConnection(targetNode, path[path.Count - 1])) {
                        if (!(targetNode.nodeType == Node.NodeType.Bike && mode != DrawMode.Bicycle)) {
                            // Check that the node is not the previous or the one before that
                            if ((path.Count == 1 || path[path.Count - 2] != targetNode)) {
                                if (path[path.Count - 1] != targetNode) {
                                    path.Add(targetNode);
                                    PlayDrawingSound();
                                }
                            }
                        }

                    }

                    // Remove the last point if you hover over the next to last point.
                    // It's the way to go back on the line
                    if (path.Count >= 2 && targetNode == path[path.Count - 2]) {
                        path.RemoveAt(path.Count - 1);
                        PlayDrawingSound();
                    }
                }
            }
        } else {
            drawingLine = false;
            if (IsPathComplete()) {
                if (packagesForPickup.Count > 0) {


                    List<PackagePoint> packagePath = new List<PackagePoint>();

                    for (int i = 0; i < path.Count; i++) {
                        Node node = path[i];


                        packagePath.Add(new PackagePoint(node.transform.position, 0, 0));

                        while (packagesForPickup.Count > 0 && packagesForPickup[0].nodeIndex == i) {
                            Package package = packagesForPickup[0];

                            if (package.nodeIndex == i && i < path.Count + 1) {
                                Vector3 pickupPoint = NearestPointOnLine(node.transform.position, path[i + 1].transform.position, package.transform.position);

                                int packagesToDrop = 0;
                                if (packagesForPickup.Count == 1 && leftOver > 0) {

                                    packagesToDrop = package.packagesToDeliver - leftOver;
                                    package.packagesToDeliver = leftOver;
                                } else {

                                    packagesToDrop = package.packagesToDeliver;
                                    package.packagesToDeliver = 0;

                                }

                                packagePath.Add(new PackagePoint(pickupPoint, packagesToDrop, package.timeCreated));
                                packagesForPickup.RemoveAt(0);
                                if (leftOver <= 0) pm.packages.Remove(package);
                            }


                        }
                    }

                    CollectOrder order = new CollectOrder();
                    order.path = packagePath;
                    order.vehicle = VehicleManager.VehicleType.GasCar;

                    vehicleManager.DeployVehicle(order);
                    // Send the order...
                }

                ClearPath();
            }
        }

        packagesForPickup.Clear();
        capacity = 0;

        for (int i = 0; i < path.Count - 1; i++) {
            foreach (Package package in PackagesInRangeOfNodes(path[i], path[i + 1])) {
                if (packagesForPickup.IndexOf(package) == -1 &&
                    package.state != Package.PackageState.awaitingPickup
                    && capacity < maxCapacity) {
                    packagesForPickup.Add(package);

                    // This determens after which node path this package should be picked up
                    package.nodeIndex = i;

                    leftOver = -(CapacityLeft() - package.packagesToDeliver);
                    capacity += package.packagesToDeliver;
                }
            }
        }

        foreach (Package package in pm.packages) {
            if (package.state == Package.PackageState.notice || package.state == Package.PackageState.readyForSelection) {
                package.SetState(packagesForPickup.IndexOf(package) == -1 ? Package.PackageState.notice : Package.PackageState.readyForSelection);
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

        line.colorGradient = lineGradient;

    }

    public int GetAmountOfPackagesToPickup() {
        int amount = 0;
        foreach (Package package in pm.packages) {
            if (package.state != Package.PackageState.awaitingPickup) amount++;
        }
        return amount;
    }


    public static Vector3 NearestPointOnLine(Vector3 start, Vector3 end, Vector3 pnt) {
        Vector3 line = (end - start);
        float len = line.magnitude;
        line.Normalize();

        Vector3 v = pnt - start;
        float d = Vector3.Dot(v, line);
        d = Mathf.Clamp(d, 0f, len);
        return start + line * d;
    }

    public void ClearPath() {
        path.Clear();
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
