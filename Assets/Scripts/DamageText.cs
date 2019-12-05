using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageText : MonoBehaviour {

    private float lifeTime;

    // Start is called before the first frame update
    void Start() {
        lifeTime = 0;
    }

    // Update is called once per frame
    void Update() {
        lifeTime += Time.deltaTime;
        if (lifeTime > 2) {
            Destroy(gameObject);
        }
        transform.position += transform.up * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }
}
