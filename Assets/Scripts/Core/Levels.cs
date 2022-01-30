using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Xolito.Core
{
    public class Levels : MonoBehaviour
    {
        [SerializeField] List<Level> levels;
        [SerializeField] GameObject counter;
        [SerializeField] GameObject staminaCounter;
        float coins = 0;
        int currentLevel = 0;

        [System.Serializable]
        class Level
        {
            [SerializeField] GameObject startPoint1;
            [SerializeField] GameObject startPoint2;
            [SerializeField] GameObject endPoint;
            [SerializeField] GameObject LevelContent;
            [SerializeField] public GameObject[] coins;

            public Vector3 StartPoint1 { get => startPoint1.transform.position; }
            public Vector3 StartPoint2 { get => startPoint2.transform.position; }
            public Vector3 EndPoint { get => endPoint.transform.position; }

            public bool Active_Level(bool ShouldActive = true)
            {
                LevelContent.SetActive(ShouldActive);

                return false;
            }
        }

        public bool Change_NextLevel()
        {
            Change_Level(currentLevel + 1);
            return true;
        }

        private void Change_Level(int level)
        {
            levels[currentLevel].Active_Level(false);
            levels[level].Active_Level();

            GameObject.FindObjectOfType<PlayerManager1>().Respawn(levels[level].StartPoint1, levels[level].StartPoint2);
            currentLevel = level;
        }

        public bool Change_FirstLevel()
        {
            Change_Level(0);

            return true;
        }

        public bool Restart_Level()
        {
            foreach (GameObject coin in levels[currentLevel].coins)
            {
                coin.SetActive(true);
            }
            GameObject.FindObjectOfType<PlayerManager1>().Respawn(levels[currentLevel].StartPoint1, levels[currentLevel].StartPoint2);

            return true;
        }

        public void Add_Coin()
        {
            coins++;
            counter.GetComponent<TMPro.TextMeshProUGUI>().text = coins.ToString();
        }
    }
}