using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogUIScript : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Text text, backText, nextText;
    [SerializeField] GameObject backBtn, nextBtn;
    [SerializeField] Image overlayImage;
    List<string> sequenceTexts;
    
    int curIndex = 0;
    string context = "";
    bool inTransition = false;

    void Awake(){ 
        sequenceTexts = new List<string>();

        if (canvas != null) { canvas.enabled = false; } 
    }

    public void ToggleCanvas(bool status) { canvas.enabled = status;}

    public void SetText(string str){ text.text = str; }

    public void SetNextText(string str){ 
        nextBtn.SetActive(str.Length > 0);
        nextText.text = str; 
    }

    public void SetBackText(string str){ 
        backBtn.SetActive(str.Length > 0);
        backText.text = str; 
    }

    public bool HasNextBtn() { return nextBtn.activeSelf; }

    public bool HasBackBtn() { return backBtn.activeSelf; }

    public void SetContext(string str) { context = str; }

    public void SetSequenceTexts(List<string> strs) { 
        sequenceTexts = strs; 
        curIndex = 0;
        SetText(sequenceTexts[curIndex]);
    }

    public void IterateSequence(int direction){
        int index = curIndex + direction;
        
        if (index < 0 || index >= sequenceTexts.Count) { ToggleCanvas(false); }
        else {
            curIndex = index;
            SetText(sequenceTexts[index]);
        }
    }

    public bool IsActive() { return canvas.enabled; }

    public bool IsLastIndex() { return curIndex == (sequenceTexts.Count - 1); }

    public bool InTransition() { return inTransition; }

    public string GetContext() { return context; }

    public IEnumerator ChangeScene(float delay = 1){
        if (inTransition == false) {
            Color originalColor = new Color(.2f, .2f, .2f, 0.1f),
                  newColor = new Color(.2f, .2f, .2f);
            float t = 0,
                duration = 0.2f;

            inTransition = true;

            while (t <= 1.0) {
                t += Time.deltaTime / duration;
                overlayImage.color = Color.Lerp(originalColor, newColor, t);
                yield return null;
            }

            yield return new WaitForSeconds(delay);
            t = 0;

            while (t <= 1.0) {
                t += Time.deltaTime / duration;
                overlayImage.color = Color.Lerp(newColor, originalColor, t);
                yield return null;
            }

            ToggleCanvas(false);
            inTransition = false;
        }
    }
}
