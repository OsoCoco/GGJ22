using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BoxDimentions
{
    public Vector2 centre;
    public Vector2 extends;

    public BoxDimentions(Vector2 centre, Vector2 extends)
    {
        this.centre = centre;
        this.extends = extends;
    }
}

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    PlayerManager1 manager1 = null;
    Rigidbody2D rgb2d;
    BoxCollider2D boxCollider;
    Animator animator;
    Vector2 dashVelocity = default;
    
    Vector2 currentDirection = default;
    bool inDash = false;
    public bool isGrounded = false;
    BoxCollider2D colliderToAvoid = default;
    (bool isTouchingWall, bool isAtRight) wallDetection;

    float Speed { get => manager1.Speed; }
    float JumpForce { get => manager1.JumpForce; }
    float DashDistance { get => manager1.DashDistance; }
    float DashTime { get => manager1.DashTime; }
    public List<string> TagsToAvoid { get => manager1.TagsToAvoid; }

    private void Awake()
    {
        manager1 = GameObject.FindObjectOfType<PlayerManager1>();
        rgb2d = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        //if (!Check_Platform())
        //    Check_Ground();
        Check_Ground();

        if (!isGrounded) Check_Platform();

        Check_Wall();
    }

    private bool Check_Platform()
    {
        if(!isGrounded && colliderToAvoid)
        {
            if (boxCollider.bounds.min.y >= colliderToAvoid.bounds.max.y)
            {
                Ignore_Collition(false);
                colliderToAvoid = default;
            }

            //Check_WayToMove(Vector2.down);
            //isGrounded = true;
        }

        return false;
    }

    public bool InteractWith_Movement(float direction)
    {
        if (!Move(direction))
            return false;

        if (direction == 0)
            return true;

        currentDirection.x = direction > 0 ? 1 : -1;
        return true;
    }

    public void InteractWith_Dash()
    {
        if (inDash)
            return;

        float? distance = Check_WayToMove(currentDirection, DashDistance);

        if (distance.HasValue)
            Dash(distance.Value);
        else
            Dash(DashDistance * currentDirection.x);
    }

    public void InteractWith_Jump()
    {
        if (!isGrounded || inDash) return;

        float? distance = Check_WayToMove(Vector2.up, 1f, TagsToAvoid);

        if (distance.HasValue && colliderToAvoid)
        {
            Jump();
            Ignore_Collition();
        }
        else
            Jump();
    }

    private void Ignore_Collition(bool shouldIgnore = true) => Physics2D.IgnoreCollision(boxCollider, colliderToAvoid, shouldIgnore);

    void Check_Ground()
    {
        float? distance = Check_WayToMove(Vector2.down);

        if (distance.HasValue)
        {
            if (distance.Value <= .5f)
                isGrounded = true;
        }
        else
            isGrounded = false;
    }

    void Check_Wall()
    {
        float? distance = Check_WayToMove(currentDirection, .1f, TagsToAvoid);

        if (distance.HasValue && distance <= 0)
            wallDetection.isTouchingWall = true;
        else
            wallDetection.isTouchingWall = false;

        wallDetection.isAtRight = (currentDirection.x >= 0);
    }

    bool Move(float direction)
    {
        if (inDash) return false;

        if (!wallDetection.isTouchingWall || (wallDetection.isTouchingWall && ((direction >= 0 && wallDetection.isAtRight) || !(direction >= 0 && wallDetection.isAtRight))))
        {
            transform.Translate(direction * Speed * Time.deltaTime, 0, 0);
        }

        return true;
    }

    void Jump()
    {
        rgb2d.velocity = new Vector2(rgb2d.velocity.x, JumpForce);
    }

    float? Check_WayToMove(Vector2 direction, float offset = .1f, List<string> tagsToAvoid = null)
    {
        RaycastHit2D[] hit2D;

        Vector2 offset1 = direction * offset;
        Vector3 startPosition = new Vector3()
        {
            x = transform.position.x + offset1.x,
            y = transform.position.y + offset1.y,
            z = 0
        };

        hit2D = Physics2D.BoxCastAll(startPosition, boxCollider.size, 0, currentDirection);

        if (hit2D.Length != 0)
        {
            foreach (RaycastHit2D hit in hit2D)
            {
                if (tagsToAvoid != null && tagsToAvoid.Count != 0)
                {
                    bool founded = false;
                    foreach (string tag in tagsToAvoid)
                    {
                        if (hit.transform.tag == tag)
                        {
                            founded = true;
                            break;
                        }
                    }

                    if (founded && !colliderToAvoid)
                        colliderToAvoid = hit.transform.GetComponent<BoxCollider2D>();
                    else if (founded && colliderToAvoid == hit.transform.GetComponent<BoxCollider2D>())
                        colliderToAvoid = hit.transform.GetComponent<BoxCollider2D>();
                    else if (!founded && colliderToAvoid)
                        colliderToAvoid = default;
                }

                if (hit.transform.gameObject == gameObject) continue;

                if (Vector2.Angle(direction *-1, hit.normal) < 30)
                {
                    return direction.x != 0 ? hit.distance * direction.x : hit.distance * direction.y;
                }
            }
        }

        return null;
    }

    private void Dash(float finalDistance)
    {
        Vector2 finalTarget = default;

        //if (DashDistance < finalDistance)
        //    finalTarget = new Vector2(transform.position.x + DashDistance * currentDirection.x, transform.position.y);
        //else
            finalTarget = new Vector2(transform.position.x + finalDistance, transform.position.y);

        StartCoroutine(DashTo(finalTarget));
    }

    IEnumerator DashTo(Vector2 final)
    {
        transform.position = Vector2.SmoothDamp(transform.position, final, ref dashVelocity, DashTime, Speed);
        inDash = true;
        float curentTime = 0;

        while (curentTime < DashTime)
        {
            transform.position = Vector2.SmoothDamp(transform.position, final, ref dashVelocity, DashTime, Speed);
            yield return new WaitForEndOfFrame();
            curentTime += Time.deltaTime;
        }

        dashVelocity = default;
        inDash = false;
        yield break;
    }
}
