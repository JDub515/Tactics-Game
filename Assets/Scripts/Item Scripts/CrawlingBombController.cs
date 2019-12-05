using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingBombController : MonoBehaviour {

    public int velocity;
    public float maxLifetime;

    private float lifetime;
    private Rigidbody rb;
    private Vector3 direction;
    private Quaternion oldRotation;
    private List<Quaternion> newRotations;
    private Vector3 oldPosition;
    private List<Vector3> newPositions;
    private float turnCounter;
    private Vector3 originalForward;

    Vector3 previousPosition;
    Vector3 previousForward;

    // Use this for initialization
    void Start () {
        lifetime = 0;
        rb = GetComponent<Rigidbody>();
        direction = transform.forward;
        newRotations = new List<Quaternion>();
        newPositions = new List<Vector3>();
        previousPosition = transform.position;
        previousForward = transform.forward;
    }

    // Update is called once per frame
    void Update() {
        lifetime += Time.deltaTime;
        if (lifetime > maxLifetime) {
            Destroy(gameObject);
            return;
        }
    }

    private void FixedUpdate() {
        Debug.DrawLine(previousPosition, transform.position, Color.red, 20);
        Debug.DrawLine(previousPosition + previousForward * .25f, transform.position + transform.forward * .25f, Color.blue, 20);
        previousPosition = transform.position;
        previousForward = transform.forward;
        if (newRotations.Count > 0) {
            if (turnCounter < 1) {
                turnCounter += .01f;//velocity * Time.fixedDeltaTime / (2 * Vector3.Distance(oldPosition, newPositions[0]));
                Quaternion newRotation = Quaternion.Lerp(oldRotation, newRotations[0], turnCounter);
                float radians = .01f * (Mathf.PI / 2);
                float h = .5f * (1 - Mathf.Cos(radians)) / Mathf.Sin(radians / 2);
                rb.MoveRotation(newRotation);
                rb.MovePosition(rb.position + (previousForward + newRotation * originalForward).normalized * h);
                return;
                /*if (turnCounter < 1) {
                    turnCounter += velocity * Time.fixedDeltaTime / (2 * Vector3.Distance(oldPosition, newPositions[0]));
                    rb.MoveRotation(Quaternion.Lerp(oldRotation, newRotations[0], turnCounter));
                    if (turnCounter < 0.5f) {
                        rb.MovePosition(Vector3.Lerp(oldPosition, newPositions[0], turnCounter * 2));
                    } else {
                        rb.MovePosition(Vector3.Lerp(newPositions[0], newPositions[1], (turnCounter * 2) - 1));
                    }
                    return;*/
            } else if (newRotations.Count > 1) {
                oldRotation = newRotations[0];
                oldPosition = newPositions[1];
                newRotations.RemoveAt(0);
                newPositions.RemoveRange(0, 2);
                turnCounter =  velocity * Time.fixedDeltaTime;
                rb.MoveRotation(Quaternion.Lerp(oldRotation, newRotations[0], turnCounter));
                rb.MovePosition(Vector3.Lerp(oldPosition, newPositions[0], turnCounter));
                return;
            } else {
                turnCounter = 0;
                newRotations.RemoveAt(0);
                newPositions.RemoveRange(0, 2);
            }
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position - .245f * transform.up, transform.forward, out hit, .75f)) {
            oldRotation = rb.rotation;
            newRotations.Add(Quaternion.FromToRotation(transform.up, hit.normal) * rb.rotation);
            oldPosition = transform.position;
            RaycastHit hit2;
            Physics.Raycast(transform.position + .25f * transform.forward + .25f * (Quaternion.FromToRotation(transform.up, hit.normal) * transform.forward), -hit.normal, out hit2, .5f);
            newPositions.Add(hit2.point + .251f * hit2.normal);
            newPositions.Insert(0, (oldPosition + newPositions[0]) / 2 + .0f * (transform.up + hit.normal).normalized);
        } else if (!Physics.Raycast(transform.position + .25f * transform.forward , -transform.up, out hit, .255f)) {
            Physics.Raycast(transform.position + .25f * transform.forward - transform.up * .255f, -transform.forward, out hit, 10);
            oldRotation = rb.rotation;
            newRotations.Add(Quaternion.FromToRotation(transform.up, hit.normal) * rb.rotation);
            originalForward = transform.forward;
            oldPosition = transform.position;
            RaycastHit hit2;
            float dist = .25f * Mathf.Cos((90 - Vector3.Angle(transform.up, hit.normal)) * Mathf.Deg2Rad) / Mathf.Cos(Vector3.Angle(-Vector3.RotateTowards(transform.up, hit.normal, -(90 - Vector3.Angle(transform.up, hit.normal)) * Mathf.Deg2Rad, 1), Quaternion.FromToRotation(transform.up, hit.normal) * transform.forward) * Mathf.Deg2Rad);
            //Physics.Raycast(transform.position + .25f * transform.forward + (dist + .25f) * (Quaternion.FromToRotation(transform.up, hit.normal) * transform.forward), -hit.normal, out hit2, .5f);
            Vector3 finalDest = transform.position + .25f * transform.forward + (dist + .25f) * (Quaternion.FromToRotation(transform.up, hit.normal) * transform.forward);
            float dist2 = Vector3.Dot(finalDest - hit.point, hit.normal);
            Debug.DrawLine(transform.position + .25f * transform.forward, transform.position + .25f * transform.forward + (dist + .25f) * (Quaternion.FromToRotation(transform.up, hit.normal) * transform.forward), Color.cyan, 10);
            Debug.DrawLine(transform.position + .25f * transform.forward + (dist + .25f) * (Quaternion.FromToRotation(transform.up, hit.normal) * transform.forward), finalDest + (.251f - dist2) * hit.normal, Color.cyan, 10);
            newPositions.Add(finalDest + (.251f - dist2) * hit.normal);
            Debug.Log(newPositions[0].x - oldPosition.x);
            Debug.Log(newPositions[0].y - oldPosition.y);
            Debug.Log(newPositions[0].z - oldPosition.z);
            Physics.Raycast((oldPosition + newPositions[0]) / 2, -transform.up - hit.normal, out hit2, .25f);
            Debug.DrawRay((oldPosition + newPositions[0]) / 2, -transform.up - hit.normal, Color.green, 10);
            newPositions.Insert(0, hit2.point + .25f * ((transform.up + hit.normal).normalized));
        } else {
            rb.MovePosition(rb.position + transform.forward * velocity * Time.deltaTime);
        }
        Vector3 oldNormal = hit.normal;
        Vector3 oldForward = Quaternion.FromToRotation(transform.up, hit.normal) * transform.forward;
        //Debug.DrawRay(oldPosition, transform.forward, Color.red, 10);
        //Debug.DrawRay(newPositions[0], -oldForward, Color.red, 10);
        if (newRotations.Count > 1) {
            while (Physics.Raycast(hit.point + .01f * hit.normal, oldForward, out hit, .51f)) {
                //newPositions[newPositions.Count - 1] = newPositions[newPositions.Count - 1] - Vector3.Distance(newPositions[newPositions.Count - 1], hit.point) * oldForward;
                newRotations.Add(Quaternion.FromToRotation(oldNormal, hit.normal) * newRotations[newRotations.Count - 1]);
                oldForward = (Quaternion.FromToRotation(oldNormal, hit.normal) * oldForward);
                newPositions.Add(hit.point + .251f * hit.normal + .25f * oldForward);
                oldNormal = hit.normal;
            }
        }
    }
}

