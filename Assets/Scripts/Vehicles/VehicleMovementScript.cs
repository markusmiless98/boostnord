using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleMovementScript : MonoBehaviour {
    /*VehicleInfoScript InfoOfVehicle;*/
    public CollectOrder order;
    public VehicleManager vm;

    public AudioSource idle, effect;

    public AudioClip[] packageDropOfSounds;

    int pathIndex = 0;
    int packagesToDropOff = 0;

    public VehicleManager.Vehicle vehicle;
    public Transform packageSpawner;
    public GameObject packagePrefab;
    public Transform model;

    public GameObject[] packageModels;

    public PackagePoint GetPoint() {
        return order.path[pathIndex];
    }

    public void SetModel(GameObject newModel) {
        while (model.childCount > 0) DestroyImmediate(model.GetChild(0).gameObject);
        Instantiate(newModel, model);
    }

    void Start() {
        effect.clip = vehicle.template.startupSound;
        effect.time = 0;
        effect.volume = 1f;

        effect.Play();
    }

    private void OnDrawGizmos() {
        if (order != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(GetPoint().position, .7f);
        }

        for (int i = pathIndex + 1; i < order.path.Count; i++) {
            Gizmos.color = Color.gray;

            if (order.path[i].packagesToLeave > 0) Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(order.path[i].position, order.path[i].packagesToLeave > 0 ? 1f : .7f);
        }
    }

    private IEnumerator LeavePackage(float creationTime) {
        yield return new WaitForSeconds(0.5f);
        packagesToDropOff--;

        effect.clip = packageDropOfSounds[Random.Range(0, packageDropOfSounds.Length)];

        effect.volume = .5f;

        effect.time = 0;
        effect.Play();

        float rating = Time.time - creationTime;
        if (rating > 30) rating = 30;
        vm.SubmitRating(rating);

        vm.SpawnPackage(packageSpawner.position);

        if (packagesToDropOff > 0) yield return LeavePackage(creationTime);
    }

    private void DoVehicleMovement() {

        if (pathIndex == 0) {
            transform.position = GetPoint().position;
            pathIndex++;
        }

        Vector3 targetPos = GetPoint().position;

        float MovementThisFrame = vehicle.template.speed * Time.deltaTime;
        transform.LookAt(targetPos);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, MovementThisFrame);

        if (transform.position == targetPos) {
            float distanceTraveled = Vector3.Distance(targetPos, order.path[pathIndex - 1].position);
            vm.RecordEmissions(distanceTraveled * vehicle.template.emissions);

            vehicle.odometer += distanceTraveled;

            if (GetPoint().packagesToLeave > 0) {
                packagesToDropOff = GetPoint().packagesToLeave;
                StartCoroutine(LeavePackage(GetPoint().creationTime));
            }
            pathIndex++;
        }

        if (pathIndex == order.path.Count) {
            vm.ReturnVehicle(vehicle);
            DestroyImmediate(gameObject);
        }

    }


    void Update() {
        if (packagesToDropOff <= 0) DoVehicleMovement();
    }

}