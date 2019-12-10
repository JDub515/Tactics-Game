using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {

    public GameObject levelExit;    
    public GameObject[] basicTiles;
    public GameObject[] lowOnlyTiles;
    public GameObject[] floors;
    public GameObject[] floorTransitions;
    public GameObject[] floorTransitions2;

    private int[,] connectionGraph;

    public Vector3 GenerateLevel(int xSize, int zSize) {
        Transform levelContainer = GameObject.Find("Level Container").transform;

        int[,] heightIndex = new int[xSize, zSize];

        connectionGraph = new int[xSize, zSize];
        int newGraphLayer = 0;
        bool connected;

        List<int> weightedHeight;

        for (int i = 0; i < xSize; i++) {
            for (int j = 0; j < zSize; j++) {
                connected = false;
                weightedHeight = new List<int>(new int[] { 0, 1, 2 });
                if (i != 0) {
                    weightedHeight.Add(heightIndex[i - 1, j]);
                }
                if (j != 0) {
                    weightedHeight.Add(heightIndex[i, j - 1]);
                }
                heightIndex[i, j] = weightedHeight[Random.Range(0, weightedHeight.Count)];

                if (j != 0) {
                    if (heightIndex[i, j] == heightIndex[i, j - 1]) {
                        connectionGraph[i, j] = connectionGraph[i, j - 1];
                        connected = true;
                    }
                }
                if (i != 0) {
                    if (heightIndex[i, j] == heightIndex[i - 1, j]) {
                        connectionGraph[i, j] = connectionGraph[i - 1, j];
                        connected = true;
                    }
                }
                if (!connected) {
                    connectionGraph[i, j] = newGraphLayer;
                    newGraphLayer++;
                }
            }
        }


        int height;
        List<Vector3> importantTransitionDirections = new List<Vector3>();
        List<Vector3> importantTransitionDirections2 = new List<Vector3>();
        List<Vector3> transitionDirections = new List<Vector3>();
        List<Vector3> transitionDirections2 = new List<Vector3>();

        List<int> locations = new List<int>();
        for (int i = 0; i < xSize * zSize; i++) {
            locations.Add(i);
        }
        for (int i = xSize * zSize; i > 0; i--) {
            int index = Random.Range(0, i);
            locations.Add(locations[index]);
            locations.RemoveAt(index);
        }

        Vector3 startLocation = Vector3.zero;
        bool placedExit = false;
        Vector3 tileLocation;

        foreach (int loc in locations) {
            int i = loc / zSize;
            int j = loc % xSize;

            height = heightIndex[i, j];
            tileLocation = new Vector3(7 + 10 * i, 0, 7 + 10 * j);

            if (height != 0) {
                GameObject.Instantiate(floors[height - 1], tileLocation, Quaternion.identity, levelContainer);
            }

            importantTransitionDirections.Clear();
            transitionDirections.Clear();
            if (i != 0 && heightIndex[i-1, j] == height + 1) {
                transitionDirections.Add(new Vector3(-1, 0, 0));
                if (connectionGraph[i, j] != connectionGraph[i - 1, j]) {
                    importantTransitionDirections.Add(new Vector3(-1, 0, 0));
                }
            }
            if (j != 0 && heightIndex[i, j - 1] == height + 1) {
                transitionDirections.Add(new Vector3(0, 0, -1));
                if (connectionGraph[i, j] != connectionGraph[i, j - 1]) {
                    importantTransitionDirections.Add(new Vector3(0, 0, -1));
                }
            }
            if (i != xSize - 1 && heightIndex[i + 1, j] == height + 1) {
                transitionDirections.Add(new Vector3(1, 0, 0));
                if (connectionGraph[i, j] != connectionGraph[i + 1, j]) {
                    importantTransitionDirections.Add(new Vector3(1, 0, 0));
                }
            }
            if (j != zSize - 1 && heightIndex[i, j + 1] == height + 1) {
                transitionDirections.Add(new Vector3(0, 0, 1));
                if (connectionGraph[i, j] != connectionGraph[i, j + 1]) {
                    importantTransitionDirections.Add(new Vector3(0, 0, 1));
                }
            }

            importantTransitionDirections2.Clear();
            transitionDirections2.Clear();
            if (i != 0 && heightIndex[i - 1, j] == height + 2) {
                transitionDirections2.Add(new Vector3(-1, 0, 0));
                if (connectionGraph[i, j] != connectionGraph[i - 1, j]) {
                    importantTransitionDirections2.Add(new Vector3(-1, 0, 0));
                }
            }
            if (j != 0 && heightIndex[i, j - 1] == height + 2) {
                transitionDirections2.Add(new Vector3(0, 0, -1));
                if (connectionGraph[i, j] != connectionGraph[i, j - 1]) {
                    importantTransitionDirections2.Add(new Vector3(0, 0, -1));
                }
            }
            if (i != xSize - 1 && heightIndex[i + 1, j] == height + 2) {
                transitionDirections2.Add(new Vector3(1, 0, 0));
                if (connectionGraph[i, j] != connectionGraph[i + 1, j]) {
                    importantTransitionDirections2.Add(new Vector3(1, 0, 0));
                }
            }
            if (j != zSize - 1 && heightIndex[i, j + 1] == height + 2) {
                transitionDirections2.Add(new Vector3(0, 0, 1));
                if (connectionGraph[i, j] != connectionGraph[i, j + 1]) {
                    importantTransitionDirections2.Add(new Vector3(0, 0, 1));
                }
            }

            tileLocation += new Vector3(0, height * 2, 0);

            if (importantTransitionDirections.Count + importantTransitionDirections2.Count > 0) {
                Vector3 chosenDirection;
                if (Random.Range(0, importantTransitionDirections.Count + importantTransitionDirections2.Count) < importantTransitionDirections.Count) {
                    chosenDirection = importantTransitionDirections[Random.Range(0, importantTransitionDirections.Count)];
                    GameObject.Instantiate(floorTransitions[Random.Range(0, floorTransitions.Length)], tileLocation, Quaternion.FromToRotation(Vector3.right, -chosenDirection), levelContainer);
                } else {
                    chosenDirection = importantTransitionDirections2[Random.Range(0, importantTransitionDirections2.Count)];
                    GameObject.Instantiate(floorTransitions2[Random.Range(0, floorTransitions2.Length)], tileLocation, Quaternion.FromToRotation(Vector3.right, -chosenDirection), levelContainer);
                }
                ConnectLevels(connectionGraph[i, j], connectionGraph[i + (int)chosenDirection.x, j + (int)chosenDirection.z]);
            } else if (j == 0 && startLocation == Vector3.zero) {
                startLocation = tileLocation;
            } else if (j == zSize - 1 && !placedExit) {
                GameObject.Instantiate(levelExit, tileLocation, Quaternion.Euler(new Vector3(0, 90 * Random.Range(0, 4), 0)), levelContainer);
                placedExit = true;
            } else if (Random.Range(0, (int)Mathf.Pow(transitionDirections.Count + transitionDirections2.Count, 2) + 2) > 1) {
                if (Random.Range(0, transitionDirections.Count + transitionDirections2.Count) < transitionDirections.Count) {
                    GameObject.Instantiate(floorTransitions[Random.Range(0, floorTransitions.Length)], tileLocation, Quaternion.FromToRotation(Vector3.right, -transitionDirections[Random.Range(0, transitionDirections.Count)]), levelContainer);
                } else {
                    GameObject.Instantiate(floorTransitions2[Random.Range(0, floorTransitions2.Length)], tileLocation, Quaternion.FromToRotation(Vector3.right, -transitionDirections2[Random.Range(0, transitionDirections2.Count)]), levelContainer);
                }
            } else {
                if (height != 2 && Random.Range(0, lowOnlyTiles.Length + basicTiles.Length) < lowOnlyTiles.Length) {
                    GameObject.Instantiate(lowOnlyTiles[Random.Range(0, lowOnlyTiles.Length)], tileLocation, Quaternion.Euler(new Vector3(0, 90 * Random.Range(0, 4), 0)), levelContainer);
                } else {
                    GameObject.Instantiate(basicTiles[Random.Range(0, basicTiles.Length)], tileLocation, Quaternion.Euler(new Vector3(0, 90 * Random.Range(0, 4), 0)), levelContainer);
                }
            }
        }

        return startLocation;
    }

    void ConnectLevels(int level1, int level2) {
        for (int i = 0; i < connectionGraph.GetLength(0); i++) {
            for (int j = 0; j < connectionGraph.GetLength(1); j++) {
                if (connectionGraph[i, j] == level2) {
                    connectionGraph[i, j] = level1;
                }
            }
        }
    }
}
