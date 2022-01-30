using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xolito.Control;

namespace Xolito.Core
{
    public class PlayerManager1 : MonoBehaviour
    {
        #region AUDIO //Borrar si rompe algo
        public AudioClip jump, dash;
        #endregion
        
        #region Variables
        [Header("References")]
        [SerializeField] PlayerController[] players = default;

        float xDirection = 0;
        #endregion
        void Update()
        {
            Check_Movement();
            Check_Jump();
            Check_Dash();
        }

        private void Check_Dash()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                foreach (PlayerController player in players)
                {
                    player.InteractWith_Dash();
                }
            }
        }

        private void Check_Jump()
        {
            if (Input.GetButton("Jump"))
            {
                foreach (PlayerController player in players)
                {
                    player.InteractWith_Jump();
                }
            }
        }

        private void Check_Movement()
        {
            xDirection = Input.GetAxisRaw("Horizontal");

            if (xDirection != 0)
            {

                try
                {
                    players[0]?.InteractWith_Movement(xDirection);
                    players[1]?.InteractWith_Movement(-xDirection);
                }
                catch (System.Exception)
                {

                }
            }
        }
    }
}