using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xolito.Control;
using Xolito.Core;
using static Xolito.Utilities.Utilities;

namespace Xolito.Movement
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Mover : MonoBehaviour
    {
        #region variables
        [SerializeField] PlayerSettings pSettings = null;
        Rigidbody2D rgb2d;
        BoxCollider2D boxCollider;
        BoxCollider2D colliderToAvoid = default;

        Vector2 dashVelocity = default;
        Vector2 currentDirection = default;
        public bool inDash = false;
        public bool isGrounded = false;
        (bool isTouchingWall, bool isAtRight) wallDetection;

        //GroundCheck
        public bool isBesidePlatform = false;
        public bool isWallRight = false;
        public float angleOfContact = 0;

        public bool canJump = true;
        bool testJump = false;

        public bool canDash = false;
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
        }

        private void Update()
        {
            //if (isGrounded)
            //    Check_Ground();
            //else
            //    Check_GoundWithLines();
            //Check_GoundWithLines();
            Check_Ground();


            Check_Wall();
        }

        //private void OnCollisionEnter2D(Collision2D collision)
        //{
        //    if (colliderToAvoid)
        //        Ignore_Collition();
        //}

        //private void OnCollisionStay2D(Collision2D collision)
        //{
        //    try
        //    {
        //        if (collision.gameObject.CompareTag("Floor"))
        //        {
        //            if (collision.GetContact(0).normal == Vector2.up)
        //            {
        //                isGrounded = true;
        //            }
        //            else if (collision.GetContact(0).normal == Vector2.right)
        //                wallDetection = (true, true);
        //            else if (collision.GetContact(0).normal == Vector2.left)
        //                wallDetection = (true, false);
        //        }
        //        else if (collision.gameObject.CompareTag("Platform"))
        //        {
        //            if (collision.GetContact(0).normal == Vector2.down)
        //            {
        //                Ignore_Collition(collision.collider, false);
        //                isGrounded = true;
        //            }
        //            else if (collision.GetContact(0).normal == Vector2.right)
        //            {
        //                wallDetection = (true, true);
        //                Ignore_Collition(collision.collider, true);
        //                StartCoroutine(EnableCollition(collision.collider));
        //            }
        //            else if (collision.GetContact(0).normal == Vector2.left)
        //            {
        //                wallDetection = (true, true);
        //                Ignore_Collition(collision.collider, true);
        //                StartCoroutine(EnableCollition(collision.collider));
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}
        //private void OnCollisionExit2D(Collision2D collision)
        //{
        //    try
        //    {
        //        if (collision.gameObject.CompareTag("Floor"))
        //        {
        //            if (collision.GetContact(0).normal == Vector2.up)
        //            {
        //                StartCoroutine(ExitGround());
        //            }
        //            else if (collision.GetContact(0).normal == Vector2.right)
        //                wallDetection = (false, true);
        //            else if (collision.GetContact(0).normal == Vector2.left)
        //                wallDetection = (false, false);
        //        }
        //        else if (collision.gameObject.CompareTag("Platform"))
        //        {
        //            if (collision.GetContact(0).normal == Vector2.up)
        //            {
        //                //Ignore_Collition(collision.collider, false);
        //                //isGrounded = true;
        //            }
        //            else if (collision.GetContact(0).normal == Vector2.right)
        //            {
        //                wallDetection = (false, true);
        //            }
        //            else if (collision.GetContact(0).normal == Vector2.left)
        //            {
        //                wallDetection = (false, true);
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawCube(transform.position + Vector3.up, new Vector3(boxCollider.size.x * 1f, boxCollider.size.y * 1f, 1));
            //Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, new Vector3(boxCollider.size.x * 1f, boxCollider.size.y * 1f, 1));
        }
        #endregion

        public bool InteractWith_Movement(float direction)
        {
            if (inDash) return false;

            if (!wallDetection.isTouchingWall || (wallDetection.isTouchingWall && !((direction >= 0 && wallDetection.isAtRight) || direction < 0 && !wallDetection.isAtRight)))
            {
                transform.Translate(direction * pSettings.Speed * Time.deltaTime, 0, 0);
            }

            currentDirection.x = direction > 0 ? 1 : -1;

            //transform.Translate(direction * pSettings.Speed * Time.deltaTime, 0, 0);

            return true;
        }

        public bool InteractWith_Jump()
        {
            if (!isGrounded || inDash || !canJump) return false;

            rgb2d.AddForce(Vector2.up * pSettings.JumpForce, ForceMode2D.Impulse);
            rgb2d.velocity = new Vector2(rgb2d.velocity.x, pSettings.JumpForce);

            float? distance = Check_WayToMove(Vector2.up, 1, .1f, pSettings.TagsToAvoid);

            if (colliderToAvoid)
            {
                //AUDIO
                //source.PlayOneShot(manager1.jump);

                //Debug.Log("Jumping");

                rgb2d.velocity = new Vector2(rgb2d.velocity.x, pSettings.JumpForce);

                Ignore_Collition();
                return true;
            }

            if (!distance.HasValue)
                rgb2d.velocity = new Vector2(rgb2d.velocity.x, pSettings.JumpForce);
            else
                return false;

            Jump();

            return true;
        }

        public bool InteractWithDash()
        {
            if (inDash || !canDash)
                return false;

            //float finalDistance = Check_Dash();

            //Vector2 finalTarget = Get_FinalTarget(finalDistance);

            //if (finalDistance == pSettings.DashDistance)
            //{
            //    StartCoroutine(DashTo(finalTarget, pSettings.DashTime));
            //}
            //else if (finalDistance != pSettings.DashDistance)
            //{
            //    StartCoroutine(DashTo(finalTarget, Mathf.Abs((finalDistance / pSettings.DashDistance) * pSettings.DashTime)));
            //}

            StartCoroutine(DashTo(currentDirection * pSettings.DashDistance, pSettings.DashTime));
            StartCoroutine(Disable_Dash());

            return true;

            Vector2 Get_FinalTarget(float finalDistance)
            {
                Vector2 finalTarget;
                if (pSettings.DashDistance < finalDistance)
                    finalTarget = new Vector2(transform.position.x + pSettings.DashDistance * currentDirection.x, transform.position.y);
                else
                    finalTarget = new Vector2(transform.position.x + finalDistance + (boxCollider.size.x * 2) * currentDirection.x, transform.position.y);
                return finalTarget;
            }
        }

        public void ChechkFloor()
        {

        }

        void Check_Ground()
        {
            float? distance = default;
            distance = Check_WayToMove(Vector2.down * .1f, .9f);

            if (distance.HasValue)
            {
                //if (!isGrounded && canJump)
                //    StartCoroutine(Disable_Jump());

                if (distance.Value <= .1f)
                {
                    isGrounded = true;
                    StartCoroutine(Disable_Jump());
                }

                if (colliderToAvoid && (boxCollider.transform.position.y - boxCollider.bounds.extents.y) > (colliderToAvoid.transform.position.y + colliderToAvoid.bounds.extents.y))
                    Ignore_Collition(false);

                //if ()
            }
            else
                isGrounded = false;
        }

        void Check_GoundWithLines()
        {
            Vector3 startL = new Vector3()
            {
                x = transform.position.x - boxCollider.bounds.extents.x,
                y = transform.position.y - boxCollider.bounds.extents.y,
                z = transform.position.z
            };

            Vector3 startR = startL + Vector3.right * boxCollider.bounds.extents.x * 2f;

            if (!Cast_Ground(startL))
                Cast_Ground(startR);

            bool Cast_Ground(Vector3 start)
            {
                RaycastHit2D[] hits;
                Debug.DrawRay(start, Vector3.down * .2f, Color.red, .1f);

                hits = Physics2D.RaycastAll(start, Vector3.down, .2f);

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.transform.name == transform.name) continue;

                    if (boxCollider.transform.position.y - boxCollider.bounds.extents.y < hit.point.y)
                        continue;

                    if (colliderToAvoid == hit.collider)
                        Ignore_Collition(false);

                    isGrounded = true;
                    StartCoroutine(Disable_Jump());

                    return true;
                }

                return false;
            }
        }

        void Check_Wall()
        {
            float? distance = Check_WayToMove(currentDirection * .1f, .9f, .1f, pSettings.TagsToAvoid);

            if (colliderToAvoid)
                wallDetection.isTouchingWall = false;

            if (distance.HasValue && Mathf.Abs(distance.Value) <= 0)
                wallDetection.isTouchingWall = true;
            else if (!distance.HasValue)
                wallDetection.isTouchingWall = false;

            wallDetection.isAtRight = (currentDirection.x >= 0);
        }

        float Check_Dash()
        {
            float? distance = Check_WayToMove(currentDirection * pSettings.DashDistance, .9f);

            if (distance.HasValue)
                return distance.Value;
            else
                return pSettings.DashDistance * currentDirection.x;
        }

        public float? Check_WayToMove(Vector2 Destiny, float size, float offset = .1f, List<string> tagsToAvoid = null)
        {
            RaycastHit2D[] hit2D;
            Vector3 startPosition = Get_VectorWithOffset(Destiny, offset);
            Vector3 finalPosition = startPosition + new Vector3(Destiny.x, Destiny.y, 0);

            float angle = Get_Angle(Destiny.normalized);
            hit2D = Physics2D.BoxCastAll(startPosition, boxCollider.size * size, angle, finalPosition, Destiny.magnitude);

            if (hit2D.Length != 0)
            {
                foreach (RaycastHit2D hit in hit2D)
                {
                    if (hit.transform.gameObject == gameObject) continue;

                    if (tagsToAvoid != null && tagsToAvoid.Count != 0)
                    {
                        bool founded = Find_TagIn(tagsToAvoid, hit.transform.tag);

                        Check_Collider(hit, founded);
                        if (founded) continue;
                    }


                    if (Vector2.Angle(Destiny * -1, hit.normal) < 80)
                    {
                        float distance = Destiny.x != 0 ? hit.distance * Destiny.normalized.x : hit.distance * Destiny.normalized.y;
                        return distance;
                    }
                }
            }

            return null;

            void Check_Collider(RaycastHit2D hit, bool founded)
            {
                if (founded && !colliderToAvoid)
                    colliderToAvoid = hit.transform.GetComponent<BoxCollider2D>();
                //else if (founded && colliderToAvoid == hit.transform.GetComponent<BoxCollider2D>())
                //    colliderToAvoid = hit.transform.GetComponent<BoxCollider2D>();
                else if (!founded && colliderToAvoid)
                    colliderToAvoid = default;
            }
        }

        #region private methods
        private void Jump()
        {
            rgb2d.velocity = new Vector2(rgb2d.velocity.x, pSettings.JumpForce);
        }

        private Vector3 Get_VectorWithOffset(Vector2 finalPosition, float offset)
        {
            Vector3 startPosition = new Vector3()
            {
                x = transform.position.x + finalPosition.normalized.x * offset,
                y = transform.position.y + finalPosition.normalized.y * offset,
                z = 0
            };
            return startPosition;
        }

        private void Clear_XVelocity() => rgb2d.velocity = Vector2.zero;

        private void FreezeVerticalPosition(bool shouldFreeze = true)
        {
            if (shouldFreeze)
            {
                rgb2d.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                //rgb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            else if (!shouldFreeze)
                rgb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Ignore_Collition(bool shouldIgnore = true)
        {
            if (!colliderToAvoid) return;

            Physics2D.IgnoreCollision(boxCollider, colliderToAvoid, shouldIgnore);

            if (!shouldIgnore)
                colliderToAvoid = null;
        }

        private void Ignore_Collition(Collider2D collider, bool shouldIgnore)
        {
            Physics2D.IgnoreCollision(boxCollider, collider, shouldIgnore);
        }
        #endregion

        #region Coroutines
        IEnumerator DashTo(Vector2 final, float dashTime)
        {
            inDash = true;
            FreezeVerticalPosition();
            Clear_XVelocity();

            transform.position = Vector2.SmoothDamp(transform.position, final, ref dashVelocity, dashTime, pSettings.DashSpeed);
            float currentTime = 0;

            //AUDIO
            //source.PlayOneShot(manager1.dash);

            while (currentTime < dashTime)
            {
                transform.position = Vector2.SmoothDamp(transform.position, final, ref dashVelocity, dashTime, pSettings.DashSpeed);
                yield return new WaitForEndOfFrame();
                currentTime += Time.deltaTime;
            }

            dashVelocity = default;
            inDash = false;


            this.GetComponent<Animator>().SetBool("isDashing", false);
            Clear_XVelocity();
            FreezeVerticalPosition(false);
            yield break;
        }

        IEnumerator Disable_Dash()
        {
            canDash = false;
            yield return new WaitForSeconds(pSettings.DashCoolDown);

            canDash = true;
        }

        IEnumerator Disable_Jump()
        {
            canJump = false;
            yield return new WaitForSeconds(pSettings.JumpCoolDown);

            if (isGrounded)
                canJump = true;
        }

        IEnumerator ExitGround()
        {
            yield return new WaitForSeconds(0.2f);
            isGrounded = false;
        }

        IEnumerator EnableCollition(Collider2D collider)
        {
            yield return new WaitForSeconds(2);

            Physics2D.IgnoreCollision(boxCollider, collider, false);
        }
        #endregion
    }
}
