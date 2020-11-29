using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclesScript : MonoBehaviour
{
    [SerializeField] PlayerScript playerScript;
    [SerializeField] NumUIScript numUIScript;
    [SerializeField] Transform smallProjectPrefab, medProjectPrefab, bigEffPrefab, shieldPrefab;
    [SerializeField] List<Sprite> circleSprites;
    [SerializeField] List<Transform> circleBars;
    Transform target, player;
    List<Transform> hilitedBars;
    List<float> barAngles;
    List<int> nums;
    int currentIndex = -1, num = 10, maxNum = 10;

    void Awake(){
        hilitedBars = new List<Transform>();
        barAngles = new List<float>{-45, -135, -225, -315};
        player = playerScript.transform;
        
        DehiliteAllBars();
    }

    public void HiliteBar(Transform bar){
        SpriteRenderer barRenderer = bar.Find("BarSprite").GetComponent<SpriteRenderer>();
        string[] str = barRenderer.sprite.name.Split('_');
        int barIndex = circleBars.FindIndex(circleBar => circleBar == bar),
            index = int.Parse(str[str.Length - 1]),
            bigBarIndex = index - 4;

        if (barIndex != -1 && (bigBarIndex > 0 && bigBarIndex < circleSprites.Count)){
            barRenderer.sprite = circleSprites[bigBarIndex];
            currentIndex = barIndex;
            hilitedBars.Add(bar);

            numUIScript.ToggleCursor(true); 
            numUIScript.MoveCursor(barIndex);
            numUIScript.HiliteCursor();
            numUIScript.SetCursorNum(nums[barIndex]);
        }
    }

    public void DehiliteBar(Transform bar){
        SpriteRenderer barRenderer = bar.Find("BarSprite").GetComponent<SpriteRenderer>();
        string[] str = barRenderer.sprite.name.Split('_');
        int barIndex = circleBars.FindIndex(circleBar => circleBar == bar),
            index = int.Parse(str[str.Length - 1]),
            smallBarIndex = index + 4;

        if (barIndex != -1 && (smallBarIndex > 0 && smallBarIndex < circleSprites.Count)){
            barRenderer.sprite = circleSprites[smallBarIndex];
            currentIndex = -1;
            hilitedBars.Remove(bar); 
        }
    }

    public void DehiliteAllBars(){
        if (hilitedBars.Count > 0){
            List<Transform> tempList = new List<Transform>(hilitedBars);

            foreach(Transform bar in tempList){ DehiliteBar(bar); }

            hilitedBars.Clear();
        }

        foreach(Transform bar in circleBars){ bar.gameObject.SetActive(false); }

        numUIScript.ToggleCursor(false);
        numUIScript.SetVisible(false);
        ResetNums();
    }

    public void SetBarColor(Transform bar, Color color){
        int index = GetBarIndex(bar);

        if (index != -1){
            bar.Find("BarSprite").GetComponent<SpriteRenderer>().color = color;
        }
    }

    public void UpdateNum(int num = 0){
        this.num = Mathf.Max(Mathf.Min(this.num + num, maxNum), 0);
    }

    public void SetupNums(int max_num){
        List<int> tempNums = new List<int>();
        int count = circleBars.Count;
        float angle = 90;

        // get list of all possible number
        for(int j = 0; j <= count; j++){ tempNums.Add(j <= max_num? j : 0); }

        tempNums = ShuffleIntList(tempNums);
        nums = new List<int>();

        for(int i = 0; i < count; i++){
            int temp;

            if (tempNums.Count > 0) { 
                temp = tempNums[0];
                tempNums.RemoveAt(0);
            }
            else { temp = 0; }

            nums.Add(temp);
            numUIScript.SetNum(i, temp.ToString());
        }

        foreach(Transform bar in circleBars){ 
            bar.localRotation = Quaternion.Euler(0, 0, -angle);
            bar.gameObject.SetActive(true); 
            angle += 90;
        }

        numUIScript.SetVisible(true);
    }

    public void ResetNums(){
        for(int i = 0; i < 4; i++){ 
            numUIScript.SetNum(i, ""); 
            numUIScript.DehiliteNum(i);
        }
    }

    public IEnumerator SwapNum(Transform bar){
        int index = circleBars.FindIndex(circleBar => circleBar == bar),
            curNum = nums[index],
            prevIndex,
            prevNum;

        if (index != -1){
            curNum = nums[index];

            if (index == 0) { prevIndex = nums.Count - 1; }
            else { prevIndex = index - 1; }

            prevNum = nums[prevIndex];
            nums[prevIndex] = curNum;
            nums[index] = prevNum;

            numUIScript.SetNum(prevIndex, curNum.ToString());
            numUIScript.SetNum(index, prevNum.ToString());
            numUIScript.SetCursorNum(prevNum);
            numUIScript.DehiliteCursor();
            numUIScript.HiliteNum(prevIndex);
            playerScript.PlayClip("swap");

            yield return new WaitForSeconds(0.5f);

            numUIScript.DehiliteNum(prevIndex);
        }
    }

    public IEnumerator HiliteNums(float delay = 0.5f){
        if (IsNumsInOrder()){
            numUIScript.ToggleCursor(false);
            playerScript.PlayClip("skill_charged");

            for(int i = 0; i < nums.Count; i++){
                numUIScript.HiliteNum(i);
            }

            // blinking effect on text
            for(int i = 0; i < 3; i++){
                for(int j = 0; j < nums.Count; j++){ numUIScript.SetNum(j, ""); }

                yield return new WaitForSeconds(0.2f);

                for(int j = 0; j < nums.Count; j++){ numUIScript.SetNum(j, nums[j].ToString()); }

                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(delay);

            for(int i = 0; i < nums.Count; i++){ numUIScript.DehiliteNum(i); }
        }
    }

    public bool IsNumsInOrder(){
        List<int> orderedNums = new List<int>(nums);
        bool inOrder = false;

        orderedNums.Sort();

        for(int i = 0; i < nums.Count; i++){
            inOrder = nums[i] == orderedNums[i];

            if (inOrder == false) { break; } // no needed further checking
        }

        return inOrder;
    }

    public int GetBarIndex(Transform bar) { return circleBars.FindIndex(circleBar => circleBar == bar); }

    public int GetNum() { return num; }

    public void SetTarget(Transform obj) { target = obj; }

    public IEnumerator ShootProjectile(Vector3 post, Vector3 dir, string mode){
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        Vector3 originalPost = post;
        Transform projectile;
        SmallProjectileScript script;

        post = post + (dir * 1);

        switch(mode){
            case "medium": projectile = Instantiate(medProjectPrefab, post, rotation); break;
            default: projectile = Instantiate(smallProjectPrefab, post, rotation);break;
        }
        
        script = projectile.GetComponent<SmallProjectileScript>();
        script.SetOrigin(transform.parent.Find("Character"));
        script.GetComponent<Animator>().SetTrigger("emerge");

        yield return new WaitForSeconds(0.5f);

        if (script != null) {
            script.ToggleParticle(true);
            script.SetDirection((post - originalPost).normalized);
            script.StartCoroutine(script.DelayedSetTarget(target, 0.5f));
        }
    }

    public IEnumerator TriggerBigEffect(Vector3 post){
        Transform bigEff = Instantiate(bigEffPrefab, post, Quaternion.identity);
        SmallEffectScript script = bigEff.GetComponent<SmallEffectScript>();

        script.TriggerAnim("emerge");

        yield return new WaitForSeconds(2);

        script.TriggerAnim("explosion");

        yield return new WaitForSeconds(0.3f);

        script.PlayClip(playerScript.GetClip("explosion_2"));

        yield return new WaitForSeconds(0.7f);

        Destroy(bigEff.gameObject);
    }

    public void TriggerShield(Transform obj){
        Transform shield = playerScript.GetShield();
        ShieldScript script;

        if (shield == null) { 
            shield = Instantiate(shieldPrefab, obj.position, Quaternion.identity); 
            shield.parent = obj;
            playerScript.SetShield(shield);
        }

        script = shield.GetComponent<ShieldScript>();

        if (shield.gameObject.activeSelf == false) { shield.gameObject.SetActive(true); }

        script.IgnoreCollision(player.Find("Character").GetComponent<Collider2D>());
        script.SetLife(15);
        script.SetCoverage("25");
        script.StartCoroutine(script.ToggleLife(true));  
    }

    public void MergeIntoTwoBars(Vector3 dir, List<int> indexes){
        if (indexes.Count != 2) { return; } // support 2 bar merging only

        indexes.Sort(); // ensure index in order

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        List<float> circleAngles = new List<float>{-90, -180, -270, -360},
                    staticAngles = new List<float>();
        
        // bar 1 rotate to bar 0 / bar 2 rotate to bar 3
        // get bars that will move and bars that will not move
        List<Transform> staticBars = new List<Transform>(),
                        movingBars = new List<Transform>();

        // get static bars
        foreach(int index in indexes){
            if (index >= 0 && index < circleBars.Count){ 
                staticBars.Add(circleBars[index]); 
                staticAngles.Add(circleAngles[index]);
                DehiliteBar(circleBars[index]);
            }
        }
        
        // get moving bars
        foreach(Transform circleBar in circleBars){
            if (staticBars.FindIndex(staticBar => staticBar == circleBar) == -1){
                movingBars.Add(circleBar);
                DehiliteBar(circleBar);
            }
        }
        
        // animate the moving bars
        for(int i = 0; i < movingBars.Count; i++){
            float offsetAngle = staticAngles[i]; 
            Quaternion offsetRotation = Quaternion.AngleAxis(offsetAngle, Vector3.forward),
                       tempRotation = offsetRotation * rotation;
            StartCoroutine(LerpRotation(movingBars[i], tempRotation, 0.5f));
        }
    }

    public void MergeIntoOneBar(Vector3 dir){
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle - 45, Vector3.forward);
        
        // animate the moving bars
        for(int i = 0; i < circleBars.Count; i++){
            DehiliteBar(circleBars[i]);
            StartCoroutine(LerpRotation(circleBars[i], rotation, 0.5f));
        }        
    }

    public Transform GetHilitedBar(){
        // get last hilited bar else return null
        if (hilitedBars.Count > 0) { return hilitedBars[hilitedBars.Count - 1]; }
        else { return null; }
    }

    public List<int> ShuffleIntList(List<int> list){
        for (int i = 0; i < list.Count; i++) {
            int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);

            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        return list;
    }

    IEnumerator LerpRotation(Transform obj, Quaternion rotation,float seconds) {
        float t = 0f;

        while (t <= 1.0) {
            t += Time.deltaTime / seconds;
            obj.rotation = Quaternion.Lerp(obj.rotation, rotation, t);
            yield return null;
        }

        obj.gameObject.SetActive(false);
    }
}

