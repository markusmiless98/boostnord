using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalPackage : MonoBehaviour {

    void Start() {
        StartCoroutine(Kill());
    }

    IEnumerator Kill() {
        yield return new WaitForSeconds(60f);
        DestroyImmediate(gameObject);
    }

}
