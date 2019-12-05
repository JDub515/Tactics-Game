using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherPayloads : MonoBehaviour {
    private string payloadType;

    public GameObject bigExplosionPrefab;
    public GameObject smallExplosionPrefab;
    public GameObject tinyExplosionPrefab;
    public GameObject shrapnelPrefab;

    private Dictionary<string, float> values;

    void Start() {
        
    }

    void Update() {
        
    }

    public void SetType(string name) {
        payloadType = name;
    }

    public void SetValues(Dictionary<string, float> stats) {
        values = stats;
    }

    public void Triggered() {
        switch (payloadType) {
            case "Bomb Payload":
                ExplosionHelper.Explode(transform.position, values["Power"], bigExplosionPrefab, 1, values["Falloff"]);
                break;
            case "Vacuum Bomb Payload":
                ExplosionHelper.Explode(transform.position, values["Power"], smallExplosionPrefab, -1, values["Falloff"]);
                break;
            case "Shrapnel Payload":
                GameObject.Instantiate(tinyExplosionPrefab, transform.position, Quaternion.identity);
                Time.fixedDeltaTime = .02f;
                GameObject temp;
                for (int i = 0; i < values["Shrapnel Count"]; i++) {
                    GameController.activeObjects++;
                    temp = GameObject.Instantiate(shrapnelPrefab, transform.position, Quaternion.identity);
                    temp.GetComponent<Rigidbody>().mass = values["Power"];
                }
                break;
        }
        GameController.activeObjects--;
        Destroy(gameObject);
    }
}
