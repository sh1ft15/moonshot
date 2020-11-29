using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallEffectScript : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] AudioSource audioSource;
    [SerializeField] float damage;
    Transform origin;

    public void SetOrigin(Transform origin){
        this.origin = origin;
    }

    public void TriggerAnim(string name){
        animator.SetTrigger(name);
    }

    public void SetColor(Color color){
        spriteRenderer.color = color;
    }

    public Transform GetOrigin() { return this.origin; }

    public float GetDamage() { return this.damage; }

    public void PlayClip(AudioClip clip){ 
        audioSource.PlayOneShot(clip);
    }
}
