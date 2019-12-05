using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {

    Transform objTransform;

    // Start is called before the first frame update
    void Start() {
        objTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void LateUpdate() {
        objTransform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }
}
