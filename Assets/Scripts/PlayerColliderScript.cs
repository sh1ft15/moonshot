using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColliderScript : MonoBehaviour
{   
    PlayerScript playerScript;
    EffectsScript effectsScript;

    void Awake(){
        effectsScript = GameObject.Find("Effects").GetComponent<EffectsScript>();
        playerScript = transform.parent.GetComponent<PlayerScript>();
    }
    
    void OnTriggerEnter2D(Collider2D col){
        Vector3 post = col.transform.position;

        switch(col.tag){
            case "SmallProjectile": 
            case "MediumProjectile": 
                SmallProjectileScript script = col.GetComponent<SmallProjectileScript>();
                Transform origin = script.GetOrigin();
                bool isSmall = col.tag.Equals("SmallProjectile");
                int damage = (int) script.GetDamage();

                if (origin != transform) {
                    TriggerDamage(damage);
                    effectsScript.StartCoroutine(effectsScript.SpawnSmallEffect(
                        isSmall? "small_explosion" : "medium_explosion", post, 1));
                    Destroy(col.gameObject);
                }
            break;
            case "BigEffect": 
                SmallEffectScript effScript = col.GetComponent<SmallEffectScript>();
  
                if (effScript.GetOrigin() != transform) {  TriggerDamage((int) effScript.GetDamage()); }
            break;
            case "EmmiterProjectile": 
                Destroy(col.gameObject);
            break;
        }
    }

    void TriggerDamage(int damage){
        playerScript.StartCoroutine(playerScript.LerpScale(new Vector2(1.1f, 0.9f), 
            new Color(1, .3f, .3f), 0.15f));
        playerScript.SpawnParticle(new Color(1, 0, 0));
        playerScript.UpdateHealth(-damage);
        effectsScript.StartCoroutine(effectsScript.SpawnRateEffect(
            transform.position, -damage, 1));
    }
}
