using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] CirclesScript circlesScript;
    [SerializeField] SkillUIScript skillUIScript;
    [SerializeField] DialogUIScript dialogUIScript;
    [SerializeField] MapScript mapScript;
    [SerializeField] CursorScript cursorScript;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Rigidbody2D rbody;
    [SerializeField] Transform character, circleContainer, circleCursor, dirCursor;
    [SerializeField] LayerMask cursorMask;
    ParticleSystem particle;
    EffectsScript effectsScript;
    bool isCasting = false, isLocking = false, isActive = true;
    float castingAngle = 0, initCastingAngle, moveSpeed = 5, maxSpeed = 7, linearDrag = 4;
    Transform curShield;
    Vector2 direction;
    Coroutine recoverCoroutine;
    AudioScript audioScript;
    AudioSource audioSource;

    void Start(){
        effectsScript = GameObject.Find("Effects").GetComponent<EffectsScript>();
        particle = transform.GetComponent<ParticleSystem>();
        recoverCoroutine = StartCoroutine(RecoverMana(2));
        audioScript = GameObject.Find("Audio").GetComponent<AudioScript>();
        audioSource = transform.GetComponent<AudioSource>();

        audioScript.PlaySFX("safe_area");
    }

    // Update is called once per frame
    void Update(){
        if (dialogUIScript.IsActive() || !isActive) { DialogControl(); }
        else {
            direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            LookAtCursor();
            Casting();

            if (Input.GetMouseButtonUp(0)){ ClickOnTarget(); }

            // change spell to perform
            if (Input.GetKeyUp(KeyCode.Space)) { 
                if (isCasting) { 
                    circlesScript.DehiliteAllBars(); 
                    SetCasting(false); 
                }
                else { skillUIScript.SwitchSkill(); }
            }

            // start casting those spell
            if (Input.GetMouseButtonDown(1) && !isLocking) { 
                if (skillUIScript.GetMemory() == 0){ ToggleCasting(); }
                else { StartCoroutine(ActiveSkill()); }
            }
        }
    }

    void FixedUpdate(){
        if (dialogUIScript.IsActive() || !isActive){ return; } 

        MoveCharacter();
        ModifyPhysics();

        animator.SetFloat("magnitude", Mathf.Abs(rbody.velocity.magnitude));
    }

    void DialogControl(){
        if (dialogUIScript.InTransition()) { return; }

        if (Input.GetMouseButtonUp(0) && dialogUIScript.HasBackBtn()) { // no / back
            dialogUIScript.IterateSequence(-1);
        } 
        else if (Input.GetMouseButtonUp(1) && dialogUIScript.HasNextBtn()) { // yes / next
            if (dialogUIScript.IsLastIndex()) {
                switch(dialogUIScript.GetContext()) {
                    case "floor_cleared":
                        ResetStat();
                        dialogUIScript.StartCoroutine(dialogUIScript.ChangeScene(0.5f));
                        mapScript.StartCoroutine(mapScript.MoveFloor(0.3f));
                    break;
                    case "floor_failed":
                        ResetStat();
                        dialogUIScript.StartCoroutine(dialogUIScript.ChangeScene(0.5f));
                        mapScript.StartCoroutine(mapScript.MoveFloor(0.3f));
                        character.GetComponent<Collider2D>().enabled = true;
                        spriteRenderer.color = Color.white;
                        isActive = true;
                    break;
                    case "move_floor": 
                        dialogUIScript.StartCoroutine(dialogUIScript.ChangeScene(0.5f));
                        mapScript.StartCoroutine(mapScript.MoveFloor(0.3f));
                    break;
                    default: dialogUIScript.IterateSequence(1); break;
                }
            }
            else { dialogUIScript.IterateSequence(1); }
        }
    }

    void MoveCharacter(){
        if (direction.magnitude > 0) {
            direction.x *= isCasting? 0.7f : 1f;
            direction.y *= isCasting? 0.7f : 1f;
        } 

        rbody.velocity = direction * moveSpeed;
    }

    void ModifyPhysics() {
        if (Mathf.Abs(rbody.velocity.x) < 0.4f || Mathf.Abs(rbody.velocity.y) < 0.4f) { rbody.drag = linearDrag; } 
        else { rbody.drag = 0f; }
    }

    void ToggleCasting(){
        float cost = skillUIScript.GetCost();
        bool enoughMana = cost <= skillUIScript.GetMana();

        // start casting
        if (isCasting == false){
            if (enoughMana) {
                Vector3 dir = GetCursorDirection();

                castingAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                circlesScript.SetupNums(skillUIScript.GetCombination());

                StartCoroutine(DelayedEnableDirCollider());
                SetCasting(true);
                PlayClip("swap");
                skillUIScript.UpdateMana(-cost);

                // initial no combination edi in order
                if (circlesScript.IsNumsInOrder()) { 
                    StartCoroutine(Locking());
                }
            }
        }
        // action during casting
        else { 
            Transform hilitedBar = circlesScript.GetHilitedBar();

            if (hilitedBar != null && enoughMana) { 
                skillUIScript.UpdateMana(-cost);
                StartCoroutine(Locking()); 
            }
            else {
                PlayClip("nope");
                circlesScript.DehiliteAllBars(); 
                SetCasting(false);  
            }
        }
    }

    void ClickOnTarget(){
        Transform obj = GetObjectOnCursor(); 

        if (obj == null) { return; }

        switch(obj.tag){
            case "Dummy": 
                PlayClip("swap");
                obj.GetComponent<DummyScript>().PromptDialogs();
                break;
            case "NPC": 
                PlayClip("swap");
                obj.GetComponent<NPCScript>().PromptDialogs();
            break;
            case "Stone": 
                int index = obj.GetSiblingIndex();

                PlayClip("swap");
                mapScript.PromptFloor(index);
            break;
            // default: cursorScript.SetTarget(obj); break;
        }
    }

    void LookAtCursor(){
        Vector3 dir = GetCursorDirection();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        animator.SetFloat("x", dir.x);
        animator.SetFloat("y", dir.y);
        dirCursor.rotation = rotation;
        circleContainer.rotation = rotation;

        if (curShield != null) { curShield.rotation = rotation; }

        
        if (isCasting == false && isLocking == false){ circleCursor.rotation = rotation; }
    }

    void Casting(){
        if (isCasting){
            castingAngle -= Time.deltaTime * (isLocking? 90 : 120);
            circleCursor.rotation = Quaternion.AngleAxis(castingAngle, Vector3.forward);
        }
    }

    public void SpawnParticle(Color color){
        ParticleSystem.MainModule main = particle.main;

        main.startColor = color;
        particle.Play();
    }

    public void PlayClip(string name){
        audioSource.PlayOneShot(audioScript.GetClip(name));
    }

    public AudioClip GetClip(string name) { return audioScript.GetClip(name); }

    public IEnumerator LerpScale(Vector2 newScale, Color newColor, float seconds){
        Vector2 originalScale = Vector2.one;
        Color originalColor = Color.white;
        float t = 0f;

        while (t <= 1.0) {
            if (isActive == false) { break; }

            t += Time.deltaTime / seconds;
            character.localScale = Vector2.Lerp(originalScale, newScale, t);
            spriteRenderer.color = Color.Lerp(originalColor, newColor, t);
            yield return null;
        }

        t = 0;

        while (t <= 1.0) {
            if (isActive == false) { break; }

            t += Time.deltaTime / seconds;
            character.localScale = Vector2.Lerp(newScale, originalScale, t);
            spriteRenderer.color = Color.Lerp(newColor, originalColor, t);
            yield return null;
        }

        character.localScale = originalScale;
    }

    public void TriggerDeath(){
        if (isActive) {
            isActive = false;
            animator.Play("idle");
            rbody.velocity = Vector2.zero;
            spriteRenderer.color = Color.black;
            character.GetComponent<Collider2D>().enabled = false;
            effectsScript.StartCoroutine(effectsScript.SpawnDeathEffect(transform.position));
        }
    }

    IEnumerator RecoverMana(float delay = 1){
        yield return new WaitForSeconds(delay);

        // recover mana if currently is not casting and not full
        if (!isCasting && !isLocking && skillUIScript.GetMana() < skillUIScript.GetMaxMana()){ 
            skillUIScript.UpdateMana(0.5f); 
        }

        // repeat after 1 seconds
        recoverCoroutine = StartCoroutine(RecoverMana());
    }

    IEnumerator Locking(){
        Transform hilitedBar = circlesScript.GetHilitedBar();

        if (isLocking == false){
            isLocking = true;

            if (hilitedBar != null) {
                circlesScript.SetBarColor(hilitedBar, Color.green);
                yield return circlesScript.StartCoroutine(circlesScript.SwapNum(hilitedBar));
            }    
            
            // yield return new WaitForSeconds(0.5f);

            if (circlesScript.IsNumsInOrder()){
                SetCasting(false);

                yield return StartCoroutine(ActiveSkill(true));

                if (skillUIScript.GetMemory() == 0){ 
                    skillUIScript.UpdateLabel(skillUIScript.GetSkillIndex(), skillUIScript.GetMaxStore());
                }

                yield return circlesScript.StartCoroutine(circlesScript.HiliteNums());
                circlesScript.DehiliteAllBars(); 
            }

            isLocking = false;

            if (hilitedBar != null) { circlesScript.SetBarColor(hilitedBar, Color.white); }
        }
    }

    IEnumerator ActiveSkill(bool init = false){
        // active Skill
        Vector3 post = transform.position,
                dir = GetCursorDirection();
        List<int> angles = new List<int>(){0, 3};
        float cost = skillUIScript.GetCost();
        bool enoughMana = cost <= skillUIScript.GetMana();
        string skillName = skillUIScript.GetSkillName(),
               mode;

        // Angles figure
        // 3 | 0 (front)
        // -----
        // 2 | 1 (back)

        switch(skillName){
            case "mana_arrow": 
            case "mana_bolt": 
                mode = skillName.Equals("mana_bolt")? "medium" : "small";

                if (init) { circlesScript.MergeIntoOneBar(dir); }
                else {
                    if (enoughMana){
                        circlesScript.StartCoroutine(circlesScript.ShootProjectile(post, dir, mode));
                        skillUIScript.UpdateMana(-cost);
                        PlayClip(mode == "medium"? "shoot_1" : "shoot_2");

                        if (skillUIScript.GetMemory() > 0){ 
                            skillUIScript.UpdateLabel(skillUIScript.GetSkillIndex(), -1);
                        }
                    }
                    else { PlayClip("nope"); }
                }
                break;
            case "mana_shield":
                if (init) { circlesScript.MergeIntoOneBar(dir); }
                else {
                    if (enoughMana) {
                        circlesScript.TriggerShield(transform);
                        skillUIScript.UpdateMana(-cost);
                        PlayClip("shield_up");

                        if (skillUIScript.GetMemory() > 0){ 
                            skillUIScript.UpdateLabel(skillUIScript.GetSkillIndex(), -1);
                        }
                    }
                    else { PlayClip("nope"); }
                } 
            break;
            case "mana_wave":
                if (init) { circlesScript.MergeIntoOneBar(dir); }
                else {
                    if (enoughMana) {
                        post = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        post.z = 0;

                        circlesScript.StartCoroutine(circlesScript.TriggerBigEffect(post));
                        skillUIScript.UpdateMana(-cost);
                        PlayClip("charged");

                        if (skillUIScript.GetMemory() > 0){ 
                            skillUIScript.UpdateLabel(skillUIScript.GetSkillIndex(), -1);
                        } 
                    }
                    else { PlayClip("nope"); }
                }
            break;
        }

        if (init == false){
            if (recoverCoroutine != null) { 
                StopCoroutine(recoverCoroutine); 
                recoverCoroutine = null;
            }

            yield return new WaitForSeconds(2);

            if (recoverCoroutine == null) { recoverCoroutine = StartCoroutine(RecoverMana(1)); }
        }
    }

    IEnumerator DelayedEnableDirCollider(){
        yield return new WaitForSeconds(0.5f);
        dirCursor.GetComponent<Collider2D>().enabled = true; 
    }

    public bool IsCasting(){ return isCasting; }

    public void SetCasting(bool status) { 
        Color color = Color.green,
              transColor = color;

        transColor.a = 0f;
        dirCursor.Find("Sprite").GetComponent<SpriteRenderer>().color = status? transColor : color;
        circleCursor.Find("Sprite").GetComponent<SpriteRenderer>().color = status? color : transColor;
        isCasting = status; 
    }

    public Vector3 GetCursorDirection(){
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position),
                dir = (Input.mousePosition - pos).normalized;

        return dir;
    }

    public void SetShield(Transform shield){
        curShield = shield;
    }

    public Transform GetShield() { return curShield; }

    public void UpdateHealth(float num = 0){
        skillUIScript.UpdateHealth(num);

        if (skillUIScript.GetHealth() <= 0 && isActive) {
            TriggerDeath();
            mapScript.PromptDeath();
        }
    }

    public void ResetStat(){
        skillUIScript.ResetStat();
    }

    void OnCollisionEnter2D(Collision2D col){
        switch(col.transform.tag){
            case "Mob":
                MobScript script = col.transform.GetComponent<MobScript>();

                // mob is melee attacking
                if (script.IsAttacking()){
                    int melee_dmg = (int) script.GetMeleeDamage();

                    StartCoroutine(LerpScale(new Vector2(1.1f, 0.9f), new Color(1, .3f, .3f), 0.15f));
                    SpawnParticle(new Color(1, 0, 0));
                    UpdateHealth(-melee_dmg);
                    effectsScript.StartCoroutine(effectsScript.SpawnRateEffect(
                        transform.position, -melee_dmg, 1));
                    script.ResetAttack();
                    PlayClip("hurt");
                }
                // Physics2D.IgnoreCollision(transform.GetComponent<Collider2D>(), col.collider);
            break;
        }
    }

    Transform GetObjectOnCursor(){
        Vector3 mouse_post = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(mouse_post.x, mouse_post.y), 
            Vector2.zero, cursorMask);

        return hit.transform ?? null;
    }
}


