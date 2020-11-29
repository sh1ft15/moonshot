using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldScript : MonoBehaviour
{
    [SerializeField] Transform effectsPrefab;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] List<Sprite> sprites;
    List<GameObject> colliders;
    Animator effectsAnimator;
    Coroutine endLifeCoroutine;
    Sprite curSprite;
    Transform origin;
    float life = 0;
    bool isAlive = false;

    void Awake(){
        // effects
        Transform effects = Instantiate(effectsPrefab, Vector2.zero, Quaternion.identity);

        effects.parent = transform;
        effects.localPosition = Vector2.zero;
        effectsAnimator = effects.GetComponent<Animator>();

        // colliders (25, 50, 75, 100)
        colliders = new List<GameObject>();

        foreach(Transform child in transform.Find("Colliders")){ colliders.Add(child.gameObject); }
    }

    // Update is called once per frame
    void Update(){
        if (isAlive == false) { return; }

        if (life > 0) { life -= Time.deltaTime;  }
        else { 
            if (endLifeCoroutine == null){ endLifeCoroutine = StartCoroutine(EndLife()); }
        }
    }

    // shield coverage: 25, 50, 75, 100
    public void SetCoverage(string coverage){
        GameObject collider = colliders.Find(col => col.name.Equals(coverage));
        Sprite tempSprite = sprites.Find(sp => sp.name.Equals("shield_" + coverage));

        if (collider != null && tempSprite != null) {
            // ensure all colliders are disable by default
            foreach(GameObject child in colliders){ 
                if (child == collider) { 
                    if (collider.activeSelf == false) { child.SetActive(true); } 
                }
                else { if (child.activeSelf) { child.SetActive(false); } }
            }

            curSprite = tempSprite;
        }
    }

    public void IgnoreCollision(Collider2D collider){
        origin = collider.transform;

        foreach(GameObject child in colliders){ 
            Physics2D.IgnoreCollision(child.GetComponent<Collider2D>(), collider);
        }
    }

    public IEnumerator EndLife(){
        if (isAlive){
            // blinking effect on sprite
            for(int i = 0; i < 3; i++){
                spriteRenderer.color = new Color(1, 1, 1, 0.1f);

                yield return new WaitForSeconds(0.2f);

                spriteRenderer.color = new Color(1, 1, 1);

                yield return new WaitForSeconds(0.2f);
            }

            spriteRenderer.color = new Color(1, 1, 1, 0);
            animator.SetBool("active", false);
            yield return new WaitForSeconds(0.1f);
            
            effectsAnimator.SetTrigger("small_charge");

            yield return new WaitForSeconds(0.5f);
            spriteRenderer.sprite = null;
            endLifeCoroutine = null;
            isAlive = false;
        }
    }

    public void SetLife(float num = 0){
        life = num;
    }

    public IEnumerator ToggleLife(bool status){
        if (status) { 
            if (endLifeCoroutine != null) { 
                StopCoroutine(endLifeCoroutine); 
                endLifeCoroutine = null;
            }

            animator.Play("idle"); // reset animation
            effectsAnimator.SetTrigger("small_charge");
            spriteRenderer.color = Color.white;

            yield return new WaitForSeconds(0.5f);
            animator.SetBool("active", true);
            spriteRenderer.sprite = curSprite;
        }

        isAlive = status;
    }

    public bool IsAlive() { return isAlive; }

    public Transform GetOrigin() { return origin; }
}
