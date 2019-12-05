using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSwapper : MonoBehaviour {

    public GameObject newObject;

	// Use this for initialization
	void Start () {
        enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SwapObject() {
        GameObject replacement = GameController.gameController.GetDebris();
        replacement.transform.position = transform.position;
        replacement.transform.rotation = transform.rotation;
        Destroy(gameObject);
    }
}
