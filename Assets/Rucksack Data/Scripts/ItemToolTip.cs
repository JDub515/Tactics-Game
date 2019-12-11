using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Devdog.Rucksack.UI;
using TMPro;
using UnityEngine.EventSystems;
using Devdog.Rucksack.Items;
using Devdog.Rucksack.Collections;

public class ItemToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private ItemCollectionSlotUI itemSlot;
    public int abilitySlot;

    // Start is called before the first frame update
    void Start() {
        itemSlot = GetComponent<ItemCollectionSlotUI>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        ComponentItemInstance itemInstance = null;
        if (itemSlot == null) {
            if (GameController.selectedUnit != null) {
                itemInstance = (ComponentItemInstance)GameController.selectedUnit.GetComponent<UnitController>().equipment[abilitySlot - 1];
            }
        } else if (itemSlot.current != null) {
            itemInstance = (ComponentItemInstance)itemSlot.current;
        }
        if (itemInstance != null) {
            ComponentItemDefinition itemDefinition = (ComponentItemDefinition)itemInstance.itemDefinition;

            TooltipController.itemToolTipWindow.SetActive(true);
            string itemStats = "";
            if (itemSlot != null) {
                TooltipController.itemToolTipWindow.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = itemDefinition.name;
                itemStats += "\n<u>Item Level " + itemInstance.stats["Item Level"] + "</u>";
                if (itemDefinition.equipmentType.name == "Frame") {
                    itemStats += ("\n" + "Augments: " + itemInstance.stats["Augments"]);
                }
                foreach (string stat in itemDefinition.stats) {
                    itemStats += ("\n" + stat + ": " + itemInstance.stats[stat]);
                }
            } else {
                TooltipController.itemToolTipWindow.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = itemDefinition.name.Remove(itemDefinition.name.LastIndexOf(" "));
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
            if (itemSlot != null) {
                TooltipController.itemToolTipWindow.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = "<i>" + itemDefinition.description + "</i>" + itemStats;
            } else {
                TooltipController.itemToolTipWindow.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = itemStats.Substring(1);
            }
            if (itemSlot == null) {
                TooltipController.itemToolTipWindow.GetComponent<RectTransform>().pivot = new Vector2(.5f, 0);
                TooltipController.itemToolTipWindow.transform.position = transform.position + Vector3.up * GetComponent<RectTransform>().sizeDelta.y * transform.lossyScale.y / 2f;
            } else if (transform.position.x < Screen.width / 2) {
                TooltipController.itemToolTipWindow.GetComponent<RectTransform>().pivot = new Vector2(0, .5f);
                TooltipController.itemToolTipWindow.transform.position = transform.position + Vector3.right * GetComponent<RectTransform>().sizeDelta.x * transform.lossyScale.x / 2f;
            } else {
                TooltipController.itemToolTipWindow.GetComponent<RectTransform>().pivot = new Vector2(1, .5f);
                TooltipController.itemToolTipWindow.transform.position = transform.position + Vector3.left * GetComponent<RectTransform>().sizeDelta.x * transform.lossyScale.x / 2f;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipController.itemToolTipWindow.SetActive(false);
    }
}
