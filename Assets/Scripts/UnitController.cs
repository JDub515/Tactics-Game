using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Devdog.Rucksack.Collections;
using Devdog.Rucksack.Items;
using TMPro;

public class UnitController : BaseUnitController {

    private string[] abilities;
    private Vector3 launchAngle;

    public GameObject visualizationPrefab;
    public GameObject ricochetLaserPrefab;
    public GameObject bouncingBombPrefab;
    public GameObject rollingBombPrefab;
    public GameObject antigravLauncherPrefab;

    public static List<GameObject> playerUnits = new List<GameObject>();

    private Collection<IItemInstance> equipment;

    private Image energyBar;

    void Awake () {
        maxHealth = 100;
        health = maxHealth;
        energy = 100;
        moveDistance = 15;
        //abilities = new string[3] { "RicochetAttack", "BouncingBomb", "RollingBomb" };

        resourceBar =  Instantiate(resourceBarsPrefab, GameObject.Find("WorldUICanvasPlayer").transform);
        healthBar = resourceBar.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        energyBar = resourceBar.transform.GetChild(1).GetChild(0).GetComponent<Image>();
        healthBar.fillAmount = 1;
        energyBar.fillAmount = 1;

        rb = GetComponent<Rigidbody>();
        playerUnits.Add(gameObject);

        equipment = (Collection<IItemInstance>)CollectionRegistry.byName.Get("Unit " + playerUnits.Count + " Frames");
        StartCoroutine("TempInvinicibility", .1f);
    }

    public void Select() {
        GetComponent<NavMeshObstacle>().enabled = false;
    }

    public void Deselect() {
        GetComponent<NavMeshObstacle>().enabled = true;
    }

    public bool SetTarget(int abilitySlot) {
        var frame = (ComponentItemInstance)equipment[abilitySlot];
        var launcherComponent = (ComponentItemInstance)frame.components[1][0];
        switch (launcherComponent.itemDefinition.name) {
            case "RicochetAttack":
                launchAngle = RicochetAttackController.CreateVisualization(gameObject, visualizationPrefab);
                break;
            case "Catapult Launcher":
                launchAngle = CatapultLauncher.CreateVisualization(gameObject, visualizationPrefab, launcherComponent.stats["Launch Power"]);
                break;
            case "Roller Launcher":
                launchAngle = RollerLauncher.CreateVisualization(gameObject, visualizationPrefab);
                break;
            case "Antigrav Launcher":
                launchAngle = AntigravLauncher.CreateVisualization(gameObject, visualizationPrefab);
                break;
        }
        return launchAngle == Vector3.zero ? false : true;
    }

    public void ExecuteAction(int abilitySlot) {
        EnergyChange(-50);
        var frame = (ComponentItemInstance)equipment[abilitySlot];
        var launcherComponent = (ComponentItemInstance)frame.components[1][0];
        GameController.activeObjects++;
        switch (launcherComponent.itemDefinition.name) {
            case "RicochetAttack":
                GameController.followObject = Instantiate(ricochetLaserPrefab, transform.position, Quaternion.LookRotation(launchAngle));
                break;
            case "Catapult Launcher":
                gameObject.layer = LayerMask.NameToLayer("Active Unit");
                GameController.followObject = Instantiate(bouncingBombPrefab, transform.position, Quaternion.LookRotation(launchAngle));
                GameController.followObject.GetComponent<CatapultLauncher>().SetVelocity(launcherComponent.stats["Launch Power"]);
                break;
            case "Roller Launcher":
                gameObject.layer = LayerMask.NameToLayer("Active Unit");
                GameController.followObject = Instantiate(rollingBombPrefab, transform.position, Quaternion.LookRotation(launchAngle));
                break;
            case "Antigrav Launcher":
                gameObject.layer = LayerMask.NameToLayer("Active Unit");
                GameController.followObject = Instantiate(antigravLauncherPrefab, transform.position, Quaternion.LookRotation(launchAngle));
                GameController.followObject.GetComponent<AntigravLauncher>().SetVelocity(launcherComponent.stats["Launch Power"]);
                break;
        }
        switch (launcherComponent.itemDefinition.name) {
            case "Catapult Launcher":
            case "Roller Launcher":
            case "Antigrav Launcher":
                GameController.followObject.GetComponent<LauncherTriggers>().SetType(((ComponentItemInstance)frame.components[2][0]).itemDefinition.name);
                GameController.followObject.GetComponent<LauncherTriggers>().SetValues(((ComponentItemInstance)frame.components[2][0]).stats);
                GameController.followObject.GetComponent<LauncherPayloads>().SetType(((ComponentItemInstance)frame.components[0][0]).itemDefinition.name);
                GameController.followObject.GetComponent<LauncherPayloads>().SetValues(((ComponentItemInstance)frame.components[0][0]).stats);
                Time.fixedDeltaTime = .004f;
                break;

        }
    }

    public void EnergyChange(int change) {
        energy += change;
        if (energy > 100) {
            energy = 100;
        }
        energyBar.fillAmount = (float)energy / 100f;
    }

    private IEnumerator OnTriggerExit(Collider collider) {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Active Object")) {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            gameObject.layer = LayerMask.NameToLayer("Unit");
        }
    }

    public float GetMoveDistance() {
        return moveDistance;
    }

    public bool HasAbility(int slot) {
        return equipment[slot] != null;
    }

    public bool HasEnergy(int ability) {
        if (energy >= 50) {
            return true;
        } else {
            GameObject energyText = Instantiate(damageTextPrefab, transform.position + 1.5f * Vector3.up, Quaternion.LookRotation(Camera.main.transform.forward));
            energyText.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 2);
            energyText.GetComponent<TextMeshPro>().SetText("Not Enough Energy!");
            return false;
        }
    }

    protected override void HandleDeath() {
        playerUnits.Remove(gameObject);
        gameObject.layer = LayerMask.NameToLayer("Active Object");
        ExplosionHelper.Explode(transform.position, 2, explosionPrefab, 1, 2);
        if (GameController.selectedUnit == gameObject) {
            GameController.selectedUnit = null;
        }
        Destroy(resourceBar);
        Destroy(gameObject);
    }
}
