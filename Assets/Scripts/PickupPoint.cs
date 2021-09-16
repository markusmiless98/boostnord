using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupPoint : MonoBehaviour {

    public Text text;
    public int packagesToDeliver = 0;


    void Update() {
        text.text = "Packages " + packagesToDeliver;
    }
}
