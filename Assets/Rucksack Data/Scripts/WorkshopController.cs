using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Devdog.Rucksack.Collections;
using Devdog.Rucksack.UI;
using Devdog.Rucksack.Items;
using UnityEngine.UI;
using Devdog.General2.UI;
using TMPro;

public class WorkshopController : MonoBehaviour {
    public static WorkshopController workshopController;

    public Text frameName;
    public UIWindow workshopWindow;
    public UIWindow[] componentWindows;
    public UIWindow[] augmentWindows;
    public TextMeshProUGUI frameDescription;

    private Text[] componentNames;
    private Collection<IItemInstance> frameCollection;
    private bool workshopOpened;

    public static ItemCollectionUI staticComponentSlots;

    private Color green = new Color(.3f, 1, .3f, .5f);
    private Color red = new Color(1, .3f, .3f, .5f);
    private Color white = new Color(1, 1, 1, .5f);

    void Start() {
        workshopController = this;
        workshopOpened = false;
        frameCollection = GetComponent<ItemCollectionCreator>().collection;
        frameCollection.restrictions.Add(new CustomRestriction("Frame"));

        componentNames = new Text[componentWindows.Length];
        for (int i = 0; i < componentWindows.Length; i++) {
            componentNames[i] = componentWindows[i].transform.GetComponentInChildren<Text>(true);
        }

        frameCollection.OnAddedItem += (sender, result) => {
            string frameType = frameCollection.GetSlot(0).item.itemDefinition.name;
            frameName.text = frameType;

            foreach(UIWindow window in componentWindows) {
                window.Hide();
            }
            foreach (UIWindow window in augmentWindows) {
                window.Hide();
            }

            Collection<IItemInstance>[] components = ((ComponentItemInstance)frameCollection.GetSlot(0).item).components;
            for (int i = 0; i < components.Length; i++) {
                string name = ((ComponentItemDefinition)frameCollection.GetSlot(0).item.itemDefinition).componentSlots[i].name;
                componentNames[i].text = componentNames[i].text = name.Substring(name.IndexOf(' ') + 1); ;
                componentWindows[i].Show();
                CollectionRegistry.byName.Register("Component Slot " + i, components[i]);
            }

            Collection<IItemInstance>[] augments = ((ComponentItemInstance)frameCollection.GetSlot(0).item).augments;
            for (int i = 0; i < augments.Length; i++) {
                augmentWindows[i].Show();
                CollectionRegistry.byName.Register("Augment Slot " + i, augments[i]);
            }
        };

        frameCollection.OnRemovedItem += (sender, result) => {
            frameName.text = "Frame";
            foreach (UIWindow window in componentWindows) {
                window.Hide();
            }
            foreach (UIWindow window in augmentWindows) {
                window.Hide();
            }
        };

        workshopWindow.OnShow += () => {
            workshopOpened = true;
        };
    }
     
    void LateUpdate() {
        if (workshopOpened) {
            if (!frameCollection.GetSlot(0).isOccupied) {
                for (int i = 0; i < 3; i++) {
                    componentWindows[i].transform.Find("ItemCollectionSlotUI:0")?.gameObject.SetActive(false);
                }
                for (int i = 0; i < 4; i++) {
                    augmentWindows[i].transform.Find("ItemCollectionSlotUI:0")?.gameObject.SetActive(false);
                }
            }
            if (TempCollection.tempCollections[0][0] != null) {
                UpdateBorders((UnityEquippableItemInstance)TempCollection.tempCollections[0][0]);
            }
            workshopOpened = false;
        }
    }

    void UpdateToolTip() {
        if (frameCollection.GetSlot(0).isOccupied) {
            ComponentItemInstance itemInstance = (ComponentItemInstance)frameCollection.GetSlot(0).item;
            //ComponentItemDefinition itemDefinition = (ComponentItemDefinition)frameCollection.GetSlot(0).item.itemDefinition;

            //string itemStats = "\n<u>Item Level " + itemInstance.stats["Item Level"] + "</u>";
            //string itemStats = ("\n" + "Augments: " + itemInstance.stats["Augments"]);
            //foreach (string stat in itemDefinition.stats) {
            //    itemStats += ("\n" + stat + ": " + itemInstance.stats[stat]);
            //}
            string itemStats = "";
            foreach (Collection<IItemInstance> collection in itemInstance.components) {
                if (collection.GetSlot(0).isOccupied) {
                    itemStats += ("\n<b>" + collection.GetSlot(0).item.itemDefinition.name + "</b>");
                    foreach (string stat in ((ComponentItemDefinition)collection.GetSlot(0).item.itemDefinition).stats) {
                        itemStats += ("\n" + stat + ": " + ((ComponentItemInstance)collection.GetSlot(0).item).stats[stat]);
                    }
                }
            }
            foreach (Collection<IItemInstance> collection in itemInstance.augments) {
                if (collection.GetSlot(0).isOccupied) {
                    itemStats += ("\n<b>" + collection.GetSlot(0).item.itemDefinition.name + "</b>");
                    foreach (string stat in ((ComponentItemDefinition)collection.GetSlot(0).item.itemDefinition).stats) {
                        itemStats += ("\n" + stat + ": " + ((ComponentItemInstance)collection.GetSlot(0).item).stats[stat]);
                    }
                }
            }
            frameDescription.text = itemStats;
        } else {
            frameDescription.text = "";
        }
    }

    public void UpdateBorders(UnityEquippableItemInstance item) {
        if (item.equipmentType.name == "Frame") {
            workshopWindow.transform.Find("Frame Slot").GetComponentInChildren<Image>().color = green;
        } else if ((ComponentItemInstance)frameCollection.GetSlot(0).item != null) {
            Collection<IItemInstance>[] components = ((ComponentItemInstance)frameCollection.GetSlot(0).item).components;
            for (int i = 0; i < components.Length; i++) {
                if (((ComponentItemDefinition)frameCollection.GetSlot(0).item.itemDefinition).componentSlots[i] == item.equipmentType) {
                    componentWindows[i].GetComponentInChildren<Image>().color = green;
                } else {
                    componentWindows[i].GetComponentInChildren<Image>().color = red;
                }
            }

            Collection<IItemInstance>[] augments = ((ComponentItemInstance)frameCollection.GetSlot(0).item).augments;
            for (int i = 0; i < augments.Length; i++) {
                if (item.equipmentType.name == "augment") {
                    augmentWindows[i].GetComponentInChildren<Image>().color = green;
                } else {
                    augmentWindows[i].GetComponentInChildren<Image>().color = red;
                }
            }
        }
        UpdateToolTip();
    }

    public void ResetBorders() {
        workshopWindow.transform.Find("Frame Slot").GetComponentInChildren<Image>().color = white;

        if ((ComponentItemInstance)frameCollection.GetSlot(0).item != null) {
            Collection<IItemInstance>[] components = ((ComponentItemInstance)frameCollection.GetSlot(0).item).components;
            for (int i = 0; i < components.Length; i++) {
                componentWindows[i].GetComponentInChildren<Image>().color = white;
            }

            Collection<IItemInstance>[] augments = ((ComponentItemInstance)frameCollection.GetSlot(0).item).augments;
            for (int i = 0; i < augments.Length; i++) {
                augmentWindows[i].GetComponentInChildren<Image>().color = white;
            }
        }
        UpdateToolTip();
    }
}
