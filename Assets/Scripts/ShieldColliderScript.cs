using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldColliderScript : MonoBehaviour
{
    EffectsScript effectsScript;
    ShieldScript shieldScript;
    Transform parent;

    void Awake(){
        Transform shieldContainer = transform.parent.parent;

        effectsScript = GameObject.Find("Effects").GetComponent<EffectsScript>();
        shieldScript = shieldContainer.GetComponent<ShieldScript>();
    }

    void OnTriggerEnter2D(Collider2D col){
        Vector3 post = col.transform.position;

        if (!shieldScript.IsAlive() || col == null) { return; }

        switch(col.tag){
            case "SmallProjectile": 
            case "MediumProjectile": 
            case "BigEffect":
                SmallProjectileScript script = col.GetComponent<SmallProjectileScript>();
                Transform origin = script.GetOrigin(),
                          curOrigin = shieldScript.GetOrigin();

                if (origin != null && curOrigin != null) {
                    string tag = origin.parent.tag,
                           curTag = curOrigin.parent.tag;
                           
                    if (origin != curOrigin && tag != curTag) {
                        effectsScript.StartCoroutine(effectsScript.SpawnSmallEffect(
                            "small_deflect", post, 1));
                        Destroy(col.gameObject);
                    }
                }
            break;
            case "EmmiterProjectile": 
                Destroy(col.gameObject);
            break;
        }
    }

    void OnCollisionEnter2D(Collision2D col){
        if (col != null) {
            Collider2D collider = transform.GetComponent<Collider2D>();

            switch(collider.tag){
                case "Mob":
                    MobScript script = col.transform.GetComponent<MobScript>();
                    Transform origin = shieldScript.GetOrigin(),
                              colOrigin = col.transform.Find("Character");

                    if (script.IsAttacking() && origin.tag != collider.tag) {
                        if (shieldScript.IsAlive()) { 
                            effectsScript.StartCoroutine(effectsScript.SpawnSmallEffect(
                                "small_deflect", col.contacts[0].point, 1));
                        }
                        else { Physics2D.IgnoreCollision(collider, colOrigin.GetComponent<Collider2D>()); }
                    }
                break;
                default:
                    if (!shieldScript.IsAlive()) { 
                        Physics2D.IgnoreCollision(collider, col.collider);
                    }
                break;
            }
        }
    }
}
