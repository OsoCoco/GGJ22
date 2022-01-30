using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xolito.Utilities;
using Xolito.Core;

namespace Xolito.Control
{
    public class PlayerController : MonoBehaviour
    {
        #region variables
        [SerializeField] Animator animatorXolos;
        [SerializeField] PlayerSettings pSettings = null;
        Movement.Mover mover;
        #endregion

        #region Unity methods
        private void Awake()
        {
            animatorXolos = GetComponent<Animator>();

            mover = GetComponent<Movement.Mover>();
        }

        private void Start()
        {
            
        }
        #endregion

        #region Public methods
        public void InteractWith_Movement(float direction)
        {
            if (mover.InteractWith_Movement(direction))
            {
                animatorXolos.SetInteger(0, (int)direction);
            }

                //if (direction == 0)
                //    return true;
        }

        public void InteractWith_Dash()
        {
            if (mover.Dash())
            {

            }
        }

        public void InteractWith_Jump()
        {
            if (mover.Jump())
            {

            }
        }
        #endregion
    }
}