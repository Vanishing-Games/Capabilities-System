using System.Collections;
using System.Collections.Generic;
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
    }
}
