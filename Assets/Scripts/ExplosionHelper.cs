using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public static class ExplosionHelper {

    public static void Explode(Vector3 explosionCenter, float force, GameObject explosionPrefab, int direction, float falloff) {
        force = force * 200;
        Time.fixedDeltaTime = .02f;
        Collider[] colliders = Physics.OverlapSphere(explosionCenter, Mathf.Sqrt(force) / 8f, LayerMask.GetMask("Swapable Object"));
        foreach (Collider collider in colliders) {
            collider.GetComponent<ObjectSwapper>().SwapObject();
        }
        Rigidbody rb;
        RaycastHit objectStart;
        float forcePower;
        Vector3 objectVector;
        int i = 0;
        colliders = Physics.OverlapSphere(explosionCenter, Mathf.Sqrt(force) / 8f, ~LayerMask.GetMask("Active Object"));
        //float[] forceArray = new float[colliders.Length];
        foreach (Collider collider in colliders) {
            rb = collider.GetComponent<Rigidbody>();
            if (rb != null) {
                objectVector = collider.transform.position - explosionCenter;
                /*Physics.Raycast(explosionCenter, objectVector, out objectStart, objectVector.magnitude);
                if (objectStart.collider == collider) {
                    forcePower = force / Mathf.Pow((1 + (objectStart.point - explosionCenter).magnitude), 2);
                } else {
                    forcePower = force / (Mathf.Pow((1 + objectVector.magnitude), 2) * Mathf.Pow(1 + (collider.transform.position - objectStart.point).magnitude / 5, 2));
                }
                forceArray[i] = forcePower;*/
                if (collider.gameObject.layer == LayerMask.NameToLayer("Debris")) {
                    forcePower = Mathf.Pow(collider.transform.localScale.x, 2) * force / (4 * Mathf.PI * (1f + Mathf.Pow(objectVector.magnitude - (collider.transform.localScale.x / 2f), falloff)));
                    if (forcePower > (rb.mass/2)) {
                        if (!collider.GetComponent<DebrisController>().enabled) {
                            collider.GetComponent<DebrisController>().enabled = true;
                            GameController.activeObjects++;
                        }
                        float oldMass = rb.mass;
                        rb.mass = Mathf.Max(.2f, oldMass / (1 + forcePower / 5f));
                        rb.isKinematic = false;
                        float newScale = Mathf.Pow(Mathf.Pow((collider.transform.localScale.x), 3) * (rb.mass/oldMass), 1.0f / 3.0f);
                        collider.transform.localScale = new Vector3(newScale, newScale, newScale);
                        rb.AddForce(direction * forcePower * objectVector.normalized, ForceMode.Impulse);
                    }
                } else if (collider.gameObject.layer == LayerMask.NameToLayer("Unit")) {
                    Physics.Raycast(explosionCenter, objectVector, out objectStart, objectVector.magnitude);
                    if (objectStart.collider == collider) {
                        forcePower = 4 * force / (4 * Mathf.PI * (1f + Mathf.Pow((objectStart.point - explosionCenter).magnitude, falloff)));
                    } else {
                        forcePower = 4 * force / (8 * Mathf.PI * (1f + Mathf.Pow(objectVector.magnitude, falloff)));
                    }
                    rb.AddForce(direction * forcePower * objectVector.normalized, ForceMode.Impulse);
                    collider.gameObject.GetComponent<BaseUnitController>().RecieveDamage(forcePower / 5f);
                }
            }
            i++;
        }
        /*i = 0;
        foreach (Collider collider in colliders) {
            rb = collider.GetComponent<Rigidbody>();
            if (rb != null && forceArray[i] > 2.5f * rb.mass && collider.gameObject.layer == LayerMask.NameToLayer("Debris")) {
                float newScale = Mathf.Pow(Mathf.Pow((collider.transform.localScale.x), 3) - forceArray[i] / (25f * rb.mass), 1.0f / 3.0f);
                collider.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
            i++;
        }*/
        GameObject.Instantiate(explosionPrefab, explosionCenter, Quaternion.identity);
        //EditorApplication.isPaused = true;
    }
}
