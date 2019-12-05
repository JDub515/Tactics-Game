using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrapnelController : MonoBehaviour {

    private float time;
    private bool destroy;

    // Start is called before the first frame update
    void Start() {
        time = 0;
        destroy = false;
        GetComponent<Rigidbody>().velocity = Random.onUnitSphere * 10;
    }

    // Update is called once per frame
    void FixedUpdate() {
        time += Time.deltaTime;
        if (time > 1) {
            GameController.activeObjects--;
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Unit")) {
            destroy = true;
        }
    }

    private void LateUpdate() {
        if (destroy) {
            GameController.activeObjects--;
            Destroy(gameObject);
        }
    }
}
