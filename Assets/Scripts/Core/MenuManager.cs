using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using TMPro;
using Xolito.Utilities;
using System;
using UnityEngine.SceneManagement;
using Xolito.UI;

namespace Xolito.Core
{
    public class MenuManager : MonoBehaviour
    {
        public GameObject mainMenu;
        public GameObject credits;
        public GameObject pause;
        [SerializeField] float pauseCooldown = 2;
        bool canPause = true;
        Fade fade;
        //public currentMenu;

        public event Action onEnteredMenu;
        public event Action onExitedMenu;

        private void Awake()
        {
            fade = GameObject.FindObjectOfType<Fade>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }
        }

        public void StartGame()
        {
            mainMenu.SetActive(false);
            SceneManager.LoadScene(1);
        }

        public void ExitGame()
        {
            Application.Quit();
        }

        public void OpenCredits()
        {
            credits.SetActive(true);
            mainMenu.SetActive(false);
        }

        public void BackToMenu()
        {
            credits.SetActive(false);
            mainMenu.SetActive(true);
        }

        public void Pause()
        {
            if (canPause)
            {
                print("pause");
                if (mainMenu.activeSelf)
                    mainMenu.SetActive(false);
                else if (credits.activeSelf)
                    return;

                print("pause");
                pause.SetActive(!pause.activeSelf);
                Utilities.Utilities.StopTime();
                //fade.FadeIn();

                onEnteredMenu();
                StartCoroutine(DisablePause());
            }
        }

        IEnumerator DisablePause()
        {
            canPause = false;
            yield return new WaitForSecondsRealtime(pauseCooldown);

            canPause = true;
        }
    } 
}
