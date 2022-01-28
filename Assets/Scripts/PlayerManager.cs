using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    float speed;

    public bool isGrounded;

    [SerializeField]
    LayerMask groundMask;

    public GameObject player1,player2;


    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");


        Vector2 movement = new Vector2(x, 0); 

        player1.transform.Translate(movement * speed * Time.deltaTime);
        player2.transform.Translate(-movement * speed * Time.deltaTime);
    }



}
