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
        //BORRAR SI ROMPE ALGO
        #region AUDIO 
        [SerializeField] public AudioSource source;
        #endregion

        #region variables
        public Animator animatorXolos;
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

                if(direction != 0)
                {
                    //Debug.Log("Direction " + direction);
                    animatorXolos.SetBool("isMoving",true);

                    if(direction < 0)
                    {
                        this.GetComponent<SpriteRenderer>().flipX = true;
                    }
                    else 
                    {
                        this.GetComponent<SpriteRenderer>().flipX = false;
                    }
                }
                  
                //animatorXolos.SetInteger(0, (int)direction);
                //source.PlayOneShot(pSettings.Get_Audio(BasicActions.Walk));
            }
        }

        public void InteractWith_Dash()
        {
            if (mover.Dash())
            {
                animatorXolos.SetBool("isDashing", true);
                //source.PlayOneShot(pSettings.Get_Audio(BasicActions.Dash));
            }
        }

        public void InteractWith_Jump()
        {
            if (mover.Jump())
            {
                animatorXolos.SetTrigger("jump");

                
                source.PlayOneShot(pSettings.Get_Audio(BasicActions.Jump));
            }
        }
        #endregion
    }
}