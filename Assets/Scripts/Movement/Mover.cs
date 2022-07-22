using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xolito.Control;
using Xolito.Core;
using Xolito.Utilities;
using static Xolito.Utilities.Utilities;

namespace Xolito.Movement
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Mover : MonoBehaviour
    {
        #region variables
        [Header("References")]
        [SerializeField] PlayerSettings pSettings = null;

        [Space]
        [Header("Status")]
        [SerializeField] private bool inDash = false;
        [SerializeField] private bool isGrounded = false;
        [SerializeField] private bool canJump = true;
        [SerializeField] private bool canDash = false;
        [SerializeField] private bool isBesidePlatform = false;
        [SerializeField] private bool isTouchingTheWall = false;
        [SerializeField] private bool isWallRight = false;
        [SerializeField] private float angleOfContact = 0;

        Rigidbody2D rgb2d;
        BoxCollider2D boxCollider;
        BoxCollider2D colliderToAvoid = default;
        CoolDownAction cdJump;
        CoolDownAction cdDash;
        CoolDownAction cdCoyote;

        Vector2 dashVelocity = default;
        Vector2 currentDirection = default;
        (bool isTouchingWall, bool isAtRight, float? distance) wallDetection;
        (float? distance, float currentVelocity) groundData = default;
        BoxCollider2D groundToLand = default;
        float dashSpeed = 0;
        Vector2 dashTarget = Vector2.zero;
        public bool shouldFall = false;

        LimitArea ground = default;
        LimitArea wall;
        struct LimitArea
        {
            public float? distance;
            public float velocity;
        }
        #endregion

        #region Unity methods
        private void Awake()
        {
            rgb2d = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            canJump = true;
            canDash = true;

            cdJump = new CoolDownAction(pSettings.JumpCoolDown, Set_CanJump);
            cdDash = new CoolDownAction(pSettings.DashCoolDown);
            cdCoyote = new CoolDownAction(pSettings.CoyoteTime, Fall);
        }

        private void Update()
        {
            Check_Ground();

            Check_Wall();
        }
        #endregion

        public bool InteractWith_Movement(float direction)
        {
            if (inDash) return false;

            if (!wallDetection.isTouchingWall || ((direction >= 0 && !wallDetection.isAtRight) || direction < 0 && wallDetection.isAtRight))
            {
                transform.Translate(direction * pSettings.Speed * Time.deltaTime, 0, 0);
                currentDirection.x = direction > 0 ? 1 : -1;
            }
            else
            {
                print("stop");
            }

            return true;
        }

        public bool InteractWith_Jump()
        {
            if (!isGrounded || inDash || !cdJump.CanUse) return false;

            (float ? distance, GameObject item) = Get_DistanceToMove(Vector2.up, pSettings.JumpSize);

            if (!distance.HasValue)
            {
                Move_Gravity();
                Jump();

                StartCoroutine(cdJump.CoolDown());
            }

            return false;
        }

        public bool InteractWithDash()
        {
            if (inDash || !canDash || !cdDash.CanUse) return false;

            (float? distance, _) = Get_DistanceToMove(currentDirection * pSettings.DashDistance, .9f);

            if (!distance.HasValue || distance >= pSettings.DashDistance)
            {
                distance = pSettings.DashDistance;
            }
            else distance += boxCollider.bounds.extents.x;

            StartCoroutine(DashTo(currentDirection.normalized * distance.Value, pSettings.DashTime));
            StartCoroutine(cdDash.CoolDown());

            return true;
        }

        public (float? distance, GameObject hit) Get_DistanceToMove(Vector2 Destiny, float size) 
        {
            RaycastHit2D[] hit2D;
            //Vector3 startPosition = Get_VectorWithOffset(Destiny, offset) + new Vector3(Destiny.x, Destiny.y) * boxCollider.bounds.extents.x;
            //Vector3 startPosition = transform.position + new Vector3(Destiny.x, Destiny.y) * boxCollider.bounds.extents.x;
            Vector3 startPosition = transform.position;
            Vector3 finalPosition = new Vector3(Destiny.x, Destiny.y, 0);
            Vector2 newSize = boxCollider.size * new Vector2
            {
                x = Destiny switch
                {
                    { x: float nx} when (nx == -1 || nx == 1) => boxCollider.size.x * size,
                    _=> .1f
                },
                y = Destiny switch
                {
                    { y: float ny } when (ny == -1 || ny == 1) => boxCollider.size.y * size,
                    _=> .1f
                }
            };

            float angle = Get_Angle(Destiny.normalized);
            bool founded = false;
            hit2D = Physics2D.BoxCastAll(startPosition, newSize, angle, finalPosition, Destiny.magnitude);

            (float? nearestDistance, GameObject item) result = default;

            if (hit2D.Length != 0)
            {
                foreach (RaycastHit2D hit in hit2D)
                {
                    if (hit.transform.gameObject == gameObject) continue;

                    if (Vector2.Angle(Destiny * -1, hit.normal) < 80)
                    {
                        if (Get_Distance(hit) is var nd && nd.HasValue && (!result.nearestDistance.HasValue || nd < result.nearestDistance))
                        {
                            result.nearestDistance = nd;
                            result.item = hit.transform.gameObject;
                        }
                    }
                    #region Debug
                    //if (gameObject.name.Contains("lanco"))
                    //    Debug.DrawLine(hit.collider.bounds.min, hit.point + hit.normal * 2, Color.red);
                    //if (Vector2.Angle(Destiny * -1, hit.normal) < 80)
                    //{
                    //    if (founded)
                    //        continue;
                    //    else
                    //        return Get_Distance(hit);
                    //} 
                    #endregion
                }
            }

            return result;

            float? Get_Distance(RaycastHit2D hit)
            {
                //if (Destiny.normalized.y == -1 && hit.collider.bounds.max.y <= boxCollider.bounds.min.y && gameObject.name.Contains("lanc"))
                //{
                //    print(hit.transform.name);
                //    Debug.DrawLine(startPosition, finalPosition, Color.blue);
                //    //Debug.DrawLine(new Vector3(startPosition.x + newSize.x, startPosition.y + newSize.y, 0), finalPosition, Color.red);
                //    //Debug.DrawLine(hit.collider.bounds.max, boxCollider.bounds.min);
                //    //Debug.DrawLine(boxCollider.bounds.min, Vector3.up * 2, Color.blue);
                //}
                //print(Destiny.normalized.y);
                return Destiny.normalized switch
                {
                    { x: -1 } when (hit.collider.bounds.max.x <= boxCollider.bounds.min.x) =>
                        boxCollider.bounds.min.x - hit.collider.bounds.max.x,

                    { x: 1 } when (hit.collider.bounds.min.x >= boxCollider.bounds.max.x) =>
                        hit.collider.bounds.min.x - boxCollider.bounds.max.x,

                    { y: -1 } when (hit.collider.bounds.max.y <= boxCollider.bounds.min.y) =>
                        boxCollider.bounds.min.y - hit.collider.bounds.max.y,

                    { y: 1 } when (hit.collider.bounds.min.y >= boxCollider.bounds.max.y) =>
                        hit.collider.bounds.min.y - boxCollider.bounds.max.y,

                    _ => null,
                };
            }
        }

        #region private methods

        private void Fall(bool shouldFall) => this.shouldFall = shouldFall;

        private void Check_Ground()
        {
            (float? distance, GameObject item) = Get_DistanceToMove(Vector2.down * pSettings.GroundDistance, pSettings.GroundSize);

            if (distance.HasValue)
            {
                if (groundToLand != null && groundToLand.gameObject != item && item.tag != "Platform")
                {
                    bool isTouching = item.GetComponent<BoxCollider2D>().IsTouching(boxCollider);
                    if (isTouching)
                    {
                        if (groundToLand != null)
                        {
                            groundToLand.isTrigger = true;
                            groundToLand = null;
                        }
                    }
                    else if (cdCoyote.CanUse)
                    {
                        if (shouldFall)
                        {
                            groundToLand.isTrigger = true;
                            groundToLand = null;
                        }
                        else
                            StartCoroutine(cdCoyote.CoolDown());
                    }
                }
                else if (groundToLand != null && groundToLand.gameObject == item)
                    groundToLand.isTrigger = false;

                shouldFall = false;

                if (groundToLand?.gameObject != item && item.tag == "Platform")
                {
                    if (groundToLand != null) groundToLand.isTrigger = true;

                    groundToLand = item.GetComponent<BoxCollider2D>();
                    item.GetComponent<BoxCollider2D>().isTrigger = false;
                    groundToLand.isTrigger = false;
                }

                if (distance < .1f) isGrounded = true;
                else isGrounded = false;
            }
            else if (groundToLand != null)
            {
                groundToLand.isTrigger = true;
                groundToLand = null;
            }
        }

        private void Check_Wall()
        {
            (float ? distance, GameObject item) = Get_DistanceToMove(currentDirection * pSettings.WallDistance, pSettings.WallSize);
            if (distance.HasValue)
            {
                if (item.tag == "Platform")
                {
                    wallDetection.isTouchingWall = false;
                    return;
                }

                wallDetection.isTouchingWall = currentDirection switch
                {
                    { x: 1 } => wallDetection.isAtRight = true,
                    { x: -1 } => !(wallDetection.isAtRight = false),
                    _ => wallDetection.isAtRight = false
                };
                print(item.name);
            }
            else
                wallDetection.isTouchingWall = false;

            isTouchingTheWall = wallDetection.isTouchingWall;
            isWallRight = wallDetection.isAtRight;
            wallDetection.distance = distance;
        }

        private float Check_Dash()
        {
            (float ? distance, _) = Get_DistanceToMove(currentDirection * pSettings.DashDistance, .9f);

            if (distance.HasValue && distance > pSettings.DashDistance)
                return distance.Value;
            else
                return pSettings.DashDistance * currentDirection.x;
        }

        private void Jump() => rgb2d.velocity = new Vector2(rgb2d.velocity.x, pSettings.JumpForce);

        private void Clear_XVelocity() => rgb2d.velocity = new Vector2(0, rgb2d.velocity.y);

        private void Clear_YVelocity() => rgb2d.velocity = new Vector2(rgb2d.velocity.x, 0);

        private void FreezeVerticalPosition(bool shouldFreeze = true) =>
            rgb2d.constraints = shouldFreeze switch
            {
                true => RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation,
                _ => RigidbodyConstraints2D.FreezeRotation
            };

        private void Set_CanJump(bool canJump) => this.canJump = canJump;

        private void Move_Gravity(bool shouldEnable = true)
        {
            rgb2d.gravityScale = shouldEnable ? 1 : 0;
            if (!shouldEnable) Clear_YVelocity();
        }
        #endregion

        #region Coroutines
        IEnumerator DashTo(Vector2 final, float dashTime)
        {
            inDash = true;
            FreezeVerticalPosition();
            Clear_XVelocity();
            Vector3 target = transform.position + new Vector3
            {
                x = final.x,
                y = final.y,
                z = 0
            };

            print("in Dash");
            //AUDIO
            //source.PlayOneShot(manager1.dash);
            float distance = Vector2.Distance(transform.position, target);

            while (distance > 0)
            {
                transform.position = Vector2.Lerp(transform.position, target, pSettings.DashSpeed * Time.deltaTime);
                distance -= pSettings.DashSpeed * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            dashVelocity = default;
            inDash = false;

            this.GetComponent<Animator>().SetBool("isDashing", false);
            Clear_XVelocity();
            FreezeVerticalPosition(false);
            yield break;
        }

        IEnumerator EnableCollition(Collider2D collider)
        {
            yield return new WaitForSeconds(2);

            Physics2D.IgnoreCollision(boxCollider, collider, false);
        }
        #endregion
    }
}
