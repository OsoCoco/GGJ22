using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Jump Variables")]
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] bool isGrounded;
    [SerializeField] float checkDistance;

    [SerializeField]
    Rigidbody2D rb;

  
    public void Move(Vector2 m, float s)
    {
        rb.MovePosition((Vector2)transform.position + m * s * Time.deltaTime);
    }


    public void Jump(float force)
    {
        if (isGrounded)
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }
    private void FixedUpdate()
    {
        if(Physics2D.Raycast(transform.position,Vector2.down,checkDistance,whatIsGround))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
}
