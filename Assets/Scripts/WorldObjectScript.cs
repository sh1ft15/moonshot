using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "World", menuName = "ScriptableObjects/WorldScriptableObject", order = 1)]
public class WorldObjectScript : ScriptableObject
{
    public string worldName;
    public int maxMobCount;
    public bool cleared;
    public List<WaveObjectScript> waves;
}
