using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Devdog.Rucksack.Items;
using Devdog.General2.UI;
using UnityEngine.UI;

namespace Devdog.Rucksack.UI {

    public class ImprovedDrag : CollectionSlotDragHandlerBase<IItemInstance> {

        public override void OnBeginDrag(PointerEventData eventData) {
            if (eventData.button == dragButton && slot.current != null) {
                if (transform.parent.gameObject.name != "Temporary Collection 1 UI") {
                    slot.collection.SwapOrMerge(slot.collectionIndex, TempCollection.tempCollections[0], 0, slot.collection.GetAmount(slot.collectionIndex));
                    TempCollection.staticTempCollecion1UI.GetComponentInChildren<ImprovedDrag>().OnBeginDrag(eventData);
                    GetComponentInParent<DynamicLayoutGroup>()?.ForceRebuildNow();
                    return;
                }

                var dragClone = GetDragObject(eventData, GetComponentInParent<Canvas>()?.rootCanvas);
                dragClone.GetComponent<Image>().enabled = false;
                dragClone.sizeDelta = dragClone.sizeDelta * new Vector2(slot.current.layoutShape.convexX, slot.current.layoutShape.convexY);
                DragAndDropUtility.BeginDrag(new DragAndDropUtility.Model(GetComponent<RectTransform>(), dragClone, slot.current), eventData);

                if (consumeEvent) {
                    eventData.Use();
                }

                if (handlePointerClick) {
                    activeCoroutine = StartCoroutine(ManualDragLoop());
                }
                WorkshopController.workshopController.UpdateBorders((UnityEquippableItemInstance)TempCollection.tempCollections[0][0]);
                TooltipController.ManualUpdate();
            }
        }

        private IEnumerator ManualDragLoop() {
            while (DragAndDropUtility.isDragging) {
                OnDrag(new PointerEventData(EventSystem.current) {
                    position = Input.mousePosition,
                });

                yield return null;
            }
        }

        public override void OnEndDrag(PointerEventData eventData) {
            if (eventData.button == dragButton && DragAndDropUtility.isDragging) {
                PointerEventData newEventData = new PointerEventData(EventSystem.current);

                RectTransform collectionWindow = null;
                foreach (GameObject hoveredObject in eventData.hovered) {
                    if (hoveredObject.GetComponent<ItemCollectionSlotUI>() != null) {
                        collectionWindow = hoveredObject.transform.parent.GetComponent<RectTransform>();
                        break;
                    }
                }
                if (collectionWindow == null) {
                    return;
                }

                float scale = 30f * DragAndDropUtility.currentDragModel.source.transform.lossyScale.x;
                IItemInstance item = (IItemInstance)DragAndDropUtility.currentDragModel.dataObject;
                Vector2 upperLeftPosition = eventData.position + (new Vector2(1 - item.layoutShape.convexX, item.layoutShape.convexY - 1)) * scale / 2f;
                Vector2 bottomRightPosition = eventData.position - (new Vector2(1 - item.layoutShape.convexX, item.layoutShape.convexY - 1)) * scale / 2f;
                Vector2 collectionUpperLeftPosition = (Vector2)collectionWindow.position + new Vector2(-1 , 1) * collectionWindow.sizeDelta * (Vector2)collectionWindow.lossyScale / 2f;
                Vector2 collectionBottomRightPosition = (Vector2)collectionWindow.position - new Vector2(-1, 1) * collectionWindow.sizeDelta * (Vector2)collectionWindow.lossyScale / 2f;
                while (upperLeftPosition.x < collectionUpperLeftPosition.x) {
                    upperLeftPosition.x += scale;
                    bottomRightPosition.x += scale;
                }
                while (bottomRightPosition.x > collectionBottomRightPosition.x) {
                    upperLeftPosition.x -= scale;
                    bottomRightPosition.x -= scale;
                }
                while (upperLeftPosition.y > collectionUpperLeftPosition.y) {
                    upperLeftPosition.y -= scale;
                    bottomRightPosition.y -= scale;
                }
                while (bottomRightPosition.y < collectionBottomRightPosition.y) {
                    upperLeftPosition.y += scale;
                    bottomRightPosition.y += scale;
                }

                if (collectionWindow.GetComponent<LayoutItemCollectionUI>() != null) {
                    Vector2 index = (collectionUpperLeftPosition - upperLeftPosition) * new Vector2(-1, 1) / scale;
                    int columnCount = collectionWindow.GetComponent<LayoutItemCollectionUI>().columnCount;
                    newEventData.hovered = new List<GameObject>() { collectionWindow.GetChild(Mathf.FloorToInt(index.x) + Mathf.FloorToInt(index.y) * columnCount).gameObject };
                } else {
                    newEventData.hovered = eventData.hovered;
                }

                HashSet<IItemInstance> coveredItems = new HashSet<IItemInstance>();
                CollectionSlotUIBase coveredItemSlot = null;
                List<RaycastResult> objectsHit = new List<RaycastResult>();
                for (int i = 0; i < item.layoutShape.convexX; i++) {
                    for (int j = 0; j < item.layoutShape.convexY; j++) {
                        newEventData.position = upperLeftPosition + new Vector2(i, -j) * scale;
                        EventSystem.current.RaycastAll(newEventData, objectsHit);
                        foreach (RaycastResult hit in objectsHit) {
                            IItemInstance coveredItem = hit.gameObject.GetComponent<ItemCollectionSlotUI>()?.current;
                            if (coveredItem != null) {
                                coveredItems.Add(coveredItem);
                                coveredItemSlot = hit.gameObject.GetComponent<ItemCollectionSlotUI>();
                            }
                        }
                    }
                }

                if (coveredItems.Count > 1) {
                    return;
                } else if (coveredItems.Count == 1) {
                    coveredItemSlot.collection.SwapOrMerge(coveredItemSlot.collectionIndex, TempCollection.tempCollections[1], 0, coveredItemSlot.collection.GetAmount(coveredItemSlot.collectionIndex));
                }

                Result<bool> success = DragAndDropUtility.EndDrag(newEventData);

                if (success.result) {
                    WorkshopController.workshopController.ResetBorders();
                    if (coveredItems.Count == 1) {
                        TempCollection.tempCollections[1].SwapOrMerge(0, TempCollection.tempCollections[0], 0, TempCollection.tempCollections[1].GetAmount(0));
                        TempCollection.staticTempCollecion1UI.GetComponentInChildren<ImprovedDrag>().OnBeginDrag(eventData);
                        WorkshopController.workshopController.UpdateBorders((UnityEquippableItemInstance)TempCollection.tempCollections[0][0]);
                    }

                } else {
                    if (coveredItems.Count == 1) {
                        TempCollection.tempCollections[1].SwapOrMerge(0, coveredItemSlot.collection, coveredItemSlot.collectionIndex, TempCollection.tempCollections[1].GetAmount(0));
                    }
                    TempCollection.staticTempCollecion1UI.GetComponentInChildren<ImprovedDrag>().OnBeginDrag(eventData);
                }

                collectionWindow.GetComponent<DynamicLayoutGroup>()?.ForceRebuildNow();

                if (activeCoroutine != null) {
                    StopCoroutine(activeCoroutine);
                }

                if (consumeEvent) {
                    eventData.Use();
                }

                TooltipController.ManualUpdate();
            }
        }
    }
}
