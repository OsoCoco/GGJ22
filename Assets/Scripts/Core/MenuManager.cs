using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{

    public GameObject mainMenu;
    public GameObject credits;
    public GameObject pause;

 
    public void StartGame()
    {
        mainMenu.SetActive(false);
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
}
