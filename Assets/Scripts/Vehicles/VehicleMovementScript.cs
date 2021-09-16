using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleMovementScript : MonoBehaviour
{
    VehicleInfoScript InfoOfVehicle;

    [SerializeField] Vector3 VehicleLocation;
    Vector3[] TargetLocation;
    Node[] nodes;
    [SerializeField] List<Node> nodeslist = new List<Node>();

    [SerializeField] int CurrentNodeToMoveTo = 0;
    [SerializeField] int MaxCurrentNode = 0;

    // Start is called before the first frame update
    void Start()
    {
        nodes = FindObjectOfType<PathDrawer>().transform.GetComponentsInChildren<Node>();
        InfoOfVehicle = FindObjectOfType<VehicleInfoScript>();
        foreach (var item in nodes)
        {
            MaxCurrentNode++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && nodeslist != null)
        {
            DoVehicleMovement();
        }
    }

    public void DoVehicleMovementUpdate(List<Node> nodeslistthing)
    {
        foreach (var item in nodeslistthing)
        {
            nodeslist.Add(item);
        }
        //nodeslist = nodeslistthing;
        MaxCurrentNode = nodeslistthing.Count;
        Debug.Log("SPACE");
    }

    private void DoVehicleMovement()
    {
        if (CurrentNodeToMoveTo <= MaxCurrentNode - 1)
        {
            var targetPos = nodeslist[CurrentNodeToMoveTo].transform.position;
            VehicleLocation = targetPos;
            var MovementThisFrame = InfoOfVehicle.VehicleMovementSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, MovementThisFrame);
            if (transform.position == targetPos)
            {
                CurrentNodeToMoveTo++;
            }
        }
        else
        {
            Debug.Log("W");
        }
    }
}
