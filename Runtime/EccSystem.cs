using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace VanishingGames.ECC.Runtime
{
    public class EccSystem : SerializedMonoBehaviour
    {
        public EccComponent GetEccComponent<T>()
        {
            return mRuntimeComponents.FirstOrDefault(c => c is T);
        }

        private void Awake()
        {
            InitDefaultSheets();
            InitBlockers();

            PrintInitStatus();
        }

        private void Update()
        {
            foreach (var tickGroup in mTickGroups)
            {
                foreach (var capability in tickGroup.Value)
                {
                    if (capability.IsActive())
                        capability.ShouldDeactivate();
                    else
                        capability.ShouldActivate();

                    if (capability.IsActive())
                        capability.OnTickActive(Time.deltaTime);
                }
            }
        }

        private void InitDefaultSheets()
        {
            if (mDefaultSheets == null)
                return;

            foreach (var sheet in mDefaultSheets)
            {
                PushEccSheet(sheet);
            }
        }

        private void InitBlockers()
        {
            mBlockers = new();
        }

        private void PushEccSheet(EccCapabilitySheet sheet)
        {
            foreach (var c in sheet.mComponents ?? Enumerable.Empty<EccComponent>())
                PushEccComponent(c);

            foreach (var c in sheet.mCapabilities ?? Enumerable.Empty<EccCapability>())
                PushEccCapability(c);

            foreach (var s in sheet.mCapabilitySheets ?? Enumerable.Empty<EccCapabilitySheet>())
                PushEccSheet(s);
        }

        private void PushEccCapability(EccCapability capability)
        {
            if (!mTickGroups.TryGetValue(capability.TickGroup, out var list))
            {
                list = new();
                mTickGroups[capability.TickGroup] = list;
            }
            list.Add(capability);
            capability.SetUp(this);
        }

        private void PushEccComponent(EccComponent component)
        {
            mRuntimeComponents.Add(component);
            component.SetUp(this);
        }

        private void PrintInitStatus()
        {
            var sb = new StringBuilder();
            sb.AppendLine("╔════════════════════════════════════════╗");
            sb.AppendLine("║      EccSystem Init Status             ║");
            sb.AppendLine("╚════════════════════════════════════════╝");
            sb.AppendLine();

            sb.AppendLine($"📋 Default Sheets: {mDefaultSheets.Count}");
            foreach (var sheet in mDefaultSheets)
            {
                sb.AppendLine($"  └─ {sheet?.name ?? "Unnamed Sheet"}");
            }
            sb.AppendLine();

            sb.AppendLine($"🧩 Runtime Components: {mRuntimeComponents.Count}");
            foreach (var component in mRuntimeComponents)
            {
                sb.AppendLine($"  └─ {component?.GetType().Name ?? "Unknown Component"}");
            }
            sb.AppendLine();

            sb.AppendLine($"⚙️ Tick Groups: {mTickGroups.Count}");
            foreach (var tickGroup in mTickGroups.OrderBy(x => x.Key))
            {
                sb.AppendLine($"  ├─ {tickGroup.Key} ({tickGroup.Value.Count} capabilities)");
                foreach (var capability in tickGroup.Value.OrderBy(x => x.TickGroupOrder))
                {
                    var tags =
                        capability.Tag?.Count > 0
                            ? $" [Tags: {string.Join(", ", capability.Tag)}]"
                            : "";
                    sb.AppendLine(
                        $"  │  └─ {capability.GetType().Name} (Order: {capability.TickGroupOrder}){tags}"
                    );
                }
            }
            sb.AppendLine("════════════════════════════════════════");
            Debug.Log(sb.ToString());
        }

        [OdinSerialize, ShowInInspector]
        internal List<EccCapabilitySheet> mDefaultSheets = new();

        [Header("Ecc Status")]
        [Space(10)]
        [ReadOnly, ShowInInspector]
        internal Dictionary<EccTag, List<EccCapability>> mBlockers = new();

        [ReadOnly, ShowInInspector]
        private Dictionary<EccTickGroup, List<EccCapability>> mTickGroups = new();

        [ReadOnly, ShowInInspector]
        private List<EccComponent> mRuntimeComponents = new();
    }
}
