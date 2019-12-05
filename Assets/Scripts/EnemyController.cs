using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
//using TMPro;

public class EnemyController : BaseUnitController {

    private float maxEngagementDistance;
    private float minEngagementDistance;
    private string[] abilities;
    private bool inVision;

    private static LayerMask visionMask = -1;

    private List<Vector3> corners;
    private HashSet<Vector3> knownPlayerPositions;

    public GameObject basicProjectilePrefab;

    public static List<GameObject> enemyUnits = new List<GameObject>();
    public static bool enemyActive = false;

    // Start is called before the first frame update
    void Awake() {
        maxHealth = 100;
        health = maxHealth;
        moveDistance = 10;
        visionDistance = 13;
        maxEngagementDistance = 8;
        minEngagementDistance = 2;
        inVision = true;
        knownPlayerPositions = new HashSet<Vector3>();
        rb = GetComponent<Rigidbody>();

        resourceBar = Instantiate(resourceBarsPrefab, GameObject.Find("WorldUICanvas").transform);
        healthBar = resourceBar.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        healthBar.fillAmount = 1;

        if (visionMask == -1) {
            visionMask = LayerMask.GetMask(new string[3] { "Swapable Object", "Debris", "Indestructable Terrain" });
        }

        enemyUnits.Add(gameObject);
        StartCoroutine("TempInvinicibility", .1f);
    }

    IEnumerator TakeTurn() {
        GameController.selectedUnit = gameObject;
        float distance;
        Vector3 target = Vector3.zero;
        energy = 100;
        bool usedMove = false;
        GetComponent<NavMeshObstacle>().enabled = false;
        yield return null;
        while (energy > 0) {
            SearchForPlayer();
            distance = 1000;
            if (knownPlayerPositions.Count > 0) {
                foreach (Vector3 unitPosition in knownPlayerPositions) {
                    if (Vector3.Distance(unitPosition, transform.position) < distance) {
                        target = unitPosition;
                        distance = Vector3.Distance(target, transform.position);
                    }
                }
            } else {
                break;
            }
          
            if (!usedMove && (maxEngagementDistance < distance || distance < minEngagementDistance)) {
                energy -= 50;
                GameController.followObject = gameObject;
                usedMove = true;
                if (distance < minEngagementDistance) {
                    if (SetUpMove(transform.position + (transform.position - target).normalized)) {
                        yield return StartCoroutine("MoveUnit");
                    }
                } else if (SetUpMove(target)) {
                    yield return StartCoroutine("MoveUnit");
                }
            } else {
                energy -= 100;
                GameController.activeObjects++;
                gameObject.layer = LayerMask.NameToLayer("Active Unit");
                GameController.followObject = Instantiate(basicProjectilePrefab, transform.position + Vector3.up * .5f, Quaternion.LookRotation(target - transform.position));
                while (GameController.activeObjects > 0) {
                    yield return null;
                }
            }
        }
        GetComponent<NavMeshObstacle>().enabled = true;
        enemyActive = false;
    }

    void SearchForPlayer() {
        knownPlayerPositions.Clear();
        Vector3 enemyPosition = transform.position;
        foreach (GameObject unit in UnitController.playerUnits) {
            Vector3 unitPosition = unit.transform.position;
            if (visionDistance - Vector2.Distance(new Vector2(unitPosition.x, unitPosition.z), new Vector2(enemyPosition.x, enemyPosition.z)) > 0) {
                if (!Physics.Linecast(enemyPosition + Vector3.up *.5f, unitPosition + Vector3.up * .5f, visionMask) || !Physics.Linecast(enemyPosition + Vector3.up * .5f, unitPosition - Vector3.up * .5f, visionMask)) {
                    knownPlayerPositions.Add(unitPosition);
                }
            }
        }
    }

    bool SetUpMove(Vector3 target) {
        NavMeshPath path = new NavMeshPath();
        bool foundPath = true;
        if (!NavMesh.CalculatePath(transform.position - Vector3.down, target - Vector3.down, NavMesh.AllAreas, path)) {
            NavMeshHit navMeshHit;
            NavMeshHit navMeshHit2;
            if (NavMesh.SamplePosition(target - Vector3.down, out navMeshHit, 5, NavMesh.AllAreas) && NavMesh.SamplePosition(transform.position - Vector3.down, out navMeshHit2, 5, NavMesh.AllAreas)) {
                if (!NavMesh.CalculatePath(navMeshHit2.position, navMeshHit.position, NavMesh.AllAreas, path)) {
                    foundPath = false;
                }
            }
        }
        if (foundPath && path.corners.Length >= 2) {
            corners = new List<Vector3>(path.corners);
            OptomizePath();
            AddPathHeight();
            ShortenPath();
            return true;
        } else {
            return false;
        }
    }

