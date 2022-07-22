using System.Collections;
using UnityEngine;
using System;

namespace Xolito.Utilities
{
    public class CoolDownAction
    {
        private float time;
        private float currentTime;
        private bool canUse;
        private bool inCoolDown;
        private Action<bool> OnFinished;
        private bool invertFunction;

        public bool CanUse
        {
            get => canUse;
            set
            {
                if (inCoolDown) return;

                canUse = invertFunction ? !value : value;

                OnFinished?.Invoke(CanUse);
            }
        }

        public CoolDownAction(float time)
        {
            this.time = time;
            canUse = true;
        }

        public CoolDownAction(float time, Action<bool> OnFinished) : this(time)
        {
            this.OnFinished = OnFinished;
        }

        public CoolDownAction(float time, Action<bool> OnFinished, bool invert) : this(time, OnFinished)
        {
            canUse = false;
            invertFunction = invert;
        }

        public void Restart()
        {
            currentTime = 0;
            CanUse = true;
            inCoolDown = false;
        }

        public IEnumerator CoolDown()
        {
            if (inCoolDown) yield break;

            CanUse = false;
            inCoolDown = true;
            currentTime = time - Time.deltaTime;

            while (currentTime > 0)
            {
                currentTime -= Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            inCoolDown = false;
            CanUse = true;
        }
    }
}