using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScript : MonoBehaviour
{
    [SerializeField] DialogUIScript dialogUIScript;
    [SerializeField] Transform constraint, chaosBar, mobPrefab;
    [SerializeField] List<WorldObjectScript> worlds;
    WorldObjectScript curWorld;
    AudioScript audioScript;
    List<WaveObjectScript> curWaves;
    List<Transform> stones, mobs;
    Transform player, safeArea;
    List<Anchor> anchors;
    Vector2 maxAnchor;
    Anchor curAnchor;
    Canvas chaosCanvas;
    int selectedIndex = -1, curChaos, maxChaos, waveCount;

    void Awake(){ 
        player = GameObject.Find("Player").transform;
        safeArea = transform.Find("SafeArea");
        chaosCanvas = chaosBar.parent.parent.GetComponent<Canvas>();
        audioScript = GameObject.Find("Audio").GetComponent<AudioScript>();

        anchors = new List<Anchor>();
        stones = new List<Transform>();
        mobs = new List<Transform>();
        maxAnchor = new Vector2(3, 2);

        chaosCanvas.enabled = false;

        Transform stonesContainer = transform.Find("Stones");

        for(int i = 0; i < stonesContainer.childCount; i++){
            Transform stone = stonesContainer.GetChild(i);

            stones.Add(stone);
            stone.GetComponent<StoneScript>().SetText(worlds[i].worldName);
        }

        foreach(Transform anchor in transform.Find("Anchors")){ 
            int x = int.Parse(anchor.name.Substring(0, 1)),
                y = int.Parse(anchor.name.Substring(1, 1));

            anchors.Add(new Anchor{
                coordinate = new Vector2(x, y),
                bounds = anchor.GetComponent<SpriteRenderer>().bounds,
                ocuppyBy = null
            });
        }
    }

    void Update(){
        if (curWorld != null) {
            if (waveCount == (curWaves.Count - 1) && curChaos <= 0 && !dialogUIScript.IsActive()){
                PromptCleared();
            }
        }
    }

    // void CheckCurrentAnchor(Transform target){
    //     Anchor anchor = GetAnchorOnObject(target);

    //     if (anchor != null && curAnchor != anchor){
    //         curAnchor = anchor;

    //         if (curAnchor.ocuppyBy != null) {
    //             List<Anchor> neighbours = GetNeighbourAnchors(GetAvailabeAnchors(), curAnchor, 2);
    //             Anchor neighbour = neighbours.Count > 0? neighbours[0] : null;

    //             if (neighbour != null){
    //                 int newIndex = anchors.FindIndex(an => an == neighbour),
    //                     oldIndex = anchors.FindIndex(an => an == curAnchor);
    //                 Bounds bound = neighbour.bounds;
    //                 Transform dummy = curAnchor.ocuppyBy;

    //                 dummy.GetComponent<DummyScript>().SetOriginPost(bound.center);

    //                 curAnchor.ocuppyBy = null;
    //                 neighbour.ocuppyBy = dummy;

    //                 anchors[oldIndex] = curAnchor;
    //                 anchors[newIndex] = neighbour;
    //             }
    //         }
    //     }
    //      else { if (curAnchor != null) { curAnchor = null;} }
    // }

    public void PromptFloor(int index){
        if (index >= 0 && index < worlds.Count){
            WorldObjectScript attr = worlds[index];
            bool is_returning = curWorld != null;
            string msg; 
            
            if (is_returning) { msg = "Return to 'Safe Area'"; }
            else { msg = "Move to '" + attr.worldName.ToUpper() + "' ?"; }

            selectedIndex = index;
            dialogUIScript.ToggleCanvas(true);
            dialogUIScript.SetSequenceTexts(new List<string>(){msg});
            dialogUIScript.SetBackText("Cancel");
            dialogUIScript.SetNextText("Yes");
            dialogUIScript.SetContext("move_floor");
        }
    }

    public void PromptCleared(){
        if (curWorld != null){
            int mobCount = mobs.Count;
            bool allCleared = false;
            List<string> msgs = new List<string>(){
                "- Floor '"+ curWorld.worldName +"' Cleared\n"
                + "- x" + mobCount + " Mobs Eliminated\n"
                + "- Returning to the Safe Area"
            };

            if (mobCount > 0) {
                foreach(Transform mob in mobs){
                    if (mob == null) { continue; }

                    Destroy(mob.gameObject); 
                }

                mobs.Clear();
            }

            curWorld.cleared = true;
            worlds[selectedIndex] = curWorld;
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            dialogUIScript.ToggleCanvas(true);

            foreach(WorldObjectScript world in worlds){
                if (world.cleared == false) { 
                    allCleared = false;
                    break;
                }
                else { allCleared = true;}
            }

            if (allCleared) {
                msgs.Add(
                    "You have cleared all the floors and defeated all the enemies." 
                    + "\nThere's not much else you can do aside from revisiting the floor"
                    + "\nThank you for spending your time playing this game. I'm sorry."
                );
            }

            dialogUIScript.SetSequenceTexts(msgs);

            dialogUIScript.SetBackText("");
            dialogUIScript.SetNextText("Continue");
            dialogUIScript.SetContext("floor_cleared");
        }
    }

    public void PromptDeath(){
        if (mobs.Count > 0) {
            foreach(Transform mob in mobs){
                if (mob == null) { continue; }

                MobScript script = mob.GetComponent<MobScript>();

                script.ClearProducts();
                Destroy(mob.gameObject);
            }

            mobs.Clear();
        }

        dialogUIScript.ToggleCanvas(true);
        dialogUIScript.SetSequenceTexts(new List<string>(){
            "- Floor '"+ curWorld.worldName +"' failed to be Cleared\n"
            + "- Returning to the Safe Area"
        });

        dialogUIScript.SetBackText("");
        dialogUIScript.SetNextText("Continue");
        dialogUIScript.SetContext("floor_failed");
    }

    public void UpdateChaos(int num = 0){
        Vector2 scale = chaosBar.localScale;
        float percent = (float) curChaos / maxChaos;

        curChaos = Mathf.Max(Mathf.Min(curChaos + num, maxChaos), 0);
        scale.x = (float) curChaos / maxChaos;

        chaosBar.localScale = scale;

        if (percent <= 0.5f && curWaves.Count > 0) { TriggerWave(); }
    }

    IEnumerator SpawnMob(Anchor anchor, WaveObjectScript wave, float delay = 1){
        if (anchor != null) {
            MobObjectScript charAttr = wave.mob;
            float randX = Random.Range(anchor.bounds.min.x, anchor.bounds.max.x),
                  randY = Random.Range(anchor.bounds.min.y, anchor.bounds.max.y);
            Transform mob = Instantiate(mobPrefab, new Vector2(randX, randY), Quaternion.identity),
                      character = Instantiate(charAttr.character, Vector2.zero, Quaternion.identity);
            MobScript script = mob.GetComponent<MobScript>();

            script.SetHealth(charAttr.health);
            script.UpdateHealth();
            script.SetChar(character);
            script.SetAttr(charAttr);

            script.SetMode("");
            script.SetAttackRate(wave.attackRate);
            script.SetAttackMode(wave.attackPattern);
            script.SetStoppingDist(wave.stoppingDistance);

            if (wave.blockRate > 0) { 
                script.SetBlockRate(wave.blockRate);
                script.InitShield(mob); 
            }

            script.SetActive(false);
            mobs.Add(mob);

            yield return new WaitForSeconds(delay);

            script.SetActive(true);
        }
    }

    void TriggerWave(bool init = false){
        if (curWaves.Count > 0 && waveCount < curWorld.waves.Count) {
            int tempWave = init? 0 : (waveCount + 1);

            if (tempWave >= curWorld.waves.Count) { return; }
            else { waveCount = tempWave; }

            WaveObjectScript wave = curWaves[waveCount];
            Anchor anchor = GetAnchorOnObject(player);
            List<Anchor> tempAnchors = anchors.FindAll(an => an != anchor);
            
            tempAnchors = ShuffleAnchorList(tempAnchors);

            if (init) {
                if (chaosCanvas.enabled == false) { chaosCanvas.enabled = true; }

                curChaos = maxChaos = wave.count;
            }
            else {
                maxChaos += wave.count;
                curChaos += wave.count;
            }
            
            UpdateChaos();

            for(int i = 0; i < wave.count; i++){ 
                StartCoroutine(SpawnMob(tempAnchors[i], wave));
            }
        }
    }

    public IEnumerator MoveFloor(float delay = 0){
        yield return new WaitForSeconds(delay);

        if (selectedIndex >= 0 && selectedIndex < worlds.Count){
            bool is_returning = curWorld != null;

            safeArea.gameObject.SetActive(is_returning);

            if (is_returning) {
                audioScript.PlaySFX("safe_area");
                curWorld = null;
                selectedIndex = -1;

                for(int i = 0; i < stones.Count; i++){ stones[i].gameObject.SetActive(true); }
            }
            else { 
                audioScript.PlaySFX("battle_area");
                curWorld = worlds[selectedIndex]; 
                curWaves = new List<WaveObjectScript>(curWorld.waves);
                waveCount = 0;

                for(int i = 0; i < stones.Count; i++){
                    stones[i].gameObject.SetActive(i == selectedIndex);
                }

                yield return new WaitForSeconds(1);
                TriggerWave(true);
            }
        }
    }

    List<Anchor> GetAvailabeAnchors(){
        return anchors.FindAll(anchor => anchor.ocuppyBy == null);
    }

    Anchor GetAnchorOnObject(Transform obj){
        return anchors.Find(anchor => anchor.bounds.Contains(obj.position));
    }

    public List<Anchor> ShuffleAnchorList(List<Anchor> list){
        for (int i = 0; i < list.Count; i++) {
            Anchor temp = list[i];
            int randomIndex = Random.Range(i, list.Count);

            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        return list;
    }

    List<Anchor> GetNeighbourAnchors(List<Anchor> list, Anchor an, int offset = 1){
        Vector2 center = an.coordinate,
                north = new Vector2(center.x, center.y - offset),
                northWest = new Vector2(center.x + offset, center.y - offset),
                west = new Vector2(center.x + offset, center.y),
                southWest = new Vector2(center.x + offset, center.y + offset),
                south = new Vector2(center.x, center.y + offset),
                southEast = new Vector2(center.x - offset, center.y + offset),
                east = new Vector2(center.x - offset, center.y),
                northEast = new Vector2(center.x - offset, center.y -offset);

        return list.FindAll(anchor => anchor.coordinate == north 
            || anchor.coordinate == northWest
            || anchor.coordinate == west
            || anchor.coordinate == southWest
            || anchor.coordinate == south
            || anchor.coordinate == southEast
            || anchor.coordinate == east
            || anchor.coordinate == northEast);
    }
}

public class Anchor{
    public Vector2 coordinate;
    public Bounds bounds;
    public Transform ocuppyBy;
    public bool exists;
}