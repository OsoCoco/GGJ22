using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    float playerSpeed;


    public Player player1,player2;


    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");


        Vector2 movement = new Vector2(x, 0);

        player1.Movement(movement, playerSpeed);
        player2.Movement(-movement, playerSpeed);


        if(Input.GetKeyDown(KeyCode.Space))
        {
            player1.Jump();
            player2.Jump();
        }
    }

    //HOLA GITHUB

}