// Vector3 GetPostAtAngle(Vector3 currentPost, float angle, float dist){
//     float x = dist * Mathf.Cos(angle * Mathf.Deg2Rad);
//     float y = dist * Mathf.Sin(angle * Mathf.Deg2Rad);

//     currentPost.x += x;
//     currentPost.y += y;

//     return currentPost;
// }


// angles = new List<int>{0, 1, 2, 3};

// for(int i = 0; i < angles.Count; i++){
//     circlesScript.StartCoroutine(circlesScript.ShootSmallProjectile(post, dir, angles[i]));
//     yield return new WaitForSeconds(0.1f);
// }

// angles = new List<int>(){0, 3};
// circlesScript.MergeIntoTwoBars(dir, angles); // merge all bar to angle 0 n 3

// for(int i = 0; i < angles.Count; i++){
//     circlesScript.StartCoroutine(circlesScript.ShootMediumProjectile(post, dir, angles[i]));
//     yield return new WaitForSeconds(0.1f);
// }

// if (isLocking == false) { circleContainer.rotation = rotation; }
        
        // testProject.position = Quaternion.AngleAxis(-135, Vector3.forward) * dir * 2.5f;
        // testProject.rotation = rotation * Quaternion.AngleAxis(-135, Vector3.forward);

        // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-45, Vector3.forward) * dir * 2.5f, Color.red);
        // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-135, Vector3.forward) * dir * 2.5f, Color.red);
        // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-225, Vector3.forward) * dir * 2.5f, Color.red);
        // Debug.DrawRay(transform.position, Quaternion.AngleAxis(-315, Vector3.forward) * dir * 2.5f, Color.red);