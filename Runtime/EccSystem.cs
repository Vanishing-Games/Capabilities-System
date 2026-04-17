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

        public void BlockCapabilities(EccTag tagToBlock, IEccInstigator instigator)
        {
            BlockCapabilities(new[] { tagToBlock }, instigator);
        }

        public void BlockCapabilities(IEnumerable<EccTag> tagsToBlock, IEccInstigator instigator)
        {
            var instigatorName = instigator.GetType().Name;
            var sb = new StringBuilder();
            sb.AppendLine($"Blocking capabilities for tags by instigator: **{instigatorName}**");

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

        public void UnblockCapabilities(EccTag tagToBlock, IEccInstigator instigator)
        {
            UnblockCapabilities(new[] { tagToBlock }, instigator);
        }

        public void UnblockCapabilities(IEccInstigator instigator)
        {
            var instigatorName = instigator.GetType().Name;
            var sb = new StringBuilder();
            sb.AppendLine($"Unblocking capabilities for tags by instigator: **{instigatorName}**");

            var tagsToRemove = new List<EccTag>();
            foreach (var kvp in mBlockers)
            {
                if (kvp.Value.Remove(instigator))
                {
                    sb.AppendLine($"  - Tag: {kvp.Key}");

                    if (kvp.Value.Count == 0)
                        tagsToRemove.Add(kvp.Key);
                }
            }

            foreach (var tag in tagsToRemove)
                mBlockers.Remove(tag);

            EccLogger.LogInfo(sb.ToString());
        }

        public void UnblockCapabilities(IEnumerable<EccTag> tagsToBlock, IEccInstigator instigator)
        {
            var instigatorName = instigator.GetType().Name;
            var sb = new StringBuilder();
            sb.AppendLine($"Unblocking capabilities for tags by instigator: **{instigatorName}**");

            foreach (var tag in tagsToBlock)
            {
                if (mBlockers.TryGetValue(tag, out var blockers))
                {
                    blockers.Remove(instigator);
                    sb.AppendLine($"  - Tag: {tag}");

                    if (blockers.Count == 0)
                        mBlockers.Remove(tag);
                }
                else
                {
                    EccLogger.LogError(
                        $"Tag **{tag}** not found in blockers, but trying to unblock by instigator: **{instigatorName}**"
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

        private void Update()
        {
            TickCapabilitiesByType(EccTickType.ByFrame, Time.deltaTime);
        }

        private void FixedUpdate()
        {
            TickCapabilitiesByType(EccTickType.Fixed, Time.fixedDeltaTime);
        }

        private void TickCapabilitiesByType(EccTickType tickType, float deltaTime)
        {
            foreach (var tickGroup in mTickGroups)
            {
                foreach (var capability in tickGroup.Value)
                {
                    if (capability.TickType != tickType)
                        continue;

                    bool isBlocked = false;
                    foreach (var tag in capability.Tags)
                    {
                        if (mBlockers.ContainsKey(tag))
                        {
                            isBlocked = true;
                            break;
                        }

#if UNITY_EDITOR
                        if (DebugBlockedTags.Contains(tag))
                        {
                            isBlocked = true;
                            break;
                        }
#endif
                    }

                    if (isBlocked)
                        continue;

                    if (capability.IsActive())
                    {
                        if (capability.ShouldDeactivate())
                            capability.Deactivate();
                    }
                    else
                    {
                        if (capability.ShouldActivate())
                            capability.Activate();
                    }

                    if (capability.IsActive())
                        capability.Tick(deltaTime);
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
            if (sheet == null)
            {
                EccLogger.LogError("Attempted to add a null EccCapabilitySheet.");
                return;
            }

            EccCapabilitySheet targetSheet = sheet;

            // 如果启用了运行时深拷贝，且当前处于 Play 模式，则通过实例化创建副本
            if (RuntimeDeepCopy && Application.isPlaying)
            {
                targetSheet = Instantiate(sheet);
                targetSheet.name = $"{sheet.name} (Runtime Copy)";
            }

            foreach (var c in targetSheet.mComponents ?? Enumerable.Empty<EccComponent>())
                PushEccComponent(c);

            foreach (var c in targetSheet.mCapabilities ?? Enumerable.Empty<EccCapability>())
                PushEccCapability(c);

            // 递归调用：如果 targetSheet 包含嵌套的 Sheet，这里也会正常触发顶部的 Instantiate 深拷贝逻辑
            foreach (var s in targetSheet.mCapabilitySheets ?? Enumerable.Empty<EccCapabilitySheet>())
                PushEccSheet(s);
        }

        private void PushEccCapability(EccCapability capability)
        {
            if (capability == null)
            {
                EccLogger.LogError("Attempted to add a null EccCapability.");
                return;
            }

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
            if (component == null)
            {
                EccLogger.LogError("Attempted to add a null EccComponent.");
                return;
            }

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
                    var tickType = $"[{capability.TickType}]";
                    sb.AppendLine(
                        $"  │  └─ {capability.GetType().Name} {tickType} (Order: {capability.TickOrderInGroup}){tags}"
                    );
                }
            }
            sb.AppendLine("════════════════════════════════════════");
            EccLogger.LogInfo(sb.ToString());
        }

        void OnDestroy()
        {
            foreach (var component in mRuntimeComponents)
                component.Remove();
        }

        [Header("Runtime Settings")]
        [Space(10)]
        [InfoBox("If true, creates a deep copy of the capability sheets at runtime to prevent modifying the original ScriptableObjects.")]
        [ShowInInspector]
        public bool RuntimeDeepCopy = true;

        [OdinSerialize, ShowInInspector, InlineEditor]
        internal List<EccCapabilitySheet> mDefaultSheets = new();

#if UNITY_EDITOR
        [Header("Debug Settings")]
        [Space(10)]
        [InfoBox(
            "Select tags to block for debugging purposes. These blocks are only active in the Unity Editor."
        )]
        [ShowInInspector]
        public List<EccTag> DebugBlockedTags = new();
#endif

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