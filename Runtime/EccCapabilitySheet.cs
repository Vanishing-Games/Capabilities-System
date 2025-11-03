using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace VanishingGames.ECC.Runtime
{
    [ShowOdinSerializedPropertiesInInspector]
    public class VgSerializedScriptableObject : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
        }
    }

    [CreateAssetMenu(fileName = "New EccCapabilitySheet", menuName = "ECC/Capability Sheet")]
    public class EccCapabilitySheet : VgSerializedScriptableObject
    {
        [OdinSerialize, ShowInInspector]
        internal List<EccComponent> mComponents;

        [OdinSerialize, ShowInInspector]
        internal List<EccCapability> mCapabilities;

        [OdinSerialize, ShowInInspector]
        internal List<EccCapabilitySheet> mCapabilitySheets;

#if UNITY_EDITOR
        [BoxGroup("Debug View"), ShowInInspector, PropertyOrder(1000), HideLabel]
        [InfoBox("Capabilities grouped by TickGroup. Active capabilities are shown in RED.")]
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout)]
        private Dictionary<EccTickGroup, List<CapabilityDebugInfo>> CapabilitiesGroupedByTickGroup
        {
            get
            {
                var grouped = new Dictionary<EccTickGroup, List<CapabilityDebugInfo>>();

                if (mCapabilities == null || mCapabilities.Count == 0)
                    return grouped;

                foreach (var capability in mCapabilities)
                {
                    if (capability == null)
                        continue;

                    if (!grouped.ContainsKey(capability.TickGroup))
                    {
                        grouped[capability.TickGroup] = new List<CapabilityDebugInfo>();
                    }

                    grouped[capability.TickGroup].Add(new CapabilityDebugInfo(capability));
                }

                // Sort by TickGroup
                var sortedDict = new Dictionary<EccTickGroup, List<CapabilityDebugInfo>>();
                foreach (var kvp in grouped.OrderBy(x => x.Key))
                {
                    // Sort capabilities within each group by TickOrderInGroup
                    kvp.Value.Sort((a, b) => a.TickOrder.CompareTo(b.TickOrder));
                    sortedDict[kvp.Key] = kvp.Value;
                }

                return sortedDict;
            }
        }

        [System.Serializable]
        private class CapabilityDebugInfo
        {
            [ShowInInspector, HideLabel, DisplayAsString]
            [GUIColor("GetStatusColor")]
            public string Info { get; private set; }

            [HideInInspector]
            public uint TickOrder { get; private set; }

            private bool isActive;

            public CapabilityDebugInfo(EccCapability capability)
            {
                isActive = capability.IsActive();
                TickOrder = capability.TickOrderInGroup;

                string status = isActive ? "[ACTIVE]" : "[Inactive]";
                string capabilityName = capability.GetType().Name;
                string tags =
                    capability.Tags?.Count > 0
                        ? $"Tags: [{string.Join(", ", capability.Tags)}]"
                        : "No Tags";

                Info = $"{status} {capabilityName} | Order: {TickOrder} | {tags}";
            }

            private Color GetStatusColor()
            {
                return isActive ? Color.red : Color.white;
            }
        }
#endif
    }
}
