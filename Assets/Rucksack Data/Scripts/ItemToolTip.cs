using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Devdog.Rucksack.UI;
using TMPro;
using UnityEngine.EventSystems;
using Devdog.Rucksack.Items;
using Devdog.Rucksack.Collections;

public class ItemToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    ItemCollectionSlotUI itemSlot;

    // Start is called before the first frame update
    void Start() {
        itemSlot = GetComponent<ItemCollectionSlotUI>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (itemSlot.current != null) {
            ComponentItemInstance itemInstance = (ComponentItemInstance)itemSlot.current;
            ComponentItemDefinition itemDefinition = (ComponentItemDefinition)itemSlot.current.itemDefinition;

            TooltipController.itemToolTipWindow.SetActive(true);
            TooltipController.itemToolTipWindow.transform.position = transform.position + Vector3.left * GetComponent<RectTransform>().sizeDelta.x * transform.lossyScale.x / 2f;
            TooltipController.itemToolTipWindow.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = itemSlot.current.itemDefinition.name;
            string itemStats = "\n<u>Item Level " + itemInstance.stats["Item Level"] + "</u>";
            if (itemDefinition.equipmentType.name == "Frame" ) {
                itemStats += ("\n" + "Augments: " + itemInstance.stats["Augments"]);
            }
            foreach (string stat in itemDefinition.stats) {
                itemStats += ("\n" + stat + ": " + itemInstance.stats[stat]);
            }
            if (itemDefinition.equipmentType.name == "Frame") {
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
            }
            TooltipController.itemToolTipWindow.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = "<i>" + itemDefinition.description + "</i>" + itemStats;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipController.itemToolTipWindow.SetActive(false);
    }
}
