using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultLauncher : MonoBehaviour {

	// Use this for initialization
	void Start() {
        
    }
	
	// Update is called once per frame
	void Update() {

	}

    public void SetVelocity(float power) {
        GetComponent<Rigidbody>().velocity = transform.forward * power;
    }

    public static Vector3 CreateVisualization(GameObject selectedUnit, GameObject visualizationPrefab, float power) {
        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 60, ~LayerMask.GetMask("Ignore Raycast"))) {
            return Vector3.zero;
        }
        Vector3 target = hit.point;

        if (Vector3.Distance(selectedUnit.transform.position, target) < 1.3f) {
            return Vector3.zero;
        }

        float g = Physics.gravity.magnitude;
        float h = target.y - selectedUnit.transform.position.y;
        float d = Vector2.Distance(new Vector2(selectedUnit.transform.position.x, selectedUnit.transform.position.z), new Vector2(target.x, target.z));
        float step1 = Mathf.Pow(power, 4) - g * (g * Mathf.Pow(d, 2) + 2 * h * Mathf.Pow(power, 2));
        if (step1 < 0) {
            float upperbound = d;
            float lowerbound = 0;
            Ray directionRay = new Ray(new Vector3(selectedUnit.transform.position.x, 0, selectedUnit.transform.position.z), new Vector3(target.x, 0, target.z) - new Vector3(selectedUnit.transform.position.x, 0, selectedUnit.transform.position.z));
            int ii = 0;
            while ((Mathf.Abs(step1) > 10 || step1 < 0) && ii < 100) {
                Vector3 nextTest = directionRay.GetPoint((upperbound + lowerbound) / 2);
                Physics.Raycast(new Vector3(nextTest.x, 20, nextTest.z), Vector3.down, out hit);
                h = hit.point.y - selectedUnit.transform.position.y;
                d = Vector2.Distance(new Vector2(selectedUnit.transform.position.x, selectedUnit.transform.position.z), new Vector2(hit.point.x, hit.point.z));
                step1 = Mathf.Pow(power, 4) - g * (g * Mathf.Pow(d, 2) + 2 * h * Mathf.Pow(power, 2));
                if (step1 > 0) {
                    lowerbound = (upperbound + lowerbound) / 2;
                    if (upperbound - lowerbound < .01f) {
                        nextTest = directionRay.GetPoint(upperbound);
                        Physics.Raycast(new Vector3(nextTest.x, 20, nextTest.z), Vector3.down, out hit);
                        lowerbound = h;
                        upperbound = hit.point.y - selectedUnit.transform.position.y;
                        while ((Mathf.Abs(step1) > 10 || step1 < 0) && ii < 100) {
                            h = (upperbound + lowerbound) / 2;
                            step1 = Mathf.Pow(power, 4) - g * (g * Mathf.Pow(d, 2) + 2 * h * Mathf.Pow(power, 2));
                            if (step1 > 0) {
                                lowerbound = h;
                            } else {
                                upperbound = h;
                            }
                            ii++;
                        }
                        target = hit.point;
                        break;
                    }
                } else {
                    upperbound = (upperbound + lowerbound) / 2;
                }
                ii++;
            }
        }
        float angle1 = Mathf.Atan((Mathf.Pow(power, 2) + Mathf.Sqrt(step1)) / (g * d));
        float angle2 = Mathf.Atan((Mathf.Pow(power, 2) - Mathf.Sqrt(step1)) / (g * d));
        if (angle2 > angle1) {
            angle1 = angle2;
        }
        float distance = 0;
        List<Vector3> points = new List<Vector3>();
        Vector2 groundCord = new Vector2(selectedUnit.transform.position.x, selectedUnit.transform.position.z);
        RaycastHit sphereHit;
        while (groundCord != new Vector2(target.x, target.z)) {
            float y = Mathf.Tan(angle1) * distance - ((g * Mathf.Pow(distance, 2)) / (Mathf.Pow(power, 2) * (Mathf.Cos(2 * angle1) + 1)));
            points.Add(new Vector3(groundCord.x, y + selectedUnit.transform.position.y, groundCord.y));
            if (points.Count > 1 && Physics.SphereCast(points[points.Count - 2], 0.25f, points[points.Count - 1] - points[points.Count - 2], out sphereHit, Vector3.Distance(points[points.Count - 2], points[points.Count - 1]))) {
                break;
            }
            distance += .2f;

            groundCord = Vector2.MoveTowards(new Vector2(selectedUnit.transform.position.x, selectedUnit.transform.position.z), new Vector2(target.x, target.z), distance);
        }
        VisualizationHelper.ProjectileVisualization(points.ToArray(), visualizationPrefab);
        return Vector3.RotateTowards(new Vector3(target.x - selectedUnit.transform.position.x, 0, target.z - selectedUnit.transform.position.z), Vector3.up, angle1, 1);
    }
}