// t = 0f;
// while (t <= 1.0) {
//     t += Time.deltaTime / seconds;
//     //characterHolder.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
//     yield return null;
// }

// Vector3 GetPostAtAngle(float angle, float dist){
    //     float x = dist * Mathf.Cos(angle * Mathf.Deg2Rad);
    //     float y = dist * Mathf.Sin(angle * Mathf.Deg2Rad);
    //     Vector3 post = player.position;

    //     post.x += x;
    //     post.y += y;

    //     return post;
    // }

// public IEnumerator ShootSmallProjectile(Vector3 post, Vector3 dir, int index){
//     float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg,
//             offsetAngle = index == -1? 0 : barAngles[index];
//     Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward),
//                 offsetRotation = Quaternion.AngleAxis(offsetAngle, Vector3.forward);
//     Vector3 originalPost = post;
//     Transform projectile;
//     SmallProjectileScript script;

//     rotation = offsetRotation * rotation;
//     post = post + (offsetRotation * dir * 1);
//     projectile = Instantiate(smallProjectPrefab, post, rotation);
//     script = projectile.GetComponent<SmallProjectileScript>();

//     script.SetOrigin(transform.parent.Find("Character"));
//     script.GetComponent<Animator>().SetTrigger("emerge");

//     if (index != -1) { circleBars[index].gameObject.SetActive(false); }

//     yield return new WaitForSeconds(0.5f);

//     if (script != null) { 
//         script.ToggleParticle(true);
//         script.SetDirection((post - originalPost).normalized);
//         script.StartCoroutine(script.DelayedSetTarget(target, 0.5f));
//     }
// }