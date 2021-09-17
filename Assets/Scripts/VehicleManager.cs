using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleManager : MonoBehaviour {

    public GameObject car;
    public GameObject packagePrefab;
    public Transform packages;

    public Transform deployButtonsParent;
    public GameObject deployButtonPrefab;
    public List<DeployButton> deployButtons = new List<DeployButton>();
    public PathDrawer path;
    public PackageManager pm;

    public Text timeDisplay, packagesCollectedDisplay, activePackagesDisplay;

    public Slider ratingSlider, coSlider;

    DeployButton selectedButton = null;

    public int packagesDelivered = 0;
    public float emissions = 0;
    public float ratings = 0;

    public float timeStarted;

    public AudioClip click;
    public AudioSource audio;

    public enum VehicleType {
        GasCar, GasTruck, EvCar, EvTruck, Bicycle
    }

    [Serializable]
    public struct VehicleTemplate {
        public bool ev;

        public AudioClip startupSound;
        public VehicleType type;
        public Sprite icon;
        public float speed;
        public int capacity;
        public string name;
        public float emissions;
        public float chargeTime; // Seconds
        public GameObject model;
    }

    public class Vehicle {
        public bool Avalible() {
            return !driving && charge == 100;
        }
        public Vehicle(VehicleTemplate template) {
            this.template = template;
            this.driving = false;
            this.charge = 100;
        }
        public float odometer = 0;
        public VehicleTemplate template;
        public bool driving;
        public float charge; // 0-100
    }

    public VehicleTemplate[] vehicleTemplates;
    public List<Vehicle> vehicles = new List<Vehicle>();
    public GameObject[] packageModels;

    void Start() {
        Setup();
    }



    public void Setup() {
        CreateVehicle(VehicleType.GasCar);
        CreateVehicle(VehicleType.GasCar);
        CreateVehicle(VehicleType.GasCar);
        CreateVehicle(VehicleType.GasTruck);
        CreateVehicle(VehicleType.GasTruck);
        CreateVehicle(VehicleType.EvCar);

        CreateVehicle(VehicleType.Bicycle);
        CreateVehicle(VehicleType.Bicycle);
        CreateVehicle(VehicleType.Bicycle);
        CreateVehicle(VehicleType.Bicycle);
        CreateVehicle(VehicleType.Bicycle);
        CreateVehicle(VehicleType.Bicycle);
        CreateVehicle(VehicleType.Bicycle);
        CreateVehicle(VehicleType.Bicycle);

        CreateVehicle(VehicleType.EvTruck);

        timeStarted = Time.time;

        CreateDeployButtons();
    }

    void CreateDeployButtons() {
        while (deployButtonsParent.childCount > 0) DestroyImmediate(deployButtonsParent.GetChild(0).gameObject);

        foreach (VehicleTemplate template in vehicleTemplates) {
            int amount = GetAmountOfCarsOfType(template.type);
            if (amount > 0) {
                // Create the button
                DeployButton button = Instantiate(deployButtonPrefab, deployButtonsParent).GetComponent<DeployButton>();
                button.icon.sprite = template.icon;
                if (!template.ev) {
                    button.charge.gameObject.SetActive(false);
                    button.chargeIcon.gameObject.SetActive(false);
                } else {
                    button.charge.value = 100f;
                }

                button.selected.enabled = false;
                button.amount.text = amount + "/" + amount;
                deployButtons.Add(button);


                button.type = template.type;

                button.button.onClick.AddListener(() => {
                    SelectButton(button);
                });
            }
        }
    }

    int GetAmountOfCarsOfType(VehicleType type) {
        int amount = 0;
        foreach (Vehicle vehicle in vehicles) {
            if (vehicle.template.type == type) amount++;
        }
        return amount;
    }

    public void CreateVehicle(VehicleType type) {
        foreach (VehicleTemplate template in vehicleTemplates)
            if (type == template.type)
                vehicles.Add(new Vehicle(template));
    }

    public void RecordEmissions(float amount) {
        emissions += amount;
    }

    public void SubmitRating(float rating) {
        ratings += rating;
        packagesDelivered++;
    }

    List<Vehicle> GetVehiclesOfType(VehicleType type) {
        List<Vehicle> vehiclesOfType = new List<Vehicle>();
        foreach (Vehicle vehicle in vehicles) {
            if (vehicle.template.type == type) vehiclesOfType.Add(vehicle);
        }
        return vehiclesOfType;
    }

    int GetAvalibleVehicles(VehicleType type) {
        List<Vehicle> vehiclesOfType = GetVehiclesOfType(type);
        int avalible = 0;
        foreach (Vehicle vehicle in vehiclesOfType)
            if (vehicle.Avalible()) avalible++;


        return avalible;
    }



    public void SelectButton(DeployButton button) {
        if (GetAvalibleVehicles(button.type) > 0) {
            audio.clip = click;
            audio.Play();

            DeselectAllButtons();
            selectedButton = button;
            button.selected.enabled = true;
            VehicleTemplate selectedVehicleTemplate = GetTemplate(selectedButton.type);


            path.canDraw = true;
            path.capacity = 0;
            path.maxCapacity = selectedVehicleTemplate.capacity;
            path.mode = selectedButton.type == VehicleType.Bicycle ? PathDrawer.DrawMode.Bicycle : PathDrawer.DrawMode.Car;
        }
    }

    public float GetTimePassed() {
        return Time.time - timeStarted;
    }

    void DeselectAllButtons() {
        selectedButton = null;
        path.canDraw = false;
        foreach (DeployButton button in deployButtons) {
            button.selected.enabled = false;
        }
    }

    public void SpawnPackage(Vector3 spawnPoint) {
        GameObject package = Instantiate(packageModels[UnityEngine.Random.Range(0, packageModels.Length)], packages);
        package.transform.position = spawnPoint;
    }

    public void DeployVehicle(CollectOrder order) {
        foreach (Vehicle vehicle in GetVehiclesOfType(selectedButton.type)) {
            if (vehicle.Avalible()) {
                VehicleMovementScript obj = Instantiate(car, transform).GetComponent<VehicleMovementScript>();
                obj.order = order;
                obj.vm = this;
                obj.vehicle = vehicle;
                if (vehicle.template.ev) vehicle.charge = 0;
                vehicle.driving = true;

                obj.SetModel(vehicle.template.model);

                return;
            }
        }
    }

    public void ReturnVehicle(Vehicle vehicle) {
        vehicle.driving = false;
        if (vehicle.template.ev) {
            vehicle.charge = 0;
        }
    }

    VehicleTemplate GetTemplate(VehicleType type) {
        foreach (VehicleTemplate template in vehicleTemplates) {
            if (template.type == type) return template;
        }
        return new VehicleTemplate();
    }

    // Update is called once per frame
    void Update() {
        foreach (DeployButton button in deployButtons) {
            button.amount.text = GetAvalibleVehicles(button.type) + "/" + GetVehiclesOfType(button.type).Count;
            button.disabled.enabled = GetAvalibleVehicles(button.type) == 0;

            if (GetTemplate(button.type).ev) {
                Vehicle mostChargedNotFull = null;
                foreach (Vehicle vehicle in GetVehiclesOfType(button.type)) {
                    if (vehicle.template.ev) {
                        if (vehicle.charge != 100 && (mostChargedNotFull == null || mostChargedNotFull.charge < vehicle.charge)) {
                            mostChargedNotFull = vehicle;
                        }
                    }
                }
                button.charge.value = mostChargedNotFull == null ? 100 : mostChargedNotFull.charge;
            }
        }
        if (selectedButton != null && GetAvalibleVehicles(selectedButton.type) == 0) DeselectAllButtons();


        foreach (Vehicle vehicle in vehicles) {
            if (vehicle.template.ev && vehicle.charge != 100 && !vehicle.driving) {
                vehicle.charge += Time.deltaTime / vehicle.template.chargeTime * 100;
                if (vehicle.charge > 100) vehicle.charge = 100;
            }
        }

        // Balance Emissions
        float emissionsBalance = .5f;
        if (emissions > 0) coSlider.value = (emissions / (packagesDelivered > 0 ? packagesDelivered : 1)) * emissionsBalance;

        // Balance Ratings
        float ratingsBalance = 1.5f;
        if (packagesDelivered > 0) ratingSlider.value = (ratings / packagesDelivered + 1) * ratingsBalance;

        activePackagesDisplay.text = pm.GetAmountToDeliver().ToString();
        packagesCollectedDisplay.text = packagesDelivered.ToString();
        timeDisplay.text = ZeroPadd((GetTimePassed() / 60)) + ":" + ZeroPadd((GetTimePassed() % 60));

    }

    public string ZeroPadd(double value) {
        string result = Math.Floor(value).ToString();
        if (result.Length != 2) result = "0" + result;
        return result;
    }

}
