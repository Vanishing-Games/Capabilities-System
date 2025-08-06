using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VanishingGames.ECC.Runtime
{
    public class EccCapability
    {
        protected virtual void Setup() { }

        protected virtual bool ShouldActivate()
        {
            return false;
        }

        protected virtual void OnActivated() { }

        protected virtual void OnDeactivated() { }

        protected virtual void OnTickActive(float time) { }

        protected List<EccTag> mTags;
        protected EccTickGroup mTickGroup;
    }

    public enum EccTag { }

    public enum EccTickGroup { }
}
