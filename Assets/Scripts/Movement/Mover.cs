using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xolito.Control;
using Xolito.Core;

namespace Xolito.Movement
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Mover : MonoBehaviour
    {
        //BORRAR SI ROMPE ALGO
        #region AUDIO 
        PlayerManager1 manager1;
        public AudioSource source;
        #endregion


        #region variables
        [SerializeField] PlayerSettings pSettings = null;
        Rigidbody2D rgb2d;
        BoxCollider2D boxCollider;
        BoxCollider2D colliderToAvoid = default;

        Vector2 dashVelocity = default;
        Vector2 currentDirection = default;
        bool inDash = false;
        public bool isGrounded = false;
        (bool isTouchingWall, bool isAtRight) wallDetection;

        public bool canJump = true;
        public bool testJump = false;

        bool canDash = false;
        #endregion

        private void Awake()
        {
            //AUDIO
            manager1 = GetComponent<PlayerManager1>();
           
            rgb2d = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            canJump = true;
        }

        private void Update()
        {
            Check_Ground();
            Check_Wall();

            float? test = Check_WayToMove(Vector2.up, .5f, pSettings.TagsToAvoid);
            testJump = test.HasValue;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (colliderToAvoid)
                Ignore_Collition();
        }

        public bool InteractWith_Movement(float direction)
        {
            if (inDash) return false;

            if (!wallDetection.isTouchingWall || (wallDetection.isTouchingWall && !((direction >= 0 && wallDetection.isAtRight) || direction < 0 && !wallDetection.isAtRight)))
            {
                transform.Translate(direction * pSettings.Speed * Time.deltaTime, 0, 0);
            }

            currentDirection.x = direction > 0 ? 1 : -1;

            return true;
        }

        public bool Jump()
        {
            if (!isGrounded || inDash || !canJump) return false;

            float? distance = Check_WayToMove(Vector2.up, .5f, pSettings.TagsToAvoid);

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

            return true;
        }

        public bool Dash()
        {
            if (inDash && !canDash)
                return false;

            float finalDistance = Check_Dash();

            Vector2 finalTarget = Get_FinalTarget(finalDistance);

            if (finalDistance == pSettings.DashDistance)
            {
                StartCoroutine(DashTo(finalTarget, pSettings.DashTime));
            }
            else if (finalDistance != pSettings.DashDistance)
            {
                StartCoroutine(DashTo(finalTarget, Mathf.Abs((finalDistance / pSettings.DashDistance) * pSettings.DashTime)));
            }

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
            float? distance = Check_WayToMove(currentDirection, isGrounded? .1f : .3f, pSettings.TagsToAvoid);

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
            float? distance = Check_WayToMove(currentDirection, .1f);

            if (distance.HasValue)
                return distance.Value;
            else
                return pSettings.DashDistance * currentDirection.x;
        }

        public float? Check_WayToMove(Vector2 direction, float offset = .1f, List<string> tagsToAvoid = null)
        {
            RaycastHit2D[] hit2D;

            Vector2 offset1 = direction * offset;
            Vector3 startPosition = new Vector3()
            {
                x = transform.position.x + direction.x * offset,
                y = transform.position.y + direction.y * offset,
                z = 0
            };

            float angle = Utilities.Utilities.Get_AngleDirection(direction);

            hit2D = Physics2D.BoxCastAll(startPosition, boxCollider.size, angle, direction, offset);

            if (hit2D.Length != 0)
            {
                foreach (RaycastHit2D hit in hit2D)
                {
                    if (tagsToAvoid != null && tagsToAvoid.Count != 0)
                    {
                        bool founded = Utilities.Utilities.Find_TagIn(tagsToAvoid, hit.transform.tag);

                        Check_Collider(hit, founded);
                        if (founded) continue;
                    }

                    if (hit.transform.gameObject == gameObject) continue;

                    if (Vector2.Angle(direction * -1, hit.normal) < 30)
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
                //else if (founded && colliderToAvoid == hit.transform.GetComponent<BoxCollider2D>())
                //    colliderToAvoid = hit.transform.GetComponent<BoxCollider2D>();
                else if (!founded && colliderToAvoid)
                    colliderToAvoid = default;
            }
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

        public void Ignore_Collition(bool shouldIgnore = true)
        {
            Physics2D.IgnoreCollision(boxCollider, colliderToAvoid, shouldIgnore);

            if (!shouldIgnore)
                colliderToAvoid = default;
        }

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
            Clear_XVelocity();
            FreezeVerticalPosition(false);

            StartCoroutine(Disable_Dash());
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
        #endregion
    }
}
