using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VanishingGames.ECC.Runtime
{
    public abstract class EccCapability
    {
        public bool IsActive() => mIsActive;

        /// <summary>
        /// Called when Capability is created.
        /// </summary>
        internal virtual void SetUp(EccSystem owner)
        {
            mOwner = owner;
        }

        /// <summary>
        /// Called on every frame when Capability is NOT active.
        /// </summary>
        internal virtual bool ShouldActivate()
        {
            return true;
        }

        /// <summary>
        /// Called on every frame when Capability IS active.
        /// </summary>
        internal virtual bool ShouldDeactivate()
        {
            return false;
        }

        /// <summary>
        /// Called when Capability is activated.
        /// </summary>
        internal virtual void OnActivated() { }

        /// <summary>
        /// Called when Capability is deactivated.
        /// </summary>
        internal virtual void OnDeactivated() { }

        /// <summary>
        /// Called on every frame when Capability IS active.
        /// </summary>
        internal virtual void OnTickActive(float deltaTime) { }

        /// <summary>
        /// Called when Owner is destroyed.
        /// </summary>
        internal virtual void OnOwnerDestroyed()
        {
            if (mIsActive)
                OnDeactivated();
        }

        internal List<EccTag> Tag
        {
            get => mTags;
            private set => mTags = value;
        }

        internal EccTickGroup TickGroup
        {
            get => mTickGroup;
            private set => mTickGroup = value;
        }

        internal uint TickGroupOrder
        {
            get => mTickGroupOrder;
            private set => mTickGroupOrder = value;
        }

        protected List<EccTag> mTags;
        protected EccTickGroup mTickGroup;
        protected uint mTickGroupOrder = 100;

        protected EccSystem mOwner;
        protected bool mIsActive;
        protected float mActiveDuration;
        protected float mDeactiveDuration;
    }

    public interface IEccInstigator { }

    public enum EccTag { }

    // csharpier-ignore-start
    public enum EccTickGroup
    {
        Input          = 0,
        BeforeMovement = 1,
        Movement       = 2,
        AfterMovement  = 3,
        BeforeGameplay = 4,
        Gameplay       = 5,
        AfterGameplay  = 6,
        BeforePhysics  = 7,
        Physics        = 8,
        AfterPhysics   = 9,
    }
    // csharpier-ignore-end
}
