using System.Collections.Generic;
using UnityEngine;
using Xolito.Core;

namespace Xolito.Control
{
    [System.Serializable]
    public class Audio
    {
        [SerializeField]private AudioClip clip;
        [SerializeField]private Core.BasicActions action;

        public AudioClip Clip { get => clip; }
        public Core.BasicActions Action { get => action; }

        public Audio(AudioClip clip, Core.BasicActions action)
        {
            this.clip = clip;
            this.action = action;
        }

        public void Deconstruct(out AudioClip clip, out Core.BasicActions action)
        {
            clip = this.clip;
            action = this.action;
        }
    }

    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "XolitoSettings/Player Settings", order = 0)]
    public class PlayerSettings : ScriptableObject
    {
        #region variables
        [Header("General")]
        [SerializeField] List<string> tagsToAvoid = default;

        [Space]
        [Header("Player Stats")]
        [Range(0, 6)]
        [SerializeField] float gravity = 3;
        [SerializeField] float speed = 5;
        [SerializeField] float jumpForce = 5;
        [SerializeField] float jumpCoolDown = 1f;

        [Space]
        [Header("Ground")]
        [Header("Collition Settings")]
        [Range(0, 2)]
        [SerializeField] float groundDistance = .1f;
        [Range(0, 2)]
        [SerializeField] float groundSize = .1f;
        [SerializeField] float coyoteTime = 1f;
        [Header("Wall")]
        [Range(0, 1)]
        [SerializeField] float wallDistance = .1f;
        [Range(0, 2)]
        [SerializeField] float wallSize = .1f;
        [Header("Jump")]
        [Range(0, 2)]
        [SerializeField] float jumpSize = .1f;
        [Header("Dash")]
        [Range(0, 1)]
        [SerializeField] float dashOffset = .1f;
        [Space]
        [Header("Dash Settings")]
        [SerializeField] float dashDistance = 5;
        [SerializeField] float dashSpeed = 5;
        [SerializeField] float dashTime = 3;
        [SerializeField] float dashCoolDown = 1f;

        [Header("Audio")]
        [SerializeField] List<Audio> audios = null;
        #endregion

        #region Properties
        public float Gravity { get => gravity * Time.deltaTime; }
        public float Speed { get => speed; }
        public float JumpForce { get => jumpForce; }
        public float JumpCoolDown { get => jumpCoolDown; }
        public List<string> TagsToAvoid { get => tagsToAvoid; }

        public float GroundDistance { get => groundDistance; }
        public float GroundSize { get => groundSize; }
        public float CoyoteTime { get => coyoteTime; }
        public float WallDistance { get => wallDistance; }
        public float WallSize { get => wallSize; }
        public float JumpSize { get => jumpSize; }

        public float DashDistance { get => dashDistance; }
        public float DashSpeed { get => dashSpeed; }
        public float DashTime { get => dashTime; }
        public float DashCoolDown { get => dashCoolDown; }
        #endregion

        public AudioClip Get_Audio(Core.BasicActions action)
        {
            foreach (Audio audio in audios)
            {
                if (audio.Action == action)
                {
                    return audio.Clip;
                }
            }

            return null;
        }
    }
}