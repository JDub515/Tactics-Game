using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialRandomizer : MonoBehaviour {

    public Material[] materials;

    void Start() {
        if (Random.Range(0, 4) < 3) {
            //GetComponent<Renderer>().material = materials[Mathf.Abs(transform.parent.name.GetHashCode()) % materials.Length];
        } else {
            GetComponent<Renderer>().material = materials[Random.Range(0, materials.Length)];
        }
    }
}
