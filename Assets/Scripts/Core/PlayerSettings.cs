using System.Collections.Generic;
using UnityEngine;

namespace Xolito.Control
{
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "PlayerSettings/New Settings", order = 0)]
    public class PlayerSettings : ScriptableObject
    {
        #region variables
        [Header("General")]
        [SerializeField] List<string> tagsToAvoid = default;

        [Header("Player Stats")]
        [SerializeField] float speed = 5;
        [SerializeField] float jumpForce = 5;
        [SerializeField] float jumpCoolDown = 1f;

        [Space]
        [Header("Dash Settings")]
        [SerializeField] float dashDistance = 5;
        [SerializeField] float dashSpeed = 5;
        [SerializeField] float dashTime = 3;
        [SerializeField] float dashCoolDown = 1f;
        #endregion

        #region Properties
        public float Speed { get => speed; }
        public float JumpForce { get => jumpForce; }
        public float JumpCoolDown { get => jumpCoolDown; }
        public List<string> TagsToAvoid { get => tagsToAvoid; }

        public float DashDistance { get => dashDistance; }
        public float DashSpeed { get => dashSpeed; }
        public float DashTime { get => dashTime; }
        public float DashCoolDown { get => dashCoolDown; }
        #endregion
    }
}