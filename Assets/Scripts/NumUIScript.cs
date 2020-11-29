using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumUIScript : MonoBehaviour
{
    [SerializeField] RectTransform cursor;
    [SerializeField] Text cursorText;
    [SerializeField] List<RectTransform> numContainers;
    RectTransform numsContainer;
    Color defColor;
    bool isVisible = false;

    void Awake(){
        defColor = new Color(1, 1, 1, 0.5f);
        numsContainer = transform.Find("Nums").GetComponent<RectTransform>();
        SetVisible(false);
    }

    public void MoveCursor(int index){
        cursor.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 50 * index, 50);
        // cursor.offsetMin = new Vector2(50 + (index + 1), cursor.offsetMin.y);
    }

    public void SetCursorNum(int num){
        cursorText.text = num.ToString();
    }

    public void SetNum(int index, string text = ""){
        numContainers[index].Find("Text").GetComponent<Text>().text = text;
    }

    public void HiliteNum(int index){
        numContainers[index].Find("Image").GetComponent<Image>().color = Color.green;
    }

    public void DehiliteNum(int index){
        numContainers[index].Find("Image").GetComponent<Image>().color = defColor;
    }

    public void HiliteCursor(){
        cursor.Find("Image").GetComponent<Image>().color = Color.green;
    }

    public void DehiliteCursor(){
        cursor.Find("Image").GetComponent<Image>().color = Color.white;
    }

    public void ToggleCursor(bool status){
        cursor.gameObject.SetActive(status);
    }

    public void SetVisible(bool status){
        isVisible = status;
        numsContainer.gameObject.SetActive(status);
    }

    public bool IsVisible() { return isVisible; }
}
