using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour
{
    [SerializeField] EffectsScript effectsScript;

    void OnTriggerEnter2D(Collider2D col){
        switch(col.tag){
            case "SmallProjectile": 
                effectsScript.StartCoroutine(effectsScript.SpawnSmallEffect("small_explosion", col.transform.position, 1));
                Destroy(col.gameObject);
            break;
            case "MediumProjectile": 
                effectsScript.StartCoroutine(effectsScript.SpawnSmallEffect("medium_explosion", col.transform.position, 1));
                Destroy(col.gameObject);
            break;
            case "EmmiterProjectile": 
                Destroy(col.gameObject);
            break;
        }
        // Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
        // effectsScript.StartCoroutine(effectsScript.SpawnSmallEffect("small_explosion", col.transform.position, 1));
        
    }
}
