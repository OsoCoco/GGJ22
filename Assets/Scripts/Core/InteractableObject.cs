using System.Collections;
using UnityEngine;

namespace Xolito.Core
{
    public class InteractableObject : MonoBehaviour
    {
        [SerializeField] Interaction interaction = Interaction.None;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.transform.tag == "Player")
            {
                Levels level = GameObject.FindObjectOfType<Levels>();

                if (interaction == Interaction.Damage)
                {
                    level.Restart_Level();
                }
                else if (interaction == Interaction.EndPoint)
                {
                    level.Change_NextLevel();
                }
                else if (interaction == Interaction.Coin)
                {
                    level.Add_Coin();
                }
            }
        }
    }
}