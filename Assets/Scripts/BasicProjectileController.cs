using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectileController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        GetComponent<Rigidbody>().velocity = transform.forward * 10f;
    }

    // Update is called once per frame
    void Update() {
        
    }

    private void OnCollisionEnter(Collision collision) {
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().drag = 2;
        gameObject.layer = LayerMask.NameToLayer("Debris");
        enabled = false;
    }
}
