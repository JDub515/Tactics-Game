using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Devdog.Rucksack.Collections;
using Devdog.Rucksack.Items;

public class ItemTest : MonoBehaviour {

    public ComponentItemDefinition[] items;

    // Start is called before the first frame update
    void Start() {
        var c = GetComponent<LayoutItemCollectionCreator>();
        ItemFactory.Bind<ComponentItemDefinition, ComponentItemInstance>();

        foreach (ComponentItemDefinition item in items) {
            var inst = ItemFactory.CreateInstance(item, System.Guid.NewGuid());
            c.collection.Add(inst, 1);
        }
    }

    // Update is called once per frame
    void Update() {
        
    }
}
