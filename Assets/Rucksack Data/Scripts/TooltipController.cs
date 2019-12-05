using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipController : MonoBehaviour {

    public static GameObject itemToolTipWindow;

    void Awake() {
        itemToolTipWindow = gameObject;
        gameObject.SetActive(false);
    }

    public void OnWindowClose(Transform triggeringWindow) {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> objectsHit = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, objectsHit);
        foreach (RaycastResult hit in objectsHit) {
            if (hit.gameObject.transform == triggeringWindow) {
                itemToolTipWindow.SetActive(false);
            }
        }
    }

    public static void ManualUpdate() {
        itemToolTipWindow.SetActive(false);
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> objectsHit = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, objectsHit);
        foreach (RaycastResult hit in objectsHit) {
            hit.gameObject.GetComponent<ItemToolTip>()?.OnPointerEnter(eventData);
        }
    }
}
