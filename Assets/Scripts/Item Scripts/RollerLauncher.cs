using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollerLauncher : MonoBehaviour {

    private Rigidbody rb;
    private ConstantForce constantForce;
    private bool rotating;
    private int rotateSpeed;

    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody>();
        constantForce = GetComponent<ConstantForce>();
        rotating = false;
        rb.maxAngularVelocity = 10;
        transform.position += .75f * Vector3.down;
    }

    // Update is called once per frame
    void Update() {

    }

    void FixedUpdate() {
        rb.angularVelocity = transform.right * 10;
        if (!Physics.Raycast(transform.position, constantForce.force, .26f)) {
            RaycastHit hit;
            Collider[] results = new Collider[1];
            if (!rotating && Physics.Raycast(transform.position + constantForce.force.normalized * .26f, Vector3.Cross(transform.right, constantForce.force), out hit, .3f)) {
                Debug.DrawLine(transform.position + constantForce.force.normalized * .26f, transform.position + constantForce.force.normalized * .26f + .3f * Vector3.Cross(transform.right, constantForce.force).normalized, Color.blue, 10);
                StopAllCoroutines();
                rotateSpeed = 6;
                StartCoroutine("Rotate", hit.normal);
            } else if (Physics.OverlapSphereNonAlloc(transform.position, .26f, results, ~LayerMask.GetMask("Active Object")) == 0) {
                Debug.DrawLine(transform.position + constantForce.force.normalized * .26f, transform.position + constantForce.force.normalized * .26f + .3f * Vector3.Cross(transform.right, constantForce.force).normalized, Color.red, 10);
                results[0] = null;
                float x = .26f;
                do {
                    x += .02f;
                    Physics.OverlapSphereNonAlloc(transform.position, x, results, ~LayerMask.GetMask("Active Object"));
                } while (results[0] == null);
                Vector3 magVector = results[0].ClosestPoint(rb.position) - rb.position;
                rb.position += (magVector.magnitude - .255f) * magVector.normalized;
                rb.velocity += 2 * magVector + Vector3.Cross(transform.right, constantForce.force).normalized;
                //Physics.Raycast(transform.position, results[0].transform.position - transform.position, out hit, x + 1);
                //StopAllCoroutines();
                //rb.rotation = Quaternion.FromToRotation(constantForce.force, -hit.normal) * rb.rotation;
                //constantForce.force = -10 * hit.normal;
                //Debug.DrawRay(hit.point, hit.normal, Color.yellow, 10);
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        StopAllCoroutines();
        rotateSpeed = 15;
        StartCoroutine("Rotate", collision.GetContact(0).normal);
        Debug.DrawRay(collision.GetContact(0).point, collision.GetContact(0).normal, Color.green, 10);
    }

    IEnumerator Rotate(Vector3 destinationNormal) {
        rotating = true;
        Quaternion currentRotation = rb.rotation;
        Quaternion destinationRotation = Quaternion.FromToRotation(constantForce.force, -destinationNormal) * rb.rotation;
        Vector3 currentForce = constantForce.force;
        Vector3 destinationForce = -10 * destinationNormal;
        for (float t = 0; t <= 1; t += (rotateSpeed * Time.fixedDeltaTime * 100) / Quaternion.Angle(currentRotation, destinationRotation)) {
            rb.rotation = Quaternion.Lerp(currentRotation, destinationRotation, t);
            constantForce.force = Vector3.Lerp(currentForce, destinationForce, t);
            yield return new WaitForFixedUpdate();
        }
        rb.rotation = destinationRotation;
        constantForce.force = destinationForce;
        rotating = false;
    }

    public static Vector3 CreateVisualization(GameObject selectedUnit, GameObject visualizationPrefab) {
        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 60, ~LayerMask.GetMask("Ignore Raycast"))) {
            return Vector3.zero;
        }
        Vector3 target = new Vector3(hit.point.x, selectedUnit.transform.position.y, hit.point.z);

        if (Vector3.Distance(selectedUnit.transform.position, target) < .6f || Physics.Raycast(selectedUnit.transform.position, target - selectedUnit.transform.position, .6f)) {
            return Vector3.zero;
        }

        List<Vector3> points = new List<Vector3>();
        Ray ray = new Ray(selectedUnit.transform.position + .749f * Vector3.down, target - selectedUnit.transform.position);
        for (int i = 0; i < 20; i++) {
            if (Physics.SphereCast(ray, .25f, .5f)) {
                break;
            }
            ray.origin += .5f * ray.direction.normalized;
            if (!Physics.Raycast(ray.origin, Vector3.down, .3f)) {
                break;
            }
            points.Add(ray.origin);
        }
        VisualizationHelper.ProjectileVisualization(points.ToArray(), visualizationPrefab);
        return target - selectedUnit.transform.position;
    }
}
