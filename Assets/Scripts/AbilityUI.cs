using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityUI : MonoBehaviour {

    public GameObject abilityUIAssign;
    public GameObject[] abilitiesUIAssign;
    public GameObject confirmUIAssign;
    public GameObject endTurnUIAssign;

    public static GameObject abilityUI;
    public static GameObject[] abilitiesUI;
    public static GameObject confirmUI;
    public static GameObject endTurnUI;

    private void Start() {
        abilityUI = abilityUIAssign;
        abilitiesUI = abilitiesUIAssign;
        confirmUI = confirmUIAssign;
        endTurnUI = endTurnUIAssign;
    }

    public void HandleUI(int val) {
        GameController.gameController.HandleUI(val);
    }


}
