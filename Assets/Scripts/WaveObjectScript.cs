using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "ScriptableObjects/WaveScriptableObject", order = 1)]
public class WaveObjectScript : ScriptableObject 
{
    public int count, attackRate, blockRate;
    public float stoppingDistance;
    public string attackPattern;
    public MobObjectScript mob;
}
