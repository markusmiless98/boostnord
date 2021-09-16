using System.Collections.Generic;
using UnityEngine;

public class VehicleMovementScript : MonoBehaviour
{
    VehicleInfoScript InfoOfVehicle;

    [SerializeField] List<Node> nodeslist = new List<Node>();

    [SerializeField] int CurrentNodeToMoveTo = 0;
    [SerializeField] int MaxCurrentNode = 0;

    float DirectionX = 0;
    float DirectionZ = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Checks for the Vehicle Information script, first in component in it, then in the scene
        if (GetComponentInChildren<VehicleInfoScript>())
        {
            ApplyVehicleInformationToMovement(GetComponent<VehicleInfoScript>());
        }
        else if (FindObjectOfType<VehicleInfoScript>())
        {
            ApplyVehicleInformationToMovement(FindObjectOfType<VehicleInfoScript>());
        }
        else
        {
            Debug.LogError("No Vehicle Information is Present in the scene");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && nodeslist != null && nodeslist.Count > 0)
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

    public void ApplyVehicleInformationToMovement(VehicleInfoScript theInformation)
    {
        InfoOfVehicle = theInformation;
    }

    // Moves vehicle between the different nodes
    private void DoVehicleMovement()
    {
        if (CurrentNodeToMoveTo <= MaxCurrentNode - 1)
        {
            var targetPos = nodeslist[CurrentNodeToMoveTo].transform.position;
            var MovementThisFrame = InfoOfVehicle.VehicleMovementSpeed * Time.deltaTime;
            if (targetPos != transform.position)
            {
                TurnVehicleAction(targetPos);
            }
            transform.position = Vector3.MoveTowards(transform.position, targetPos, MovementThisFrame);
            if (transform.position == targetPos)
            {
                CurrentNodeToMoveTo++;
            }
        }
        else
        {
            Debug.Log("Well done");
        }
    }

    private void TurnVehicleAction(Vector3 theLocation)
    {
        var vehicleLocation = gameObject.transform.position;
        if (vehicleLocation.x > theLocation.x * 1.25f)
        {
            DirectionX = 5;
        }
        else if (vehicleLocation.x < theLocation.x * 0.8)
        {
            DirectionX = -5;
        }
        else
        {
            DirectionX /= 2;
        }
        if (vehicleLocation.z > theLocation.z * 1.25f)
        {
            DirectionZ = -5;
        }
        else if (vehicleLocation.z < theLocation.z * 0.8)
        {
            DirectionZ = 5;
        }
        else
        {
            DirectionZ /= 2;
        }
        transform.rotation = Quaternion.Euler(DirectionX, 90, DirectionZ);
    }
}