using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCleanUp : MonoBehaviour {

    public float lifeSpan;

    private float timer;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        timer += Time.deltaTime;
        if (timer > lifeSpan) {
            Destroy(gameObject);
        }
    }
}
