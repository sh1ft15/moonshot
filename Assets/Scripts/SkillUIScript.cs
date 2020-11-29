using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIScript : MonoBehaviour
{
    [SerializeField] List<Sprite> skillSprites;
    [SerializeField] List<Transform> skillBoxes;
    [SerializeField] Text healthText, manaText;
    [SerializeField] Transform healthBar, manaBar;
    List<Skill> skills;
    Skill curSkill;
    Transform hilitedSkill = null;
    int currentIndex = -1;
    float health = 10, maxHealth = 10, mana = 10, maxMana = 10;

    void Awake(){
        skills = new List<Skill>();

        for(int i = 0; i < skillSprites.Count; i++){
            Transform box = skillBoxes[i];
            Sprite sprite = skillSprites[i];
            SkillUIBoxScript script = box.GetComponent<SkillUIBoxScript>();
            Skill skill = new Skill();
            string name = sprite.name.Replace("skill-", "");

            skill.name = name;
            skill.sprite = sprite;
            skill.memory = 0;

            switch(name){
                case "mana_arrow": 
                    skill.combination = 2;
                    skill.cost = 0.5f;
                    skill.maxStore = 10; 
                    break;
                case "mana_shield": 
                    skill.combination = 3;
                    skill.cost = 0.75f; 
                    skill.maxStore = 8;
                    break;
                case "mana_bolt": 
                    skill.combination = 4;
                    skill.cost = 1f;
                    skill.maxStore = 6; 
                    break;
                case "mana_wave": 
                    skill.combination = 5;
                    skill.cost = 1.25f; 
                    skill.maxStore = 3;
                    break;
            }

            skills.Add(skill);
            script.SetSkillImage(sprite);
            script.SetLabel(0);
            script.SetSlot(skill.cost);
        }

        HiliteSkill(0);
    }

    public void SwitchSkill(){
        if (currentIndex == (skillBoxes.Count - 1)) { currentIndex = 0; }
        else { currentIndex += 1; }

        HiliteSkill(currentIndex);
    }

    public void UpdateMana(float num = 0){
        Vector2 scale = manaBar.localScale;

        mana = Mathf.Max(Mathf.Min(mana + num, maxMana), 0);
        scale.x = (float) mana / maxMana;

        manaText.text = mana.ToString("0.0") + " / " + maxMana.ToString("0.0");
        manaBar.localScale = scale;
    }

    public void UpdateHealth(float num = 0){
        Vector2 scale = healthBar.localScale;

        health = Mathf.Max(Mathf.Min(health + num, maxHealth), 0);
        scale.x = (float) health / maxHealth;

        healthText.text = health.ToString("0.0") + " / " + maxHealth.ToString("0.0");
        healthBar.localScale = scale;
    }

    public void ResetStat(){
        UpdateHealth(maxHealth);
        UpdateMana(maxMana);

        for(int i = 0; i < skills.Count; i++){
            UpdateLabel(i, -skills[i].memory);
        }
    }

    public void UpdateLabel(int index, int num = 0){
        if (index != -1 && index < skillBoxes.Count){
            Skill skill = skills[index];
            int curNum = skill.memory;

            curNum = Mathf.Max(curNum + num, 0);
            skill.memory = curNum;
            skills[index] = skill;
            skillBoxes[index].GetComponent<SkillUIBoxScript>().SetLabel(curNum);

            if (skill.name.Equals(curSkill.name ?? "")) { curSkill.memory = curNum; }
        }
    }

    public float GetMana() { return mana; }

    public float GetMaxMana() { return maxMana; }

    public int GetMaxStore() { return curSkill.maxStore; }
    
    public float GetHealth() { return health; }

    public float GetCost() { return curSkill.cost; }

    public string GetSkillName() { return curSkill.name; }

    public int GetSkillIndex() { return skills.FindIndex(skill => skill.name == curSkill.name); }

    public int GetCombination() { return curSkill.combination; }

    public int GetMemory() { return curSkill.memory; }

    public void HiliteSkill(int index){
        if (index >= 0 && index < skillBoxes.Count){
            Transform temp = skillBoxes[index];
            SkillUIBoxScript tempScript = temp.GetComponent<SkillUIBoxScript>(),
                             hilitedScript;
            Sprite temp_sprite = tempScript.GetSprite();
            int skillIndex = skills.FindIndex(skill => skill.sprite == temp_sprite);

            if (temp != hilitedSkill && skillIndex != -1){
                curSkill = skills[skillIndex];

                // restore prev hilited skill color
                if (hilitedSkill != null) { 
                    hilitedScript = hilitedSkill.GetComponent<SkillUIBoxScript>();
                    hilitedScript.SetSkillColor(new Color (0.3f, 0.3f, 0.3f, 0.5f));
                    hilitedScript.SetLabelColor(new Color (0.3f, 0.3f, 0.3f, 0.5f));
                }
                
                // hilite current skill
                tempScript.SetSkillColor(Color.white);
                tempScript.SetLabelColor(new Color(0, 0.5f, 0, 0.5f));
                hilitedSkill = temp;
                currentIndex = index;
            }
        }
    }
}

public struct Skill{
    public string name;
    public Sprite sprite;
    public float cost;
    public int combination, memory, maxStore;
}
