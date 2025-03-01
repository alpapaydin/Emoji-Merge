using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Start()
    {
        SoundManager.Instance.PlayBGM("MenuBGM");
    }

    public void PlaySound(string soundName)
    {
        SoundManager.Instance.PlaySound(soundName);
    }

    public void IdleAnimation()
    {
        animator.Play("MenuIdle");
    }
}
