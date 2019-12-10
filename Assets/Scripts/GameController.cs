using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour {
    public static GameController gameController;

    public static int activeObjects;
    public static GameObject followObject;
    public static GameObject selectedUnit;

    public GameObject cameraContainer;
    public NavMeshSurface navMeshSurface;
    public LineRenderer line1;
    public LineRenderer line2;
    public GameObject selector;
    public GameObject ghost;
    public GameObject fogParticleSystem;

    public GameObject debrisCubePrefab;

    private GameObject abilityUI;
    private GameObject[] abilitiesUI;
    private GameObject confirmUI;
    private GameObject endTurnUI;

    private Vector3 actionTarget;
    private int selectedAction;
    private List<Vector3> corners;

    private Camera mainCamera;
    private Vector3 cameraVelocity;

    private GameObject[] debrisList;
    private int preloadCount;

    private FogParticleController fogParticleController;

    private enum States {
        NothingSelected,
        UnitSelected,
        ActionSelected,
        TargetSelected,
        ActionInProgress,
        TemporarySuspend,
        EnemyTurn
    }

    private States playerState;
    private States previousState;
    private bool inputUsed;
    private int uiPressed;

    public GameObject playerUnitPrefab;
    public GameObject enemyUnitPrefab;

    private GameObject warpPad;

    // Use this for initialization
    void Start() {
        gameController = this;
        activeObjects = 0;
        selectedAction = -1;
        uiPressed = -1;
        abilityUI = AbilityUI.abilityUI;
        abilitiesUI = AbilityUI.abilitiesUI;
        confirmUI = AbilityUI.confirmUI;
        endTurnUI = AbilityUI.endTurnUI;
        mainCamera = Camera.main;

        playerState = States.NothingSelected;
        Vector3 startLocation = GetComponent<LevelGenerator>().GenerateLevel(6, 6);
        warpPad = GameObject.Find("Warp Pad(Clone)");
        navMeshSurface.BuildNavMesh();
        preloadCount = 0;
        debrisList = new GameObject[40];
        fogParticleController = fogParticleSystem.GetComponent<FogParticleController>();

        CameraSnapTo(startLocation + Vector3.up);
        GameObject.Instantiate(playerUnitPrefab, startLocation + Vector3.up + Vector3.left * 3, Quaternion.identity);
        GameObject.Instantiate(playerUnitPrefab, startLocation + Vector3.up + Vector3.right * 3, Quaternion.identity);

        RaycastHit hit;
        Vector3 randomLoc;
        for (int i = 0; i < 20; i++) {
            do {
                randomLoc = new Vector3(Random.Range(3, 63), 10, Random.Range(3, 63));
            } while (Vector3.Distance(startLocation, randomLoc) < 20);
            Physics.Raycast(randomLoc, Vector3.down, out hit, 11, ~LayerMask.GetMask("Ignore Raycast"));
            GameObject.Instantiate(enemyUnitPrefab, hit.point + Vector3.up, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update() {
        switch (playerState) {
            case States.NothingSelected:
                UnitSelect();
                EndTurn();
                MoveCamera();
                PreLoadDebris();
                break;
            case States.UnitSelected:
                UnitSelect();
                ActionSelect();
                ResetAction();
                EndTurn();
                CameraFollow();
                MoveCamera();
                PreLoadDebris();
                break;
            case States.ActionSelected:
                ActionSelect();
                TargetSelect();
                ResetAction();
                MoveCamera();
                PreLoadDebris();
                break;
            case States.TargetSelected:
                ExecuteAction();
                ResetAction();
                MoveCamera();
                PreLoadDebris();
                break;
            case States.ActionInProgress:
                CameraFollow();
                if (activeObjects == 0) {
                    if (selectedAction == 0) {
                        fogParticleController.UpdateVision(selectedUnit);
                    } else {
                        RebuildNavMesh();
                        fogParticleController.UpdateVision();
                        followObject = selectedUnit;
                    }
                    if (selectedUnit == null) {
                        playerState = States.NothingSelected;
                    } else {
                        abilityUI.SetActive(true);
                        playerState = States.UnitSelected;
                    }
                    endTurnUI.SetActive(true);
                } else if (selectedAction == 0) {
                    DrawPath();
                    fogParticleController.UpdateVision(selectedUnit);
                }
                break;
            case States.TemporarySuspend:
                break;
            case States.EnemyTurn:
                CameraFollow();
                break;
        }

        if (selectedUnit != null) {
            if (!selector.activeSelf) {
                selector.SetActive(true);
            }
            selector.transform.position = selectedUnit.transform.position - new Vector3(0, .99f, 0);
        } else {
            if (selector.activeSelf) {
                selector.SetActive(false);
            }
        }
        inputUsed = false;
        uiPressed = -1;
    }

    public void HandleUI(int index) {
        uiPressed = index;
    }

    void UnitSelect() {
        if (inputUsed) { return; }
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            followObject = null;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 60, ~LayerMask.GetMask("Ignore Raycast"))) {
                if (hit.transform.CompareTag("Unit")) {
                    if (selectedUnit != null) {
                        selectedUnit.GetComponent<UnitController>().Deselect();
                    }
                    selectedUnit = hit.transform.gameObject;
                    selectedUnit.GetComponent<UnitController>().Select();
                    inputUsed = true;
                    abilityUI.SetActive(true);
                    for (int i = 0; i < 3; i++) {
                        if (selectedUnit.GetComponent<UnitController>().HasAbility(i)) {
                            abilitiesUI[i].SetActive(true);
                        } else {
                            abilitiesUI[i].SetActive(false);
                        }
                    }
                    confirmUI.SetActive(false);
                    playerState = States.UnitSelected;
                }
            }
        }
    }

    void EndTurn() {
        if (uiPressed == 6) {
            if (playerState == States.UnitSelected) {
                selectedUnit.GetComponent<UnitController>().Deselect();
                abilityUI.SetActive(false);
                selectedUnit = null;
            }
            abilityUI.SetActive(false);
            confirmUI.SetActive(false);
            endTurnUI.SetActive(false);
            playerState = States.EnemyTurn;
            foreach (GameObject unit in UnitController.playerUnits) {
                if (!unit.GetComponent<CapsuleCollider>().bounds.Intersects(warpPad.transform.GetChild(0).GetChild(0).GetComponent<BoxCollider>().bounds)) {
                    StartCoroutine("EnemyTurn");
                    return;
                }
            }
            StartCoroutine("NextLevel");
        }
    }

    void ActionSelect() {
        if (inputUsed) { return; }
        UnitController unitCont = selectedUnit.GetComponent<UnitController>();
        if (Input.GetKeyDown("1") || uiPressed == 0) {
            if (!unitCont.HasEnergy(0)) { return; }
            resetActionTarget();
            selectedAction = 0;
            inputUsed = true;
        } else if ((Input.GetKeyDown("2") || uiPressed == 1) && unitCont.HasAbility(0)) {
            if (!unitCont.HasEnergy(1)) { return; }
            resetActionTarget();
            selectedAction = 1;
            inputUsed = true;
        } else if ((Input.GetKeyDown("3") || uiPressed == 2) && unitCont.HasAbility(1)) {
            if (!unitCont.HasEnergy(2)) { return; }
            resetActionTarget();
            selectedAction = 2;
            inputUsed = true;
        } else if ((Input.GetKeyDown("4") || uiPressed == 3) && unitCont.HasAbility(2)) {
            if (!unitCont.HasEnergy(3)) { return; }
            resetActionTarget();
            selectedAction = 3;
            inputUsed = true;
        }
        if (inputUsed) {
            followObject = null;
            playerState = States.ActionSelected;
            endTurnUI.SetActive(false);
        }
    }

    void TargetSelect() {
        if (inputUsed) { return; }
        resetActionTarget();
        bool validTarget = false;
        if (selectedAction == 0) {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 60, ~LayerMask.GetMask(new string[3] { "Unit", "Unit Trigger", "Ignore Raycast" }))) {
                NavMeshPath path = new NavMeshPath();
                bool foundPath = true;
                if (!NavMesh.CalculatePath(selectedUnit.transform.position - Vector3.down, hit.point, NavMesh.AllAreas, path)) {
                    NavMeshHit navMeshHit;
                    NavMeshHit navMeshHit2;
                    if (NavMesh.SamplePosition(hit.point, out navMeshHit, 5, NavMesh.AllAreas) && NavMesh.SamplePosition(selectedUnit.transform.position - Vector3.down, out navMeshHit2, 5, NavMesh.AllAreas)) {
                        if (!NavMesh.CalculatePath(navMeshHit2.position, navMeshHit.position, NavMesh.AllAreas, path)) {
                            foundPath = false;
                        }
                    }
                }
                if (foundPath && path.corners.Length >= 2) {
                    corners = new List<Vector3>(path.corners);
                    OptomizePath();
                    OptomizePath();
                    AddPathHeight();
                    DrawPath();
                    validTarget = true;
                }
            }
        } else {
            validTarget = selectedUnit.GetComponent<UnitController>().SetTarget(selectedAction - 1);
        }
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && validTarget) {
            inputUsed = true;
            abilityUI.SetActive(false);
            confirmUI.SetActive(true);
            playerState = States.TargetSelected;
        }
    }

    void ExecuteAction() {
        if (inputUsed) { return; }
        if (Input.GetKeyDown("space") || uiPressed == 4) {
            if (selectedAction == 0) {
                selectedUnit.GetComponent<UnitController>().EnergyChange(-50);
                followObject = selectedUnit;
                StartCoroutine("MoveUnit");
            } else {
                selectedUnit.GetComponent<UnitController>().ExecuteAction(selectedAction - 1);
                resetActionTarget();
            }
            inputUsed = true;
            confirmUI.SetActive(false);
            playerState = States.ActionInProgress;
        }
    }

    void ResetAction() {
        if (inputUsed) { return; }
        if (Input.GetKeyDown("escape") || uiPressed == 5) {
            if (playerState == States.UnitSelected) {
                selectedUnit.GetComponent<UnitController>().Deselect();
                selectedUnit = null;
                abilityUI.SetActive(false);
                playerState = States.NothingSelected;
            } else if (playerState == States.ActionSelected) {
                resetActionTarget();
                endTurnUI.SetActive(true);
                playerState = States.UnitSelected;
            } else if (playerState == States.TargetSelected) {
                abilityUI.SetActive(true);
                confirmUI.SetActive(false);
                playerState = States.ActionSelected;
            }
            inputUsed = true;
        }
    }

    void MoveCamera() {
        if (Input.GetKeyDown("q")) {
            cameraVelocity = Vector3.zero;
            previousState = playerState;
            playerState = States.TemporarySuspend;
            StartCoroutine("RotateCamera", 1);
            inputUsed = true;
            return;
        }
        if (Input.GetKeyDown("e")) {
            cameraVelocity = Vector3.zero;
            previousState = playerState;
            playerState = States.TemporarySuspend;
            StartCoroutine("RotateCamera", -1);
            inputUsed = true;
            return;
        }

        Vector3 acceleration = Vector3.zero;
        bool withinWindow = false;
        if (Input.mousePosition.y >= 0 && Input.mousePosition.y < Screen.height && Input.mousePosition.x >=0 && Input.mousePosition.x < Screen.width) {
            withinWindow = true;
        }
        if (Input.GetKey("w") || ((Input.mousePosition.y == Screen.height - 1 || Input.mousePosition.y == Screen.height - 2) && withinWindow)) {
            acceleration += cameraContainer.transform.forward;
        }
        if (Input.GetKey("a") || ((Input.mousePosition.x == 0 || Input.mousePosition.x == 1) && withinWindow)) {
            acceleration += cameraContainer.transform.right * -1;
        }
        if (Input.GetKey("s") || ((Input.mousePosition.y == 0 || Input.mousePosition.y == 1) && withinWindow)) {
            acceleration += cameraContainer.transform.forward * -1;
        }
        if (Input.GetKey("d") || ((Input.mousePosition.x == Screen.width - 1 || Input.mousePosition.x == Screen.width - 2) && withinWindow)) {
            acceleration += cameraContainer.transform.right;
        }
        if (acceleration != Vector3.zero) {
            followObject = null;
            cameraVelocity += 75 * Time.smoothDeltaTime * acceleration.normalized;
            if (cameraVelocity.magnitude > 20) {
                cameraVelocity = cameraVelocity.normalized * 20;
            }
        } else if (followObject != null) {
            return;
        } else {
            if (cameraVelocity.magnitude > 75 * Time.smoothDeltaTime) {
                cameraVelocity -= 75 * Time.smoothDeltaTime * cameraVelocity.normalized;
            } else {
                cameraVelocity = Vector3.zero;
            }
        }
        cameraContainer.transform.position += cameraVelocity * Time.smoothDeltaTime;
    }

    void CameraFollow() {
        if (followObject != null) {
            Vector3 finalPosition = followObject.transform.position - mainCamera.transform.forward * (cameraContainer.transform.position.y - followObject.transform.position.y) / -mainCamera.transform.forward.y;
            cameraContainer.transform.position = Vector3.SmoothDamp(cameraContainer.transform.position, finalPosition, ref cameraVelocity, .5f);
        }
    }

    void CameraSnapTo(Vector3 target) {
        cameraContainer.transform.position = target - mainCamera.transform.forward * (cameraContainer.transform.position.y - target.y) / -mainCamera.transform.forward.y;
    }

    void resetActionTarget() {
        ghost.SetActive(false);
        line1.positionCount = 0;
        line2.positionCount = 0;
        VisualizationHelper.ResetVisualization();
    }

    void DrawPath() {
        float maxDistance = selectedUnit.GetComponent<UnitController>().GetMoveDistance();
        float travelDist = 0;
        Vector3 prevPos = corners[0];
        bool movementExhausted = false;
        List<Vector3> modifiedPath1 = new List<Vector3>();
        List<Vector3> modifiedPath2 = new List<Vector3>();
        line1.positionCount = 0;
        line2.positionCount = 0;
        for (int i = 0; i < corners.Count; i++) {
            if (travelDist + Vector3.Distance(prevPos, corners[i]) < maxDistance) {
                line1.positionCount += 1;
                modifiedPath1.Add(corners[i] + new Vector3(0, 1, 0));
                actionTarget = corners[i];
            } else if (!movementExhausted) {
                line1.positionCount += 1;
                line2.positionCount += 2;
                actionTarget = Vector3.Lerp(prevPos, corners[i], (maxDistance - travelDist) / Vector3.Distance(prevPos, corners[i]));
                modifiedPath1.Add(actionTarget + new Vector3(0, 1, 0));
                modifiedPath2.Add(actionTarget + new Vector3(0, .99f, 0));
                modifiedPath2.Add(corners[i] + new Vector3(0, 1, 0));
                movementExhausted = true;
            } else {
                line2.positionCount += 1;
                modifiedPath2.Add(corners[i] + new Vector3(0, 1, 0));
            }

            travelDist += Vector3.Distance(prevPos, corners[i]);
            prevPos = corners[i];
        }

        line1.SetPositions(modifiedPath1.ToArray());
        line2.SetPositions(modifiedPath2.ToArray());
        //Debug.Log(travelDist);

        ghost.SetActive(true);
        ghost.transform.position = modifiedPath1[line1.positionCount - 1];
        if (line2.positionCount > 0) {
            corners.RemoveRange(line1.positionCount - 1, line2.positionCount - 1);
            corners.Add(actionTarget);
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
        corners[corners.Count - 1] += Vector3.down * (hit.distance - .51f);
    }

    void PreLoadDebris() {
        if (preloadCount < 40) {
            debrisList[preloadCount] = Instantiate(debrisCubePrefab, new Vector3(10 + 1.1f * preloadCount, -2, 10), Quaternion.identity);
            preloadCount++;
        }
    }

    public GameObject GetDebris() {
        if (preloadCount > 0) {
            preloadCount--;
            return debrisList[preloadCount];
        } else {
            return Instantiate(debrisCubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }

    void RebuildNavMesh() {
        StartCoroutine("DelayedRebuild");
    }

    IEnumerator DelayedRebuild() {
        yield return null;
        navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
    }

    IEnumerator MoveUnit() {
        activeObjects++;
        Rigidbody rb = selectedUnit.GetComponent<Rigidbody>();
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
            yield return new WaitForFixedUpdate();
        }
        rb.MovePosition(corners[1] + Vector3.up);
        yield return new WaitForFixedUpdate();
        resetActionTarget();
        activeObjects--;
        rb.velocity = Vector3.zero;
        rb.isKinematic = false;
        selectedUnit.GetComponent<UnitController>().StartCoroutine("TempInvinicibility", .5f);
    }

    IEnumerator RotateCamera(int direction) {
        float unitHeight = selectedUnit != null ? selectedUnit.transform.position.y : 1;
        Vector3 point = cameraContainer.transform.position + mainCamera.transform.forward * (cameraContainer.transform.position.y - unitHeight) / -mainCamera.transform.forward.y;
        float rotationDistance = 90;
        while (rotationDistance > 0) {
            if (rotationDistance - 180 * Time.deltaTime > 0) {
                cameraContainer.transform.RotateAround(point, Vector3.up, 180 * Time.deltaTime * direction);
                rotationDistance -= 180 * Time.deltaTime;
            } else {
                cameraContainer.transform.RotateAround(point, Vector3.up, rotationDistance * direction);
                rotationDistance = 0;
            }
            yield return null;
        }
        playerState = previousState;
    }

    IEnumerator EnemyTurn() {
        GameObject[] enemies = EnemyController.enemyUnits.ToArray();
        foreach (GameObject enemy in enemies) {
            if (enemy != null) {
                EnemyController.enemyActive = true;
                enemy.GetComponent<EnemyController>().StartCoroutine("TakeTurn");
                while (EnemyController.enemyActive) {
                    yield return null;
                }
                fogParticleController.UpdateVision();
            }
        }
        foreach (GameObject unit in UnitController.playerUnits) {
            unit.GetComponent<UnitController>().EnergyChange(100);
        }
        selectedUnit = null;
        endTurnUI.SetActive(true);
        playerState = States.NothingSelected;
    }

    IEnumerator NextLevel() {
        followObject = warpPad;
        Rigidbody rb = warpPad.GetComponentInChildren<Rigidbody>();
        foreach (GameObject unit in UnitController.playerUnits) {
            unit.GetComponent<UnitController>().StartCoroutine("TempInvinicibility", 3f);
        }
        while (warpPad.transform.position.y < 40) {
            rb.MovePosition(rb.position + 3 * Vector3.up * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
    }
}
