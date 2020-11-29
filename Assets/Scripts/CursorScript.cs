using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    [SerializeField] CirclesScript circlesScript;
    [SerializeField] Transform targetCursor;
    Transform target;

    // Update is called once per frame
    void Update(){
        Vector2 post = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        transform.position = post;

        if (target != null){ targetCursor.position = target.position; }
        else {
            if (targetCursor.gameObject.activeSelf) { 
                targetCursor.gameObject.SetActive(false); 
            }
        }  
    }

    public void SetTarget(Transform obj){
        if (obj != null){
            if (obj == transform) { return; }
            if (obj == target) { target = null; }
            else { target = obj; }

            targetCursor.gameObject.SetActive(target != null);
            circlesScript.SetTarget(target);
        }
    }
}
