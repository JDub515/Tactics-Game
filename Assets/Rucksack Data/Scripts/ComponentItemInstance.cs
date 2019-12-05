using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Devdog.Rucksack.Collections;

namespace Devdog.Rucksack.Items {

    public class ComponentItemInstance : UnityEquippableItemInstance {

        public Collection<IItemInstance>[] components { get; }
        public Collection<IItemInstance>[] augments { get; }
        public Dictionary<string, float> stats;

        protected ComponentItemInstance() { }

        protected ComponentItemInstance(System.Guid ID, IUnityEquippableItemDefinition itemDefinition) : base(ID, itemDefinition) {
            ComponentItemDefinition componentItemDefinition = (ComponentItemDefinition)itemDefinition;
            string itemType = itemDefinition.equipmentType.name;
            int itemLevel = Random.Range(1, 11);
            float itemLevelFloat = itemLevel;
            stats = new Dictionary<string, float> {
                { "Item Level", itemLevel }
            };

            if (itemType == "Frame") {
                var builder = new CollectionBuilder<IItemInstance>();

                components = new Collection<IItemInstance>[componentItemDefinition.componentSlots.Length];
                for (int i = 0; i < componentItemDefinition.componentSlots.Length; i++) {

                    System.Guid guid = System.Guid.NewGuid();
                    components[i] = builder.SetLogger(logger)
                        .SetName(guid.ToString())
                        .SetSize(1)
                        .SetSlotType<CollectionSlot<IItemInstance>>()
                        .SetRestrictions(new CustomRestriction(componentItemDefinition.componentSlots[i].name))
                        .Build();
                    //CollectionRegistry.byID.Register(guid, collections[0]);
                    //CollectionRegistry.byName.Register(guid.ToString(), collections[0]);
                }

                int augmentCount = 1 + Mathf.RoundToInt(Random.Range(((itemLevel - 1f) / 5f), itemLevel / 3f));
                stats.Add("Augments", augmentCount);
                augments = new Collection<IItemInstance>[augmentCount];
                for (int i = 0; i < augmentCount; i++) {

                    System.Guid guid = System.Guid.NewGuid();
                    augments[i] = builder.SetLogger(logger)
                        .SetName(guid.ToString())
                        .SetSize(1)
                        .SetSlotType<CollectionSlot<IItemInstance>>()
                        .SetRestrictions(new CustomRestriction("Augment"))
                        .Build();
                    //CollectionRegistry.byID.Register(guid, collections[0]);
                    //CollectionRegistry.byName.Register(guid.ToString(), collections[0]);
                }
            }

            for (int i = 0; i < componentItemDefinition.stats.Length; i++) {
                float value;
                switch (componentItemDefinition.stats[i]) {
                    case "Time":
                        value = componentItemDefinition.statStartValue[i] + Random.Range(-componentItemDefinition.statIncrementValue[i], componentItemDefinition.statIncrementValue[i]);
                        break;
                    case "Bounces":
                        value = componentItemDefinition.statStartValue[i] + Random.Range(-(int)componentItemDefinition.statIncrementValue[i], (int)componentItemDefinition.statIncrementValue[i]) + 1;
                        break;
                    default:
                        value = componentItemDefinition.statStartValue[i] + componentItemDefinition.statIncrementValue[i] * Random.Range(((itemLevelFloat - 1f)/2f), itemLevelFloat);
                        break;
                }
                switch (componentItemDefinition.stats[i]) {
                    case "Shrapnel Count":
                        value = Mathf.Round(value);
                        break;
                    default:
                        value = Mathf.Round(value * 100f) / 100f;
                        break;
                }
                stats.Add(componentItemDefinition.stats[i], value);
            }
        }
    }
}