    void OptomizePath() {
        NavMeshHit hit;
        int i = 1;
        while (i + 1 < corners.Count) {
            int ii = i - 1;
            while (ii <= i && ii + 2 < corners.Count) {
                bool blocked = NavMesh.Raycast(corners[ii], corners[ii + 2], out hit, NavMesh.AllAreas);
                if (!blocked) {
                    corners.RemoveAt(ii + 1);
                    if (ii > 0) {
                        ii--;
                    }
                } else {
                    ii++;
                }
            }
            if (i + 1 < corners.Count && Vector3.Distance(corners[i], corners[i + 1]) < .001f) {
                corners.RemoveAt(i + 1);
            }
            if (i + 1 < corners.Count && Vector3.Distance(corners[i], corners[i - 1]) > .01f && Vector3.Distance(corners[i], corners[i + 1]) > .01f) {
                Vector3 newPoint1, newPoint2;
                float increment = 0;
                do {
                    increment += .01f;
                    newPoint1 = Vector3.Lerp(corners[i], corners[i - 1], increment);
                    newPoint2 = Vector3.Lerp(corners[i], corners[i + 1], increment);
                } while (!NavMesh.Raycast(newPoint1, newPoint2, out hit, NavMesh.AllAreas) && increment < 1);
                if (increment > .015f) {
                    increment -= .01f;
                    corners.Insert(i, Vector3.Lerp(corners[i], corners[i - 1], increment));
                    corners[i + 1] = Vector3.Lerp(corners[i + 1], corners[i + 2], increment);
                }
            }
            i++;
            if (i > 1000) {
                Debug.Log("INFINITE LOOP!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                break;
            }
        }
    }

    void AddPathHeight() {
        int i = 0;
        Ray ray;
        float previousHeight;
        RaycastHit hit;
        while (i < corners.Count - 1) {
            Vector3 firstPoint = corners[i];
            Vector3 secondPoint = corners[i + 1];
            previousHeight = firstPoint.y;
            for (float ii = 0; ii < 1; ii += .2f / Vector3.Distance(firstPoint, secondPoint)) {
                ray = new Ray(new Vector3(Vector3.Lerp(firstPoint, secondPoint, ii).x, previousHeight + 1, Vector3.Lerp(firstPoint, secondPoint, ii).z), Vector3.down);
                Physics.SphereCast(ray, .49f, out hit, 2, LayerMask.GetMask(new string[3] { "Debris", "Swapable Object", "Indestructable Terrain" }));
                if (ii == 0) {
                    corners[i] = ray.origin + Vector3.down * (hit.distance + .49f);
                } else {
                    corners.Insert(i + 1, ray.origin + Vector3.down * (hit.distance + .49f));
                    i++;
                }
                previousHeight = hit.point.y;
            }
            i++;
        }
        Physics.SphereCast(corners[corners.Count - 1] + Vector3.up, .49f, Vector3.down, out hit, 2, LayerMask.GetMask(new string[3] { "Debris", "Swapable Object", "Indestructable Terrain" }));
        corners[corners.Count - 1] += Vector3.down * (hit.distance - .49f);
    }

    void ShortenPath() {
        float travelDist = 0;
        Vector3 prevPos = corners[0];
        for (int i = 1; i < corners.Count; i++) {
            if (travelDist + Vector3.Distance(prevPos, corners[i]) > moveDistance) {
                corners.Insert(i, Vector3.Lerp(prevPos, corners[i], (moveDistance - travelDist) / Vector3.Distance(prevPos, corners[i])));
                corners.RemoveRange(i + 1, corners.Count - (i + 1));
                continue;
            }

            travelDist += Vector3.Distance(prevPos, corners[i]);
            prevPos = corners[i];
        }

    }

    IEnumerator MoveUnit() {
        rb.isKinematic = true;
        float distance;
        while (true) {
            corners[0] = rb.position + Vector3.down;
            distance = 5 * Time.fixedDeltaTime;
            distance -= (corners[1] - corners[0]).magnitude;
            if (distance > 0) {
                if (corners.Count > 2) {
                    corners.RemoveAt(1);
                } else {
                    break;
                }
            }
            rb.MovePosition(rb.position + 5 * (corners[1] - corners[0]).normalized * Time.fixedDeltaTime);
            FogParticleController.fogParticleController.UpdateEnemyStatus();
            yield return new WaitForFixedUpdate();
            SearchForPlayer();
            if (knownPlayerPositions.Count > 0) {
                float dist = 1000;
                Vector3 target = Vector3.zero;
                foreach (Vector3 unitPosition in knownPlayerPositions) {
                    if (Vector3.Distance(unitPosition, transform.position) < dist) {
                        target = unitPosition;
                        dist = Vector3.Distance(unitPosition, transform.position);
                    }
                }
                if (dist < maxEngagementDistance && dist > minEngagementDistance) {
                    rb.velocity = Vector3.zero;
                    rb.isKinematic = false;
                    StartCoroutine("TempInvinicibility", .5f);
                    yield break;
                }
            }
        }
        rb.MovePosition(corners[1] + Vector3.up);
        yield return new WaitForFixedUpdate();
        rb.velocity = Vector3.zero;
        rb.isKinematic = false;
        StartCoroutine("TempInvinicibility", .5f);
    }

    private IEnumerator OnTriggerExit(Collider collider) {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Active Object")) {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            gameObject.layer = LayerMask.NameToLayer("Unit");
        }
    }

    public void HideUnit() {
        if (inVision) {
            inVision = false;
            GetComponent<MeshRenderer>().enabled = false;
            resourceBar.SetActive(false);
        }
    }

    public void ShowUnit() {
        if (!inVision) {
            inVision = true;
            GetComponent<MeshRenderer>().enabled = true;
            resourceBar.SetActive(true);
        }
    }

    protected override void HandleDeath() {
        enemyUnits.Remove(gameObject);
        gameObject.layer = LayerMask.NameToLayer("Active Object");
        ExplosionHelper.Explode(transform.position, 2, explosionPrefab, 1, 2);
        StopAllCoroutines();
        enemyActive = false;
        Destroy(resourceBar);
        Destroy(gameObject);
    }
}
