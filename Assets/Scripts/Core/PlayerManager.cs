using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Vairbales")]
    [SerializeField] float speed;
    [SerializeField] float gravityScale;
    [SerializeField] float fallGravityScale;
    [SerializeField] float jumpForce;


    [Header("Player Ref")]
    [SerializeField] Player player1, player2;
    
    bool isMoving;
    Vector2 movement;


    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        
        movement = new Vector2(x, 0);

        if(movement != Vector2.zero)
        {
            player1.Move(movement, speed);
            player2.Move(-movement, speed);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            player1.Jump(jumpForce);
            player2.Jump(jumpForce);
        }
    }
}
