using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackagePoint {
    public Vector3 position;
    public int packagesToLeave;
    public float creationTime;



    public PackagePoint(Vector3 position, int packages, float creationTime) {
        packagesToLeave = packages;
        this.creationTime = creationTime;
        this.position = position;
    }
}

public class CollectOrder {
    public VehicleManager.VehicleType vehicle;
    public List<PackagePoint> path;
}

public class PackageManager : MonoBehaviour {
    public PathDrawer path;
    public GameObject packagePrefab;

    public bool running = false;
    Node[] nodes;
    public List<Package> packages = new List<Package>();

    public Transform deliveryStations;


    public void Stop() {
        running = false;
    }
    public void Init(Node[] nodes) {
        this.nodes = nodes;
        running = true;
        StartCoroutine(SpawnPackages());
    }

    public int GetAmountToDeliver() {
        int amount = 0;
        foreach (Package package in packages) {
            amount += package.packagesToDeliver;
        }
        return amount;
    }

    IEnumerator SpawnPackages() {
        while (running) {
            // Create a home delivery request
            CreateNewPackageDropOff(1, 2);
            if (Random.value > .9f) CreateDeliveryStationPackage();
            yield return new WaitForSeconds(Random.Range(5, 15));
        }
    }

    public bool IsNodeEligible(Node node) {
        if (node.nodeType == Node.NodeType.RoadNoPackage || node.nodeType == Node.NodeType.Terminal) return false;
        return true;
    }

    public void CreateNewPackageDropOff(int from, int to) {
        // Get a random node
        // Get a random connection from that node
        // Get a random point between the two points
        // Spawn a package on the side of the point

        List<Node> eligibleNodes = new List<Node>();

        foreach (Node node in nodes) {
            bool eligible = IsNodeEligible(node);
            foreach (Node connected in node.connectedNodes) {
                if (!IsNodeEligible(connected)) eligible = false;
            }
            if (eligible) {
                eligibleNodes.Add(node);
            }
        }

        Node startingNode = eligibleNodes[Random.Range(0, eligibleNodes.Count - 1)];
        Node endingNode = startingNode.connectedNodes[Random.Range(0, startingNode.connectedNodes.Length - 1)];
        Vector3 spawnPosition = Vector3.Lerp(startingNode.transform.position, endingNode.transform.position, Random.RandomRange(0f, 1f));
        Vector3 direction = (startingNode.transform.position - endingNode.transform.position).normalized;

        direction = Quaternion.Euler(0, 90, 0) * direction;
        if (Random.value > .5f) direction = Quaternion.Euler(0, 180, 0) * direction;

        float distanceFromSidewalk = 0.5f;

        spawnPosition += direction * distanceFromSidewalk;

        CreatePackage(Random.Range(from, to), spawnPosition);
    }

    public void CreatePackage(int amount, Vector3 position) {
        Package package = Instantiate(packagePrefab, transform).GetComponent<Package>();
        package.transform.position = position;
        package.packagesToDeliver = amount;
        packages.Add(package);
    }

    public void CreateDeliveryStationPackage() {
        CreatePackage(Random.Range(5, 20), deliveryStations.GetChild(Random.Range(0, deliveryStations.childCount)).position);
    }

    // Update is called once per frame
    void Update() {
        /*if (packages.Count < 5) CreateNewPackageDropOff();*/
    }
}
