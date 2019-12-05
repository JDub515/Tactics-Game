using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VisualizationHelper {

    public static GameObject visualizationContainer;

    public static void ProjectileVisualization(Vector3[] points, GameObject visualizationPrefab) {
        visualizationContainer = new GameObject();
        //float i = points.Length + 6;
        foreach (Vector3 point in points) {
            GameObject visualizationObject = GameObject.Instantiate(visualizationPrefab, point, Quaternion.identity, visualizationContainer.transform);
            //visualizationObject.transform.localScale = (Vector3.one * Mathf.Sqrt(i / (points.Length + 6))) * .3f;
            //i--;
        }
    }

    public static void ResetVisualization() {
        if (visualizationContainer != null) {
            GameObject.Destroy(visualizationContainer);
        }
    }
}
