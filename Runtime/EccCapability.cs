using System;
using System.Collections.Generic;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace VanishingGames.ECC.Runtime
{
    public abstract class EccCapability
    {
        public bool IsActive() => mIsActive;

        /// <summary>
        /// Called when Capability is created.
        /// </summary>
        internal virtual void SetUp(EccSystem owner, bool useClassDefinedTickSettings = true)
        {
            mOwner = owner;
            if (useClassDefinedTickSettings)
                SetUpTickSettings();
            OnSetup();
        }

        protected abstract void OnSetup();

        /// <summary>
        /// Setup TickGroup, TickGroupOrder, Tags
        /// </summary>
        protected abstract void SetUpTickSettings();

        /// <summary>
        /// Called on every frame when Capability is NOT active.
        /// </summary>
        internal virtual bool ShouldActivate()
        {
            return OnShouldActivate();
        }

        protected abstract bool OnShouldActivate();

        /// <summary>
        /// Called on every frame when Capability IS active.
        /// </summary>
        internal virtual bool ShouldDeactivate()
        {
            return OnShouldDeactivate();
        }

        protected abstract bool OnShouldDeactivate();

        /// <summary>
        /// Called when Capability is activated.
        /// </summary>
        internal virtual void Activate()
        {
            mIsActive = true;
            OnActivate();
        }

        protected abstract void OnActivate();

        /// <summary>
        /// Called when Capability is deactivated.
        /// </summary>
        internal virtual void Deactivate()
        {
            mIsActive = false;
            OnDeactivate();
        }

        protected abstract void OnDeactivate();

        /// <summary>
        /// Called on every frame when Capability IS active.
        /// </summary>
        internal virtual void Tick(float deltaTime)
        {
            OnTick(deltaTime);
        }

        protected abstract void OnTick(float deltaTime);

        /// <summary>
        /// Called when Owner is destroyed.
        /// </summary>
        internal virtual void OnOwnerDestroyed()
        {
            if (mIsActive)
                Deactivate();
        }

        [ShowInInspector, OdinSerialize]
        public EccTickType TickType { get; protected set; } = EccTickType.Fixed;

        [ShowInInspector, OdinSerialize]
        public EccTickGroup TickGroup { get; protected set; } = 0;

        [ShowInInspector, OdinSerialize]
        public uint TickOrderInGroup { get; protected set; } = 100;

        [ShowInInspector, OdinSerialize]
        public List<EccTag> Tags { get; protected set; } = new();

        protected EccSystem mOwner;
        protected bool mIsActive;
        protected float mActiveDuration;
        protected float mDeactiveDuration;
    }

    public interface IEccInstigator { }

    public enum EccTag
    {
        Move,
        Jump,
        Gravity,
        CollideAndSlide,
        GeometricDepenetration,
    }

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
    
    public enum EccTickType
    {
        ByFrame,
        Fixed,
    }
    // csharpier-ignore-end
}
