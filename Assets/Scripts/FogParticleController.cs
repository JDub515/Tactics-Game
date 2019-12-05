using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogParticleController : MonoBehaviour {

    public static FogParticleController fogParticleController;

    private ParticleSystem.Particle[] particleHolder;
    private ParticleSystem fogParticleSystem;

    private HashSet<int> fadingParticles;

    private List<Vector3> unitPositions = new List<Vector3>();
    private int unitCount;
    private Vector3 singleUnitPosition;
    private int singleUnitIndex;
    private LayerMask visionMask;
    private int particleCount;

    private Vector3[] previousUnitPositions;

    private Vector3 heightModifier;
    private Color32[] fadeColors;
    private Vector3[] enemeyVisionVectors;

    private int xSize;
    private int zSize;
    private int ySize;

    //private int incrementStart;

    private struct Particle {
        public Vector3 position;
        public Vector2 xzPos;
        public bool inVision;
        public float[] unitDistances;
        public int fadeStep;
        public bool underground;

        public Particle(Vector3 pos, bool und) {
            position = pos;
            xzPos = new Vector2(pos.x, pos.z);
            inVision = false;
            unitDistances = new float[4];
            fadeStep = und ? 16 : 0;
            underground = und; 
        }
    }

    private Particle[] particles;

    void Start() {
        fogParticleController = this;

        //incrementStart = 0;
        xSize = 64;
        zSize = 64;
        ySize = 10;
        particleCount = xSize * zSize * ySize;
        heightModifier = Vector3.up * .5f;
        fadeColors = new Color32[] { new Color32(0, 0, 0, 255), new Color32(0, 0, 0, 239), new Color32(0, 0, 0, 223), new Color32(0, 0, 0, 207),
                                     new Color32(0, 0, 0, 191), new Color32(0, 0, 0, 175), new Color32(0, 0, 0, 159), new Color32(0, 0, 0, 143),
                                     new Color32(0, 0, 0, 127), new Color32(0, 0, 0, 111), new Color32(0, 0, 0, 95), new Color32(0, 0, 0, 79),
                                     new Color32(0, 0, 0, 63), new Color32(0, 0, 0, 47), new Color32(0, 0, 0, 31), new Color32(0, 0, 0, 15),
                                     new Color32(0, 0, 0, 0) };

        unitCount = UnitController.playerUnits.Count;
        previousUnitPositions = new Vector3[4];

        for (int i = 0; i < unitCount; i++) {
            previousUnitPositions[i] = new Vector3(-1, -1, -1);
        }

        fogParticleSystem = GetComponent<ParticleSystem>();
        visionMask = LayerMask.GetMask(new string[3] { "Swapable Object", "Debris", "Indestructable Terrain" });

        fadingParticles = new HashSet<int>();
        particles = new Particle[particleCount];
        particleHolder = new ParticleSystem.Particle[particleCount];

        enemeyVisionVectors = new Vector3[] {new Vector3(.5f, .5f, .5f), new Vector3(-.5f, .5f, .5f), new Vector3(.5f, .5f, -.5f), new Vector3(-.5f, .5f, -.5f),
                                             new Vector3(.5f, -.5f, .5f), new Vector3(-.5f, -.5f, .5f), new Vector3(.5f, -.5f, -.5f), new Vector3(-.5f, -.5f, -.5f)};

        StartCoroutine("DelayedUpdate");
    }

    IEnumerator DelayedUpdate() {
        yield return null;
  
        int[,] terrainHeight = new int[xSize, zSize];
        RaycastHit hitInfo;
        for (int i = 0; i < xSize; i++) {
            for (int j = 0; j < zSize; j++) {
                if (Physics.Raycast(new Vector3(i + .5f, 10, j + .5f), Vector3.down, out hitInfo, 11, visionMask)) {
                    terrainHeight[i, j] = Mathf.RoundToInt(10.1f - hitInfo.distance);
                } else {
                    terrainHeight[i, j] = 0;
                }
            }
        }

        int index;
        for (int i = 0; i < xSize; i++) {
            for (int j = 0; j < zSize; j++) {
                for (int k = 0; k < ySize; k++) {
                    index = i * zSize * ySize + j * ySize + k;
                    particles[index] = new Particle(new Vector3(i + .5f, k + .5f, j + .5f), k < terrainHeight[i, j]);
                    particleHolder[index] = new ParticleSystem.Particle();
                    particleHolder[index].startSize = 2.5f;
                    particleHolder[index].startLifetime = float.MaxValue;
                    particleHolder[index].remainingLifetime = float.MaxValue;
                    particleHolder[index].startColor = k < terrainHeight[i, j] ? new Color32(0, 0, 0, 0) : new Color32(0, 0, 0, 255);
                    particleHolder[index].position = new Vector3(i + .5f, k + .5f, j + .5f);
                }
            }
        }
        fogParticleSystem.SetParticles(particleHolder, particleCount);
        yield return null;
        UpdateVision();
        fogParticleSystem.GetParticles(particleHolder, particleCount);
    }

    void Update() {
        
    }

    void LateUpdate() {
        if (fadingParticles.Count > 0) {
            List<int> finishedFading = new List<int>();
            foreach (int index in fadingParticles) {
                if (particles[index].inVision) {
                    particles[index].fadeStep = particles[index].fadeStep + 1;
                    particleHolder[index].startColor = fadeColors[particles[index].fadeStep];
                    if (particles[index].fadeStep == 16) {
                        finishedFading.Add(index);
                    }
                } else {
                    particles[index].fadeStep = particles[index].fadeStep - 1;
                    particleHolder[index].startColor = fadeColors[particles[index].fadeStep];
                    if (particles[index].fadeStep == 0) {
                        finishedFading.Add(index);
                    }
                }
            }
            foreach (int index in finishedFading) {
                fadingParticles.Remove(index);
            }
            fogParticleSystem.SetParticles(particleHolder, particleCount);
        }
        //incrementStart = (incrementStart + 1) % 2;
    }

    public void UpdateVision(GameObject movingUnit = null) {
        unitPositions.Clear();
        unitCount = UnitController.playerUnits.Count;
        foreach (GameObject unit in UnitController.playerUnits) {
            unitPositions.Add(unit.transform.position);
        }
        if (movingUnit == null) {
            RaycastHit hitInfo;
            for (int i = 0; i < xSize; i++) {
                for (int j = 0; j < zSize; j++) {
                    Physics.Raycast(new Vector3(i + .5f, 10, j + .5f), Vector3.down, out hitInfo, 11, visionMask);
                    int height = Mathf.RoundToInt(10.1f - hitInfo.distance);
                    for (int k = height; k < ySize; k++) {
                        if (particles[IndexFromPosition(new Vector3(i + .5f, k + .5f, j + .5f))].underground) {
                            particles[IndexFromPosition(new Vector3(i + .5f, k + .5f, j + .5f))].underground = false;
                            particles[IndexFromPosition(new Vector3(i + .5f, k + .5f, j + .5f))].inVision = true;
                        }
                    }
                }
            }

            UpdateDistances();
            UpdateStatus();
        } else {
            singleUnitPosition = movingUnit.transform.position + heightModifier;
            singleUnitIndex = UnitController.playerUnits.IndexOf(movingUnit);
            UpdateDistances();
            UpdateStatusMoving();
        }
        UpdateEnemyStatus();
    }

    void UpdateDistances() {
        for (int j = 0; j < unitCount; j++) {
            if (Vector3.SqrMagnitude(unitPositions[j] - previousUnitPositions[j]) > .1f) {
                Vector2 unitPositionXZ = new Vector2(unitPositions[j].x, unitPositions[j].z);
                for (int i = 0; i < particleCount; i++) {
                    particles[i].unitDistances[j] = 169 - Vector2.SqrMagnitude(particles[i].xzPos - unitPositionXZ);
                }
                previousUnitPositions[j] = unitPositions[j];
            }
            unitPositions[j] += heightModifier;
        }
    }

    void UpdateStatus() {
        bool inView;
        for (int i = 0; i < particleCount; i++) {
            inView = false;
            if (!particles[i].underground) {
                for (int j = 0; j < unitCount; j++) {
                    if (particles[i].unitDistances[j] > 0 && !Physics.Linecast(Vector3.MoveTowards(unitPositions[j], new Vector3(particles[i].xzPos.x, unitPositions[j].y, particles[i].xzPos.y), .5f), particles[i].position, visionMask)) {
                        if (!particles[i].inVision) {
                            particles[i].inVision = true;
                            fadingParticles.Add(i);
                        }
                        inView = true;
                        break;
                    }
                }
                if (!inView && particles[i].inVision == true) {
                    particles[i].inVision = false;
                    fadingParticles.Add(i);
                }
            }
        }
    }

    void UpdateStatusMoving() {
        bool inView;
        for (int i = 0; i < particleCount; i ++) {
            if (!particles[i].underground && particles[i].unitDistances[singleUnitIndex] > -10f) {
                if (!particles[i].inVision) {
                    if (particles[i].unitDistances[singleUnitIndex] > 0 && !Physics.Linecast(Vector3.MoveTowards(singleUnitPosition, new Vector3(particles[i].xzPos.x, singleUnitPosition.y, particles[i].xzPos.y), .5f), particles[i].position, visionMask)) {
                        particles[i].inVision = true;
                        fadingParticles.Add(i);
                    }
                } else {
                    inView = false;
                    for (int j = 0; j < unitCount; j++) {
                        if (particles[i].unitDistances[j] > 0 && !Physics.Linecast(Vector3.MoveTowards(unitPositions[j], new Vector3(particles[i].xzPos.x, unitPositions[j].y, particles[i].xzPos.y), .5f), particles[i].position, visionMask)) {
                            inView = true;
                            break;
                        }
                    }
                    if (!inView) {
                        particles[i].inVision = false;
                        fadingParticles.Add(i);
                    }
                }
            }
        }
    }

    public void UpdateEnemyStatus() {
        foreach (GameObject unit in EnemyController.enemyUnits) {
            Vector3 position = unit.transform.position;
            position = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y + .4f), Mathf.Round(position.z));
            unit.GetComponent<EnemyController>().HideUnit();
            foreach (Vector3 mod in enemeyVisionVectors) {
                if (particles[IndexFromPosition(position + mod)].inVision) {
                    unit.GetComponent<EnemyController>().ShowUnit();
                    break;
                }
            }
        }
    }

    int IndexFromPosition(Vector3 position) {
        return (int)((position.x - .5f) * zSize * ySize + (position.z - .5f) * ySize + (position.y - .5f));
    }
}
