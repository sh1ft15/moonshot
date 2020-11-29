using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIBoxScript : MonoBehaviour
{
    [SerializeField] Image skillImage, labelImage;
    [SerializeField] Text labelText, slotText;

    public void SetSkillImage(Sprite sprite){
        this.skillImage.sprite = sprite;
    }

    public void SetSkillColor(Color color){
        this.skillImage.color = color;
    }

    public void SetLabelColor(Color color){
        this.labelImage.color = color;
    }

    public void SetLabel(int num = 0){
        labelText.text = num.ToString();
    }

    public void SetSlot(float num = 0){
        slotText.text = num.ToString("0.0");
    }

    public Sprite GetSprite() { return this.skillImage.sprite; }
}
