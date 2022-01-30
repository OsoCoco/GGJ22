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
    public bool canJump = true;

    public bool testJump = false;

    #region Properties
    float Speed { get => manager1.Speed; }
    float JumpForce { get => manager1.JumpForce; }
    float JumpCoolDown { get => manager1.JumpCoolDown; }
    float DashDistance { get => manager1.DashDistance; }
    float DashTime { get => manager1.DashTime; }
    public List<string> TagsToAvoid { get => manager1.TagsToAvoid; } 
    #endregion

    #region Unity methods
    private void Awake()
    {
        manager1 = GameObject.FindObjectOfType<PlayerManager1>();
        rgb2d = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        canJump = true;
    }

    void Update()
    {
        //if (!Check_Platform())
        //    Check_Ground();
        Check_Ground();

        //if (!isGrounded) Check_Platform();

        Check_Wall();

        float? test = Check_WayToMove(Vector2.up, 1f, TagsToAvoid);
        testJump = test.HasValue;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (colliderToAvoid)
            Ignore_Collition();
    } 
    #endregion

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

        float? distance = Check_WayToMove(currentDirection, 1);

        if (distance.HasValue)
            Dash(distance.Value);
        else
            Dash(DashDistance * currentDirection.x);
    }

    public void InteractWith_Jump()
    {
        if (!isGrounded || inDash || !canJump) return;

        float? distance = Check_WayToMove(Vector2.up, .5f, TagsToAvoid);

        if (distance.HasValue)
        {
            if (colliderToAvoid)
            {
                Jump();
                Ignore_Collition(); 
            }
        }
        else
            Jump();
    }

    private void Ignore_Collition(bool shouldIgnore = true)
    {
        Physics2D.IgnoreCollision(boxCollider, colliderToAvoid, shouldIgnore);

        if (!shouldIgnore)
            colliderToAvoid = default;
    }

    void Check_Ground()
    {
        float? distance = Check_WayToMove(Vector2.down);

        if (distance.HasValue)
        {
            //if (!isGrounded && canJump)
            //    StartCoroutine(Disable_Jump());

            if (distance.Value <= .5f && !isGrounded)
            {
                isGrounded = true;
                StartCoroutine(Disable_Jump());
            }

            if (colliderToAvoid)
                Ignore_Collition(false);
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
            x = transform.position.x + direction.x * offset,
            y = transform.position.y + direction.y * offset,
            z = 0
        };

        //float angle = Get_AngleDirection();

        hit2D = Physics2D.BoxCastAll(startPosition, boxCollider.size, 0, currentDirection, 1);

        if (hit2D.Length != 0)
        {
            foreach (RaycastHit2D hit in hit2D)
            {
                if (tagsToAvoid != null && tagsToAvoid.Count != 0)
                {
                    bool founded = Find_TagIn(tagsToAvoid, hit.transform.tag);

                    Check_Collider(hit, founded);
                }

                if (hit.transform.gameObject == gameObject) continue;

                if (Vector2.Angle(direction *-1, hit.normal) < 30)
                {
                    return direction.x != 0 ? hit.distance * direction.x : hit.distance * direction.y;
                }
            }
        }

        return null;

        void Check_Collider(RaycastHit2D hit, bool founded)
        {
            if (founded && !colliderToAvoid)
                colliderToAvoid = hit.transform.GetComponent<BoxCollider2D>();
            else if (founded && colliderToAvoid == hit.transform.GetComponent<BoxCollider2D>())
                colliderToAvoid = hit.transform.GetComponent<BoxCollider2D>();
            else if (!founded && colliderToAvoid)
                colliderToAvoid = default;
        }
    }

    //private float Get_AngleDirection(Vector2 direction)
    //{
    //    switch (direction)
    //    {
    //        case Vector2.up:

    //            break;
    //    }
    //}

    private bool Find_TagIn(List<string> tagsToAvoid, string tagToFind)
    {
        bool founded = false;
        foreach (string tag in tagsToAvoid)
        {
            if (tagToFind == tag)
            {
                founded = true;
                break;
            }
        }

        return founded;
    }

    private void Dash(float finalDistance)
    {
        Vector2 finalTarget = default;

        if (DashDistance < finalDistance)
            finalTarget = new Vector2(transform.position.x + DashDistance * currentDirection.x, transform.position.y);
        else
            finalTarget = new Vector2(transform.position.x + finalDistance, transform.position.y);

        StartCoroutine(DashTo(finalTarget));
    }

    IEnumerator DashTo(Vector2 final)
    {
        inDash = true;
        FreezeVerticalPosition();

        transform.position = Vector2.SmoothDamp(transform.position, final, ref dashVelocity, DashTime, Speed);
        float curentTime = 0;

        while (curentTime < DashTime)
        {
            transform.position = Vector2.SmoothDamp(transform.position, final, ref dashVelocity, DashTime, Speed);
            yield return new WaitForEndOfFrame();
            curentTime += Time.deltaTime;
        }

        dashVelocity = default;
        inDash = false;
        FreezeVerticalPosition(false);
        yield break;
    }

    private void FreezeVerticalPosition(bool shouldFreeze = true)
    {
        rgb2d.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (!shouldFreeze)
            return;

        rgb2d.constraints = RigidbodyConstraints2D.FreezePositionY;
    }

    IEnumerator Disable_Jump()
    {
        canJump = false;
        yield return new WaitForSeconds(JumpCoolDown);
        
        if (isGrounded)
            canJump = true;
    }
}
