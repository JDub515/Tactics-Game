using System;
using Devdog.Rucksack.Items;
using Devdog.Rucksack.CharacterEquipment;

namespace Devdog.Rucksack.Collections {

    public class CustomRestriction : ICollectionRestriction<IItemInstance> {

        public string[] allowedTypes;
        public bool fullFrame;

        public CustomRestriction(string[] allowedTypes) {
            this.allowedTypes = allowedTypes;
            fullFrame = false;
        }

        public CustomRestriction(string allowedType) {
            this.allowedTypes = new string[] { allowedType };
            fullFrame = false;
        }

        public CustomRestriction() {
            this.allowedTypes = new string[] { "Frame" };
            fullFrame = true;
        }

        public Result<bool> CanAdd(IItemInstance item, CollectionContext context) {
            if (item is UnityEquippableItemInstance equipment) {
                foreach (string type in allowedTypes) {
                    if (equipment.equipmentType.name == type) {
                        if (fullFrame) {
                            ComponentItemInstance compItem = (ComponentItemInstance)item;
                            foreach (Collection<IItemInstance> collection in compItem.components) {
                                if (collection[0] == null) {
                                    return new Result<bool>(false, Errors.CollectionRestrictionPreventedAction);
                                }
                            }
                        }
                        return true;
                    }
                }
            }
            return new Result<bool>(false, Errors.CollectionRestrictionPreventedAction);
        }

        public Result<bool> CanRemove(IItemInstance item, CollectionContext context) {
            return true;
        }
    }
}