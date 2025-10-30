using Sirenix.OdinInspector;
using UnityEngine;

namespace VanishingGames.ECC.Runtime
{
    public abstract class EccComponent
    {
        /// <summary>
        /// Called when Component is added to EccSystem.
        /// </summary>
        internal void SetUp(EccSystem owner)
        {
            mOwner = owner;
            mTransform = mOwner.transform;
            mGameObject = mOwner.gameObject;

            OnSetup();
        }

        /// <summary>
        /// Called when Component is added to EccSystem.
        /// </summary>
        protected virtual void OnSetup() { }

        /// <summary>
        /// Called when Component is removed from EccSystem.
        /// </summary>
        internal void Remove()
        {
            mOwner = null;
            mTransform = null;
            mGameObject = null;

            OnRemoved();
        }

        /// <summary>
        /// Called when Component is removed.
        /// </summary>
        protected virtual void OnRemoved() { }

        protected Transform mTransform;
        protected GameObject mGameObject;
        protected EccSystem mOwner;
    }
}
