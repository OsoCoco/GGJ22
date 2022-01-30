using System.Collections.Generic;
using UnityEngine;
//using TMPro;
using UnityEngine.SceneManagement;

namespace Xolito.Core
{
    public class LevelController : MonoBehaviour
    {
        PlayerManager1 manager1;

        [SerializeField] GameObject coinsCounter;
        [SerializeField] GameObject staminaCounter;
        [Space]
        [SerializeField] GameObject startPointUp;
        [SerializeField] GameObject endPointUp;
        [SerializeField] GameObject startPointDown;
        [SerializeField] GameObject endPointDown;
        [Space]
        [SerializeField] public GameObject[] coins;

        float currentCoins = 0;
        int currentLevel = 0;

        private void Awake()
        {
            manager1 = GameObject.FindObjectOfType<PlayerManager1>();
        }

        public bool Change_NextLevel()
        {
            Change_Level(currentLevel + 1);
            return true;
        }

        private void Change_Level(int level)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

            
            currentLevel = level;
        }

        public void Change_FirstLevel()
        {
            SceneManager.LoadScene(0);

         
        }

        public void Restart_Level()
        {
            foreach (GameObject coin in coins)
            {
                coin.SetActive(true);
            }

            manager1.Respawn(startPointUp.transform.position, startPointDown.transform.position);

           ;
        }

        public void Add_Coin()
        {
            currentCoins++;
            //coinsCounter.GetComponent<TMPro.TextMeshProUGUI>().text = coins.ToString();
        }
    }
}