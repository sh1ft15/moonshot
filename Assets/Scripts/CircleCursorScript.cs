using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleCursorScript : MonoBehaviour
{   
    [SerializeField] CirclesScript circlesScript;
    [SerializeField] NumUIScript numUIScript;
    [SerializeField] PlayerScript playerScript;
    Transform hilitedBar;

    void OnTriggerEnter2D(Collider2D col){
        if (playerScript.IsCasting() && col.tag == "CircleBar") { 
            circlesScript.HiliteBar(col.transform); 
            hilitedBar = col.transform;
        }
    }

    void OnTriggerStay2D(Collider2D col){
        if (playerScript.IsCasting() && col.tag == "CircleBar" && hilitedBar != col.transform) { 
            circlesScript.HiliteBar(col.transform); 
        }
    }

    void OnTriggerExit2D(Collider2D col){
        if (playerScript.IsCasting() && col.tag == "CircleBar") { 
            circlesScript.DehiliteBar(col.transform);
            hilitedBar = null; 
            numUIScript.ToggleCursor(false);
        }
    }

    void OnCollisionEnter2D(Collision2D col){
        if (col != null) {
            switch(col.transform.tag){
                case "Mob":
                    Transform origin = col.transform.Find("Character");

                    Physics2D.IgnoreCollision(transform.GetComponent<Collider2D>(), 
                        origin.GetComponent<Collider2D>());
                break;
                default:
                    Physics2D.IgnoreCollision(transform.GetComponent<Collider2D>(), col.collider);
                break;
            }
        }
    }
}


        // if (playerScript.IsCasting() && col.tag == "DirCursor") { 
        //     circlesScript.DehiliteAllBars();
        //     playerScript.SetCasting(false); 
        // }


        // Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);