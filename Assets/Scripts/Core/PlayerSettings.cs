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

        [Header("Audio")]
        [SerializeField] List<Audio> audios = null;
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