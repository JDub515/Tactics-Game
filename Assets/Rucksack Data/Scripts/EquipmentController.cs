using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Devdog.General2.UI;
using Devdog.Rucksack.Collections;

public class EquipmentController : MonoBehaviour {

    public UIWindow equipmentWindow;
    public ItemCollectionCreator[] equipmentCollections;

    private bool equipmentOpened;

    // Start is called before the first frame update
    void Start() {
        equipmentWindow.OnShow += () => {
            equipmentOpened = true;
        };

        foreach (ItemCollectionCreator creator in equipmentCollections) {
            creator.collection.restrictions.Add(new CustomRestriction()); ;
        }
    }

    // Update is called once per frame
    void Update() {
        
    }

    void LateUpdate() {
        if (equipmentOpened) {
            for (int i = 1; i < 5; i++) {
                Transform window = equipmentWindow.transform.Find("Unit " + i + " Frames");
                if (!window.GetComponent<UIWindow>().isVisible) {
                    for (int j = 0; j < 3; j++) {
                        window.GetChild(j).gameObject.SetActive(false);
                    }
                }
            }
            equipmentOpened = false;
        }
    }
}
