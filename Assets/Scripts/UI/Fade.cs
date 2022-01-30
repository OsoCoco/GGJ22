using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xolito.Core;

namespace Xolito.UI
{
    public class Fade : MonoBehaviour
    {
        [SerializeField] float time = 1.5f;

        MenuManager menuManager;
        Image image;
        bool canFade = true;

        private void Awake()
        {
            menuManager = GameObject.FindObjectOfType<MenuManager>();
            image = GetComponent<Image>();
        }

        private void Start()
        {
            canFade = true;
        }

        public void FadeIn()
        {
            if (canFade)
            {
                image.color = Vector4.Lerp(image.color, new Vector4(1, 1, 1, 1), time);
                StartCoroutine(DisableFade());
            }
        }

        public void FadeOut()
        {
            if (canFade)
            {
                image.color = Vector4.Lerp(image.color, new Vector4(1, 1, 1, 0), time);
                StartCoroutine(DisableFade());
            }
        }

        IEnumerator DisableFade()
        {
            canFade = false;
            yield return new WaitForSecondsRealtime(time);

            canFade = true;
        }
    } 
}
