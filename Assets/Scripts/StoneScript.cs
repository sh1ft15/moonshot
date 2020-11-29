using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoneScript : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Text text;

    void Awake(){
        if (canvas != null) { canvas.enabled = false; }
    }

    void OnTriggerEnter2D(Collider2D col){
        switch(col.tag){
            case "Cursor": canvas.enabled = true; break;
        }
    }

    void OnTriggerExit2D(Collider2D col){
        switch(col.tag){
            case "Cursor": canvas.enabled = false; break;
        }
    }

    public void SetText(string str){ 
        this.text.text = str; 
    }
}
