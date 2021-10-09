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

    public float timer;

    public AudioClip click;
    public AudioSource audio;
    public GameManager gm;

    [Serializable]
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

    [Serializable]
    public class VehicleOption {
        public VehicleType template;
        public int amount = 0;
    }


    public void NewGame(VehicleOption[] options) {
        vehicles.Clear();
        DeselectAllButtons();


        packagesDelivered = 0;
        emissions = 0;
        ratings = 0;
        pm.Reset();

        while (transform.childCount > 0) DestroyImmediate(transform.GetChild(0).gameObject);

        foreach (VehicleOption option in options) {
            for (int i = 0; i < option.amount; i++) {
                CreateVehicle(option.template);
            }
        }

        CreateDeployButtons();
        pm.StartSpawningPackages();
    }

    void CreateDeployButtons() {
        while (deployButtonsParent.childCount > 0) DestroyImmediate(deployButtonsParent.GetChild(0).gameObject);
        deployButtons.Clear();

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



    public void DeselectAllButtons() {
        selectedButton = null;
        path.canDraw = false;
        path.ClearPath();
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

    public void OnEndGame() {
        DeselectAllButtons();

        foreach (Package package in pm.packages) {
            float rating = Time.time - package.timeCreated;
            ratings += rating;
        }
    }

    // Update is called once per frame
    void Update() {
        if (!gm.paused) {
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

            coSlider.value = GetEmissionScore();

            // Balance Ratings
            ratingSlider.value = GetRatingsScore();

            activePackagesDisplay.text = pm.GetAmountToDeliver().ToString();
            packagesCollectedDisplay.text = packagesDelivered.ToString();
            timer -= Time.deltaTime;
            if (timer < 0) timer = 0;
            timeDisplay.text = ZeroPadd((timer / 60)) + ":" + ZeroPadd((timer % 60));

            if (timer <= 0) {
                gm.EndGame();
            }
        }
    }

    public float GetRatingsScore() {
        float ratingsBalance = 1.5f;
        return ratings > 0 ? (ratings / packagesDelivered + 1) * ratingsBalance : 0;
    }

    public float GetEmissionScore() {
        float emissionsBalance = .5f;
        if (emissions == 0) return 0;
        return (emissions / (packagesDelivered > 0 ? packagesDelivered : 1)) * emissionsBalance;
    }

    public string ZeroPadd(double value) {
        string result = Math.Floor(value).ToString();
        if (result.Length != 2) result = "0" + result;
        return result;
    }

}
