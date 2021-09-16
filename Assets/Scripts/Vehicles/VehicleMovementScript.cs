using System.Collections.Generic;
using UnityEngine;

public class VehicleMovementScript : MonoBehaviour
{
    VehicleInfoScript InfoOfVehicle;

    [SerializeField] List<Node> nodeslist = new List<Node>();

    [SerializeField] int CurrentNodeToMoveTo = 0;
    [SerializeField] int MaxCurrentNode = 0;

    Quaternion OldRotation, NewRotation;
    float timeCount = 0;
    Vector3 CurrentTransformTarget;
    float RecentlySwappedRotation = 0;

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
            transform.LookAt(targetPos);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, MovementThisFrame);
            if (transform.position == targetPos && TargetIsPackage(CurrentNodeToMoveTo))
            {
                CurrentNodeToMoveTo++;
                RecentlySwappedRotation = 0;
            }
        }
        else
        {
            Debug.Log("Well done");
            nodeslist.Clear();
            CurrentNodeToMoveTo = 0;
        }
    }

    public bool TargetIsPackage(int theGameObjectThatIsHopefullyPackageInt)
    {
        return theGameObjectThatIsHopefullyPackageInt >= 0;
    }
}