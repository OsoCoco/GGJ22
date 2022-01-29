using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager1 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerController[] players = default;
    [SerializeField] List<string> tagsToAvoid = default;

    [Header("Configuration")]
    [SerializeField] float speed = 5;
    [SerializeField] float jumpForce = 5;
    [SerializeField] float dashDistance = 5;
    [SerializeField] float dashTime = 3;
    float xDirection = 0;

    public float Speed { get => speed; }
    public float JumpForce { get => jumpForce; }
    public float DashDistance { get => dashDistance; }
    public float DashTime { get => dashTime; }
    public List<string> TagsToAvoid { get => tagsToAvoid; }

    void Update()
    {
        xDirection = Input.GetAxisRaw("Horizontal");

        if (xDirection != 0){
            
            players[0]?.InteractWith_Movement(xDirection);
            players[1]?.InteractWith_Movement(-xDirection);
        }

        if (Input.GetButton("Jump"))
        {
            foreach (PlayerController player in players)
            {
                player.InteractWith_Jump();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            foreach (PlayerController player in players)
            {
                player.InteractWith_Dash();
            }
        }
    }
}
