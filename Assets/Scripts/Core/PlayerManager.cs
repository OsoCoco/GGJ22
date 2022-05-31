using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xolito.Control;

namespace Xolito.Core
{
    public class PlayerManager : MonoBehaviour
    {
        #region AUDIO //Borrar si rompe algo
        MenuManager menuManager;


        private bool canCheck = false;

        #endregion

        #region Variables
        [Header("References")]
        [SerializeField] PlayerController[] players = default;

        float xDirection = 0;


        #endregion

        private void Awake()
        {
            menuManager = GameObject.FindObjectOfType<MenuManager>();
        }

        private void Start()
        {
            canCheck = true;
        }

        void Update()
        {
            if (canCheck)
            {
                Check_Movement();
                Check_Jump();
                Check_Dash();
            }
        }

        private void OnEnable()
        {
            menuManager.onEnteredMenu += Disable_Inputs;
            menuManager.onExitedMenu += Enable_Inputs;
        }

        private void OnDisable()
        {
            menuManager.onEnteredMenu -= Disable_Inputs;
            menuManager.onExitedMenu -= Enable_Inputs;
        }

        private void Check_Dash()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                foreach (PlayerController player in players)
                {
                    player.Dash();
                }
            }
        }

        private void Check_Jump()
        {
            if (Input.GetButton("Jump"))
            {
                foreach (PlayerController player in players)
                {
                    player.Jump();
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
                    players[0]?.Move(xDirection);
                    players[1]?.Move(-xDirection);


                }
                catch (System.Exception)
                {

                }
            }
            else
            {
                players[0]?.animatorXolos.SetBool("isMoving", false);
                players[1]?.animatorXolos.SetBool("isMoving", false);
            }
        }

        public void Respawn(Vector3 start1, Vector3 start2)
        {
            try
            {
                players[0].transform.position = start1;
                players[1].transform.position = start2;
            }
            catch (System.Exception)
            {

            }
        }

        private void Disable_Inputs() => canCheck = false;
        private void Enable_Inputs() => canCheck = true;
    }
}