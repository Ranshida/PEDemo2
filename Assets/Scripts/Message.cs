using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message : MonoBehaviour
{
    public Animator Anim;

    private void Start()
    {
        Anim = this.GetComponent<Animator>();
    }

    public void OnPointerEnter()
    {
        Anim.SetBool("Stay", true);
    }
    public void OnPointerExit()
    {
        Anim.SetBool("Stay", false);
    }

    public void DestroyMessage()
    {
        Destroy(this.gameObject);
    }
}
