using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleManager : MonoBehaviour {

    public GameObject truck;

    void Start() {

    }

    public void SpawnVehicle(CollectOrder order) {
        /*VehicleMovementScript*/
        GameObject vehicle = Instantiate(truck, transform).gameObject;
    }

    // Update is called once per frame
    void Update() {

    }
}
