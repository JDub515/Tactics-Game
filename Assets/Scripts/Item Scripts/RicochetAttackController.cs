using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RicochetAttackController : MonoBehaviour {

    public int velocity;
    public float maxLifetime;
    public GameObject laserSegment;

    private List<LineRenderer> lineSegments;
    private float lifetime;
    private List<Vector3> destinations;
    private Vector3 nextDirection;
    private Vector3 firstPoint;
    private Vector3 lastPoint;

	// Use this for initialization
	void Start () {
        lifetime = 0;
        lineSegments = new List<LineRenderer>();
        destinations = new List<Vector3>();
        lineSegments.Add(Instantiate(laserSegment, transform).GetComponent<LineRenderer>());
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 100)) {
            destinations.Add(hit.point);
            nextDirection = Vector3.Reflect(transform.forward, hit.normal);
        } else {
            destinations.Add(transform.position + transform.forward * 100);
        }
        firstPoint = transform.position + transform.forward * .6f;
        lastPoint = transform.position - transform.forward * .4f;
        lineSegments[0].positionCount = 2;
        lineSegments[0].SetPositions(new Vector3[] {firstPoint, lastPoint});
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        lifetime += Time.deltaTime;
        if (lifetime > maxLifetime) {
            GameController.activeObjects--;
            Destroy(gameObject);
            return;
        }
        float distanceTraveled = 0;
        while (Vector3.Distance(firstPoint, destinations[0]) < velocity * Time.deltaTime - distanceTraveled) {
            Vector3 direction = destinations[0] - firstPoint;
            RaycastHit hit;
            Physics.Raycast(firstPoint, direction, out hit, 10, ~LayerMask.GetMask("Unit Trigger"));
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Unit")) {
                hit.transform.GetComponent<BaseUnitController>().RecieveDamage(10);
            }
            if (Physics.Raycast(destinations[0], nextDirection, out hit, 100)) {
                destinations.Insert(0, hit.point);
                nextDirection = Vector3.Reflect(nextDirection, hit.normal);
            } else {
                destinations.Insert(0, destinations[0] + nextDirection.normalized * 100);
            }
            distanceTraveled += direction.magnitude;
            firstPoint = destinations[1];
        }
        firstPoint = Vector3.MoveTowards(firstPoint, destinations[0], velocity * Time.deltaTime - distanceTraveled);

        distanceTraveled = 0;
        while (Vector3.Distance(lastPoint, destinations[destinations.Count - 1]) < velocity * Time.deltaTime - distanceTraveled) {
            distanceTraveled += Vector3.Distance(lastPoint, destinations[destinations.Count - 1]);
            lastPoint = destinations[destinations.Count - 1];
            destinations.RemoveAt(destinations.Count - 1);
        }
        lastPoint = Vector3.MoveTowards(lastPoint, destinations[destinations.Count - 1], velocity * Time.deltaTime - distanceTraveled);

        while (lineSegments.Count != destinations.Count) {
            if (lineSegments.Count < destinations.Count) {
                lineSegments.Add(Instantiate(laserSegment, transform).GetComponent<LineRenderer>());
                lineSegments[lineSegments.Count - 1].positionCount = 2;
            } else {
                Destroy(lineSegments[lineSegments.Count - 1].gameObject);
                lineSegments.RemoveAt(lineSegments.Count - 1);
            }
        }

        lineSegments[0].SetPosition(1, firstPoint);
        lineSegments[lineSegments.Count - 1].SetPosition(0, lastPoint);
        for (int i = 1; i < destinations.Count; i++) {
            lineSegments[i - 1].SetPosition(0, destinations[i]);
            lineSegments[i].SetPosition(1, destinations[i]);
        }
         
        transform.position = (firstPoint + lastPoint) / 2;
    }


    public static Vector3 CreateVisualization(GameObject selectedUnit, GameObject visualizationPrefab) {
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
        Vector3 target = new Vector3(hit.point.x, selectedUnit.transform.position.y, hit.point.z);

        if (Vector3.Distance(selectedUnit.transform.position, target) < .6f || Physics.Raycast(selectedUnit.transform.position, target - selectedUnit.transform.position, .6f)) {
            return target;
        }

        float visualizationDistance = 10;
        float visualizationSeparation = 0.5f;
        Vector3 nextDestination;
        List<Vector3> points = new List<Vector3>();
        Vector3 previousPoint = selectedUnit.transform.position;
        float distanceTraveled = 0;
        Vector3 nextDirection = target - selectedUnit.transform.position;

        if (Physics.Raycast(selectedUnit.transform.position, nextDirection, out hit, (visualizationDistance + 1))) {
            nextDestination = hit.point;
            nextDirection = Vector3.Reflect(nextDirection, hit.normal);
        } else {
            nextDestination = selectedUnit.transform.position + nextDirection.normalized * (visualizationDistance + 1);
        }

        while (distanceTraveled < visualizationDistance) {
            float tempDistance = 0;
            while (Vector3.Distance(previousPoint, nextDestination) < visualizationSeparation - tempDistance) {
                tempDistance += Vector3.Distance(previousPoint, nextDestination);
                Ray dirRay = new Ray(nextDestination, nextDirection);
                previousPoint = nextDestination;
                if (Physics.Raycast(dirRay, out hit, (visualizationDistance + 1) - distanceTraveled)) {
                    nextDestination = hit.point;
                    nextDirection = Vector3.Reflect(nextDirection, hit.normal);
                } else {
                    nextDestination = dirRay.GetPoint((visualizationDistance + 1) - distanceTraveled);
                }
            }
            points.Add(Vector3.MoveTowards(previousPoint, nextDestination, visualizationSeparation - tempDistance));
            previousPoint = points[points.Count - 1];
            distanceTraveled += visualizationSeparation;
        }
        VisualizationHelper.ProjectileVisualization(points.ToArray(), visualizationPrefab);
        return target - selectedUnit.transform.position;
    }
}
