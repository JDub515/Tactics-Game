using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntigravLauncher : MonoBehaviour {

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void SetVelocity(float power) {
        GetComponent<Rigidbody>().velocity = transform.forward * power;
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
        Ray ray = new Ray(selectedUnit.transform.position, target - selectedUnit.transform.position);
        for (int i = 0; i < 20; i++) {
            if (Physics.SphereCast(ray, .25f, .5f)) {
                break;
            }
            ray.origin += .5f * ray.direction.normalized;
            points.Add(ray.origin);
        }
        VisualizationHelper.ProjectileVisualization(points.ToArray(), visualizationPrefab);
        return target - selectedUnit.transform.position;
    }
}
