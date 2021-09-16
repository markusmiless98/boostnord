using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleMovementScript : MonoBehaviour {
    /*VehicleInfoScript InfoOfVehicle;*/
    public CollectOrder order;
    int pathIndex = 0;

    // Start is called before the first frame update
    void Start() {

        // Checks for the Vehicle Information script, first in component in it, then in the scene
        /*if (GetComponentInChildren<VehicleInfoScript>()) {
            ApplyVehicleInformationToMovement(GetComponent<VehicleInfoScript>());
        } else if (FindObjectOfType<VehicleInfoScript>()) {
            ApplyVehicleInformationToMovement(FindObjectOfType<VehicleInfoScript>());
        } else {
            Debug.LogError("No Vehicle Information is Present in the scene");
        }*/
    }

    // Moves vehicle between the different nodes

    public PackagePoint GetPoint() {
        return order.path[pathIndex];
    }

    private void DoVehicleMovement() {

        if (pathIndex == 0) {
            transform.position = GetPoint().position;
            pathIndex++;
        }

        Vector3 targetPos = GetPoint().position;

        // TODO IMPLEMENT MOVEMENT SPEED HERE
        float MovementThisFrame = 1f * Time.deltaTime;
        transform.LookAt(targetPos);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, MovementThisFrame);

        if (transform.position == targetPos) {
            pathIndex++;
        }

        /*if (CurrentNodeToMoveTo <= MaxCurrentNode - 1) {
            var targetPos = nodeslist[CurrentNodeToMoveTo].transform.position;
            var MovementThisFrame = InfoOfVehicle.VehicleMovementSpeed * Time.deltaTime;
            transform.LookAt(targetPos);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, MovementThisFrame);
            if (transform.position == targetPos) {
                if (!TargetIsPackage()) {
                    CurrentNodeToMoveTo++;
                }
            }
        } else {
            Debug.Log("Well done");
            nodeslist.Clear();
            CurrentNodeToMoveTo = 0;
        }*/
    }

    // Update is called once per frame
    void Update() {
        /* if (Input.GetKey(KeyCode.Space) && nodeslist != null && nodeslist.Count > 0) {
             DoVehicleMovement();
         }*/

        DoVehicleMovement();
    }

    public void DoVehicleMovementUpdate(List<Node> nodeslistthing) {
        /*foreach (var item in nodeslistthing) {
            nodeslist.Add(item);
        }
        //nodeslist = nodeslistthing;
        MaxCurrentNode = nodeslistthing.Count;*/
    }

    /*public void ApplyVehicleInformationToMovement(VehicleInfoScript theInformation) {
        InfoOfVehicle = theInformation;
    }*/



    public void AddPackageAsTarget(Package ThePackage) {
        /*packageList.Add(ThePackage);*/
    }

    /*public bool TargetIsPackage() {
        foreach (var packages in packageList) {
            if (Mathf.Abs(nodeslist[CurrentNodeToMoveTo].transform.position.x - packages.transform.position.x) < 3f && Mathf.Abs(nodeslist[CurrentNodeToMoveTo].transform.position.z - packages.transform.position.z) < 3f) {
                if (packages.HasBeenPickedUp == false) {
                    StartCoroutine(PickUpObjectAfterDelay(packages));
                    return true;
                }
            }
        }
        return false;
    }*/

    /*private IEnumerator PickUpObjectAfterDelay(Package thePackage) {
        yield return new WaitForSeconds(0.5f);
        if (thePackage.HasBeenPickedUp == false) {
            GameObject EnemyClone = Instantiate(BoxesToSpawn, new Vector3(thePackage.transform.position.x - 1f, thePackage.transform.position.y + 2f, thePackage.transform.position.z), transform.rotation);
        }
        thePackage.HasBeenPickedUp = true;
    }*/
}