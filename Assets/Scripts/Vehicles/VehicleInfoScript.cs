using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VehicleInfoScript : MonoBehaviour
{
    [Tooltip("How many packages can be carried by the vehicle.")] public int VehicleStorage = 0;
    [Tooltip("The effective range of the vehicle. Higher is better, as a penalty is to be applied when suprassing it.")] public float VehicleEffectiveRange = 100f;
    [Tooltip("The speed of the vehicle, faster is better.")] public float VehicleMovementSpeed = 50f;
    [Tooltip("The CO2 output from vehicle driving around. Higher is worse.")] public float VehicleCO2Output = 1000f;
    [Tooltip("If vehicle is affected by weather, etc. Bikes aren't while a car is.")] public bool IsWeatherResistantVehicle = true;

    // Add stuff tomorrow uwu
}