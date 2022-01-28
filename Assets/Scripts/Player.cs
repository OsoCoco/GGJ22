using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public LayerMask groundMask;

    public bool isGrounded;

    public float radiusCheck;

    public Collider2D col;


    float velocity;
    public float jumpForce;
    public float gravity;
    public float gravityScale;



    private void Update()
    {

        velocity += gravity * gravityScale * Time.deltaTime;

        if (isGrounded && velocity < 0)
        {
            velocity = 0;
        }

        transform.Translate(new Vector3(0, velocity, 0) * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        col = Physics2D.OverlapCircle(transform.position, radiusCheck,groundMask);

        if (col != null)
            isGrounded = true;
        else
            isGrounded = false;
    }
    public void Movement(Vector2 m, float speed)
    {
        transform.Translate(m * speed * Time.deltaTime);
    }

    public void Jump()
    {
        if(isGrounded)
            velocity = jumpForce;
       
    }

    public void Dash()
    {

    }
}
