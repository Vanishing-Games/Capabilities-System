using System;
using R3;
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

            mUpdateSubscribtion = Observable
                .EveryUpdate(SetFrameProvider())
                .Subscribe(_ => OnUpdateGo(Time.deltaTime));

            OnSetup();
        }

        protected virtual FrameProvider SetFrameProvider() => UnityFrameProvider.Update;

        protected virtual void OnSetup() { }

        /// <summary>
        /// Called when Component is removed from EccSystem.
        /// </summary>
        internal void Remove()
        {
            mOwner = null;
            mTransform = null;
            mGameObject = null;

            mUpdateSubscribtion.Dispose();

            OnRemoved();
        }

        protected virtual void OnRemoved() { }

        protected virtual void OnUpdateGo(float deltaTime) { }

        protected Transform mTransform;
        protected GameObject mGameObject;
        protected EccSystem mOwner;
        private IDisposable mUpdateSubscribtion;
    }
}
