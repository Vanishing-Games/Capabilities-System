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
        public T GetEccComponent<T>()
            where T : EccComponent
        {
            return mRuntimeComponents.FirstOrDefault(c => c is T) as T;
        }

        public void BlockCapabilities(IEnumerable<EccTag> tagsToBlock, IEccInstigator instigator)
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                $"Blocking capabilities for tags by instigator: **{instigator.GetType().Name}**"
            );

            foreach (var tag in tagsToBlock)
            {
                if (!mBlockers.TryGetValue(tag, out var blockers))
                {
                    blockers = new List<IEccInstigator>();
                    mBlockers[tag] = blockers;
                }

                blockers.Add(instigator);
                sb.AppendLine($"  - Tag: {tag}");
            }

            EccLogger.LogInfo(sb.ToString());
        }

        public void UnblockCapabilities(IEnumerable<EccTag> tagsToBlock, IEccInstigator instigator)
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                $"Unblocking capabilities for tags by instigator: **{instigator.GetType().Name}**"
            );

            foreach (var tag in tagsToBlock)
            {
                if (mBlockers.TryGetValue(tag, out var blockers))
                {
                    blockers.Remove(instigator);
                    sb.AppendLine($"  - Tag: {tag}");
                }
                else
                {
                    EccLogger.LogError(
                        $"Tag **{tag}** not found in blockers, but trying to unblock by instigator: **{instigator.GetType().Name}**"
                    );
                }
            }

            EccLogger.LogInfo(sb.ToString());
        }

        private void Awake()
        {
            InitDefaultSheets();
            PrintInitStatus();
        }

        private void FixedUpdate()
        {
            foreach (var tickGroup in mTickGroups)
            {
                foreach (var capability in tickGroup.Value)
                {
                    bool isBlocked = false;
                    foreach (var tag in capability.Tags)
                    {
                        if (mBlockers.ContainsKey(tag))
                        {
                            isBlocked = true;
                            break;
                        }
                    }

                    if (isBlocked)
                        continue;

                    if (capability.IsActive())
                        capability.ShouldDeactivate();
                    else
                        capability.ShouldActivate();

                    if (capability.IsActive())
                        capability.Tick(Time.deltaTime);
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
            list.Sort((a, b) => a.TickOrderInGroup.CompareTo(b.TickOrderInGroup));

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
            foreach (var tickGroup in mTickGroups)
            {
                sb.AppendLine($"  ├─ {tickGroup.Key} ({tickGroup.Value.Count} capabilities)");
                foreach (var capability in tickGroup.Value)
                {
                    var tags =
                        capability.Tags?.Count > 0
                            ? $" [Tags: {string.Join(", ", capability.Tags)}]"
                            : "";
                    sb.AppendLine(
                        $"  │  └─ {capability.GetType().Name} (Order: {capability.TickOrderInGroup}){tags}"
                    );
                }
            }
            sb.AppendLine("════════════════════════════════════════");
            EccLogger.LogInfo(sb.ToString());
        }

        [OdinSerialize, ShowInInspector]
        internal List<EccCapabilitySheet> mDefaultSheets = new();

        [Header("Ecc Status")]
        [Space(10)]
        [ReadOnly, ShowInInspector]
        internal Dictionary<EccTag, List<IEccInstigator>> mBlockers = new();

        [ReadOnly, ShowInInspector]
        private SortedDictionary<EccTickGroup, List<EccCapability>> mTickGroups = new();

        [ReadOnly, ShowInInspector]
        private List<EccComponent> mRuntimeComponents = new();
    }
}
