using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCScript : MonoBehaviour
{
    [SerializeField] DialogUIScript dialogUIScript;
    Canvas canvas;
    List<string> dialogs;
    
    void Start(){
        canvas = transform.Find("Canvas").GetComponent<Canvas>();
        dialogs = new List<string>(){
            "Hello there, I'm Floor Master. I take care of the floors... I guess",
            "We have total of 3 floors that you can access right now",
            "You can 'Left-Click' on the stones up there, it will prompt you of your destination",
            "I'll recommend working your way from left to right",
            "You can talk to the 'Dummy' if you need some combat training"
        };

        if (canvas != null) { canvas.enabled = false; }
    }

    void OnTriggerEnter2D(Collider2D col){
        switch(col.tag){
            case "Cursor": 
                canvas.enabled = true; 
            break;
        }
    }

    void OnTriggerExit2D(Collider2D col){
        switch(col.tag){
            case "Cursor": canvas.enabled = false; break;
        }
    }

    public void PromptDialogs(){
        dialogUIScript.ToggleCanvas(true);
        dialogUIScript.SetSequenceTexts(dialogs);
        dialogUIScript.SetBackText("Back");
        dialogUIScript.SetNextText("Next");
        dialogUIScript.SetContext("Dialog");
    }
}
