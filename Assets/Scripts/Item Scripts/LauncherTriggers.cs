using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherTriggers : MonoBehaviour {

    private string triggerType;

    private float timer;

    private int bounces;
    private bool bounceTriggered;

    private Dictionary<string,float> values;

    void Start() {
        timer = 0;
        bounces = 0;
        bounceTriggered = false;
    }

    public void SetType(string name) {
        triggerType = name;
    }

    public void SetValues(Dictionary<string,float> stats) {
        values = stats;
    }

    void FixedUpdate() {
        switch (triggerType) {
            case "Time Trigger":
            case "Unit Collision Trigger":
            case "Enemy Collision Trigger":
            case "Bounce Trigger":
                timer += Time.deltaTime;
                if (timer > values["Time"]) {
                    TriggerPayload();
                }
                break;
            case "Remote Trigger":
                timer += Time.deltaTime;
                if (timer > values["Time"]) {
                    TriggerPayload();
                } else if (Input.GetKeyDown("space")) {
                    TriggerPayload();
                }
                break;
        }
    }

    void OnCollisionEnter(Collision collision) {
        switch (triggerType) {
            case "Bounce Trigger":
                if (!bounceTriggered) {
                    bounces++;
                    if (bounces > values["Bounces"]) {
                        TriggerPayload();
                    }
                    bounceTriggered = true;
                }
                break;
            case "Unit Collision Trigger":
                if (collision.gameObject.layer == LayerMask.NameToLayer("Unit")) {
                    TriggerPayload();
                }
                break;
            case "Enemy Collision Trigger":
                if (collision.gameObject.layer == LayerMask.NameToLayer("Unit") && collision.gameObject.GetComponent<UnitController>() == null) {
                    TriggerPayload();
                }
                break;
            case "Collision Trigger":
                TriggerPayload();
                break;
        }
    }

    void LateUpdate() {
        if (triggerType == "Bounce Trigger") {
            bounceTriggered = false;
        }
    }

    void TriggerPayload() {
        GetComponent<LauncherPayloads>().Triggered();
    }
}
