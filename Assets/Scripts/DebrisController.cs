using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisController : MonoBehaviour {

    public Material fadeOutMaterial;
    private Rigidbody rb;
    private float timer;
    private bool fadingOut;
    private float massThreshold;
    MeshRenderer meshRenderer;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        massThreshold = rb.mass / 5f;
        meshRenderer = GetComponent<MeshRenderer>();
        timer = .3f;
        fadingOut = false;
        /*if (GetComponent<MeshRenderer>().enabled == true &&
            Physics.Raycast(transform.position, Vector3.up, .13f) &&
            Physics.Raycast(transform.position, Vector3.down, .13f) &&
            Physics.Raycast(transform.position, Vector3.left, .13f) &&
            Physics.Raycast(transform.position, Vector3.right, .13f) &&
            Physics.Raycast(transform.position, Vector3.forward, .13f) &&
            Physics.Raycast(transform.position, Vector3.back, .13f)) {
                GetComponent<MeshRenderer>().enabled = false;
        }*/
    }

    void Start () {
        //GetComponent<MeshRenderer>().enabled = true;
    }
	
	void Update () {
        if (!fadingOut && rb.mass < massThreshold) {
            StartCoroutine("FadeOut");
            fadingOut = true;
        }
		if (!fadingOut && rb.velocity.magnitude < .1f) {
            timer -= Time.deltaTime;
            if (timer < 0) {
                rb.isKinematic = true;
                timer = .3f;
                GameController.activeObjects--;
                enabled = false;
            }
        } else {
            timer = .3f;
        }
        /*if (transform.position.magnitude > 1000) {
            Debug.Log(transform);
        }*/
	}

    IEnumerator FadeOut() {
        yield return 0;
        yield return 0;
        Color color = meshRenderer.material.color;
        meshRenderer.material = fadeOutMaterial;
        float timer = 1f;
        while (timer > 0) {
            color.a = timer;
            meshRenderer.material.color = color;
            yield return 0;
            timer -= Time.deltaTime;
            timer -= .2f/rb.velocity.magnitude;
        }
        GameController.activeObjects--;
        Destroy(gameObject);
    }
}
