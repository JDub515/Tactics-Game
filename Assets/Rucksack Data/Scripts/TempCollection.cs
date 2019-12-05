using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Devdog.Rucksack.Collections;
using Devdog.Rucksack.Items;

public class TempCollection : MonoBehaviour {

    public ItemCollectionCreator CollectionCreator1;
    public ItemCollectionCreator CollectionCreator2;
    public static Collection<IItemInstance>[] tempCollections;

    public GameObject tempCollection1UI;
    public static GameObject staticTempCollecion1UI;

    void Start() {
        tempCollections = new Collection<IItemInstance>[] { CollectionCreator1.collection, CollectionCreator2.collection };
        staticTempCollecion1UI = tempCollection1UI;
    }
}
