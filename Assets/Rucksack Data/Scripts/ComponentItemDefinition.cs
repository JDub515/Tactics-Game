using UnityEngine;
using Devdog.Rucksack.CharacterEquipment;

namespace Devdog.Rucksack.Items {

    public class ComponentItemDefinition : UnityEquippableItemDefinition {

        [SerializeField]
        private string[] _stats;
        public string[] stats {
            get { return this.GetValue(o => o._stats); }
        }

        [SerializeField]
        private float[] _statStartValue;
        public float[] statStartValue {
            get { return this.GetValue(o => o._statStartValue); }
        }

        [SerializeField]
        private float[] _statIncrementValue;
        public float[] statIncrementValue {
            get { return this.GetValue(o => o._statIncrementValue); }
        }

        [SerializeField]
        private UnityEquipmentType[] _componentSlots;
        public IEquipmentType[] componentSlots {
            get { return this.GetValue(o => o._componentSlots); }
        }
    }
}