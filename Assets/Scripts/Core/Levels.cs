using System.Collections.Generic;
using UnityEngine;

namespace Xolito.Core
{
    [CreateAssetMenu(fileName = "Level", menuName = "Level/New Level list", order = 0)]
    public class Levels : ScriptableObject
    {
        [SerializeField] List<Level> levels;
        [SerializeField] GameObject counter;
        [SerializeField] GameObject staminaCounter;

        [System.Serializable]
        class Level
        {
            [SerializeField] GameObject startPoint;
            [SerializeField] GameObject endPoint;
            [SerializeField] GameObject LevelContent;

            public Vector3 StartPoint { get => startPoint.transform.position; }
            public Vector3 EndPoint { get => endPoint.transform.position; }
            
            public bool Active_Level(bool ShouldActive = true)
            {
                LevelContent.SetActive(ShouldActive);

                return false;
            }

            public bool Change_NextLevel()
            {
                return true;
            }

            public bool Change_FirstLevel()
            {
                return true;
            }

            public bool Restart_Level()
            {
                return true;
            }
        }
    }
}