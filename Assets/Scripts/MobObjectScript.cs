using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mob", menuName = "ScriptableObjects/MobScriptableObject", order = 1)]
public class MobObjectScript : ScriptableObject
{
    public Transform character;
    public float health, moveSpeed, meleeDamage;
}
