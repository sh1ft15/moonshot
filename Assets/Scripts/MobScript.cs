using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobScript : MonoBehaviour
{
    [SerializeField] Transform smallProjectPrefab, medProjectPrefab, bigEffPrefab, 
                    shieldPrefab, healthBar;
    [SerializeField] Rigidbody2D rbody;
    [SerializeField] LayerMask wallMask;
    List<GameObject> products;
    EffectsScript effectsScript;
    ShieldScript shieldScript;
    MapScript mapScript;
    Transform target, guardTarget, character, curShield;
    Animator animator;
    SpriteRenderer spriteRenderer;
    ParticleSystem particle;
    Vector2 direction, originPost, avoidPost;
    Canvas canvas;
    Coroutine attackCoroutine;
    MobObjectScript attr;
    AudioScript audioScript;
    AudioSource audioSource;
    bool isCasting = false, isLocking = false, facingRight = true, 
         isAvoiding = false, isAttacking = false, isActive = false;
    float moveSpeed = 2, stopingDist = 4, curAttackRate, attackRate, curBlockRate, blockRate;
    string mode, attackMode;
    float health, maxHealth;

    void Start(){
        effectsScript = GameObject.Find("Effects").GetComponent<EffectsScript>();
        mapScript = GameObject.Find("Map").GetComponent<MapScript>();
        target = GameObject.Find("Player").transform;
        particle = transform.GetComponent<ParticleSystem>();
        canvas = healthBar.parent.GetComponent<Canvas>();
        audioScript = GameObject.Find("Audio").GetComponent<AudioScript>();
        audioSource = transform.GetComponent<AudioSource>();
        products = new List<GameObject>();
    }

    // Update is called once per frame
    void Update(){
        if (character == null || isActive == false) { return; }

        if (curAttackRate > 0) { curAttackRate -= Time.deltaTime; }

        if (curShield != null) {
            if (shieldScript.IsAlive()) {
                Vector2 dir = (target.position - transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                curShield.rotation = rotation;
            }
            else {
                if (curBlockRate > 0) { curBlockRate -= Time.deltaTime; }
                else { TriggerShield(transform); }
            }   
        }
    }

    void FixedUpdate(){
        if (character == null || isActive == false) { return; }

        MoveCharacter();

        animator.SetFloat("magnitude", Mathf.Abs(rbody.velocity.magnitude));
    }

    void OnCollisionEnter2D(Collision2D col){
        switch(col.transform.tag){
            case "Mob":
            case "Stone":
            case "Wall": 
                if (isAttacking == false) {
                    StartCoroutine(AvoidTarget(col.contacts[0].point)); 
                }
            break;
            case "Player":
            case "Dummy":
                Physics2D.IgnoreCollision(transform.GetComponent<Collider2D>(), col.collider);
            break;
        }
    }

    public void UpdateHealth(float num = 0){
        Vector2 scale = healthBar.localScale;
        
        health = Mathf.Max(Mathf.Min(health + num, maxHealth), 0);
        scale.x = health / maxHealth;
        healthBar.localScale = scale;

        if (isActive && health <= 0) { StartCoroutine(TriggerDeath()); }
    }

    public void SpawnParticle(Color color){
        ParticleSystem.MainModule main = particle.main;

        main.startColor = color;
        particle.Play();
    }

    public void PlayClip(string name){
        audioSource.PlayOneShot(audioScript.GetClip(name));
    }

    public void SetChar(Transform obj){
        obj.parent = transform;
        obj.name = "Character";
        obj.localPosition = Vector2.zero;
        obj.GetComponent<MobColliderScript>().Init(this);
        spriteRenderer = obj.Find("Sprite").GetComponent<SpriteRenderer>();
        animator = obj.GetComponent<Animator>();
        character = obj;
    }

    public void SetAttr(MobObjectScript attr){
        this.attr = attr;
    }

    public void SetHealth(float num = 0){ health = maxHealth = num; }

    public void SetGuardTarget(Transform obj){ guardTarget = obj; }

    public void SetTarget(Transform obj){ target = obj; }

    public void SetStoppingDist(float num = 1){ stopingDist = num; }

    public void SetActive(bool status) { 
        isActive = status; 
        character.GetComponent<Collider2D>().enabled = isActive;
        spriteRenderer.color = new Color(1, 1, 1, status? 1 : 0.3f);
    }

    public void SetMode(string mod){
        this.mode = mod;
    }

    public void SetAttackMode(string mod){
        attackMode = mod;
    }

    public bool IsAttacking() { return isAttacking; }

    public float GetMeleeDamage(){ return attr.meleeDamage; }

    void MoveCharacter(){
        float dist, diff;
        string curMode = mode;
        Vector2 curPost = transform.position,
                targetPost = Vector2.left;

        if (isAvoiding) { curMode = "avoiding"; }
        else if (isAttacking) { curMode = "attacking"; }

        switch(curMode){
            case "static": 
                targetPost = originPost;
                direction = (originPost - curPost).normalized;
                dist = Vector2.Distance(curPost, originPost);
                diff = Mathf.Abs(dist - stopingDist);
                
                if (diff > 0.1f && dist < stopingDist){ direction *= -2f; }
                else if (dist <= 0.1f){ direction *= 0; }

                rbody.velocity = direction * moveSpeed;
            break;
            case "avoiding": 
                targetPost = avoidPost;
                direction = (curPost - avoidPost).normalized; 

                rbody.velocity = direction * moveSpeed;
            break;
            case "attacking": break;
            default:
                if (target != null || guardTarget != null) { 
                    targetPost = guardTarget != null? guardTarget.position : target.position;
                    direction = (targetPost - curPost).normalized;
                    dist = Vector2.Distance(curPost, targetPost);
                    diff = Mathf.Abs(dist - stopingDist);

                    if (diff > 0.5f && dist < stopingDist){  direction *= -1f; }
                    else if (diff <= 0.5f) { direction *= 0; }

                    rbody.velocity = direction * moveSpeed;
                }
            break;
        }

        // when not moving
        if (direction.magnitude == 0){ 
            if (target != null) { targetPost = target.position; }

            if ((targetPost.x > curPost.x && !facingRight) 
                || (targetPost.x < curPost.x && facingRight)){ Flip(); }

            if (curAttackRate <= 0) { Attack(); }    
        }
        // when moving
        else {
            if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight)) { Flip(); }
        }
    }

    void Flip(){
        facingRight = !facingRight;
        character.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
    }

    void Attack(){
        if (attackCoroutine != null) { return; }

        switch(attackMode){
            case "small_range":
            case "medium_range":
            case "big_range":
                Vector2 curPost = transform.position,
                        dir = ((Vector2) target.position - curPost).normalized;

                if (attackMode.Equals("big_range")) {
                    curPost = target.position;
                    attackCoroutine = StartCoroutine(PerformBombardment(curPost, dir));
                }
                else {
                    curPost = transform.position;
                    attackCoroutine = StartCoroutine(PerformRangeAttack(curPost, dir, 
                        attackMode == "medium_range"? "medium": "small"));
                    PlayClip(attackMode == "medium_range"? "shoot_1" : "shoot_2");
                }
            break;
            case "melee": attackCoroutine = StartCoroutine(PerformMeleeAttack()); break;
        }
    }

    public void ResetAttack(){
        if (isAttacking && attackCoroutine != null) { 
            StopCoroutine(attackCoroutine); 
            spriteRenderer.color = Color.white;
            curAttackRate = Random.Range(attackRate - 1, attackRate);
            isAttacking = false;
            attackCoroutine = null;
        }
    }

    public void ClearProducts(){
        if (products.Count > 0) {
            foreach(GameObject prod in products){
                Destroy(prod);
            }
        }
    }

    public void SetAttackRate(int rate){
        attackRate = curAttackRate = rate;
    }

    public void SetBlockRate(int rate){
        blockRate = curBlockRate = rate;
    }

    public void InitShield(Transform obj){
        Transform shield = GetShield();

        if (shield == null) { 
            shield = Instantiate(shieldPrefab, obj.position, Quaternion.identity); 
            shield.parent = obj;
            curBlockRate = Random.Range(blockRate * 0.1f, blockRate);;

            shieldScript = shield.GetComponent<ShieldScript>();
            shieldScript.IgnoreCollision(transform.Find("Character").GetComponent<Collider2D>());
            SetShield(shield);
        }
    }

    public void TriggerShield(Transform obj){
        if (! shieldScript.IsAlive()){
            Transform shield = GetShield();

            shieldScript.SetLife(15);
            shieldScript.SetCoverage("25");
            shieldScript.StartCoroutine(shieldScript.ToggleLife(true));  
            curBlockRate = Random.Range(blockRate * 0.8f, blockRate);
        }
    }

    public void SetShield(Transform shield){
        curShield = shield;
    }

    public Transform GetShield() { return curShield; }

    public IEnumerator TriggerDeath(){
        if (isActive) {
            if (attackCoroutine != null) { StopCoroutine(attackCoroutine); }

            ClearProducts();

            isActive = false;
            animator.Play("idle");
            canvas.enabled = false;
            rbody.velocity = Vector2.zero;
            spriteRenderer.color = Color.black;
            character.GetComponent<Collider2D>().enabled = false;
            effectsScript.StartCoroutine(effectsScript.SpawnDeathEffect(transform.position));
            mapScript.UpdateChaos(-1);
            
            yield return new WaitForSeconds(0.5f);
            
            gameObject.SetActive(false);
        }
    }

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

    IEnumerator PerformMeleeAttack(){
        if (isAttacking == false){
            Vector2 dir = (target.position - transform.position).normalized;
            float dist = Vector2.Distance(transform.position, target.position);
            Color color = new Color(.6f, .6f, 1);

            if (dist >= 1){
                rbody.velocity = Vector2.zero;
                isAttacking = true;
                
                PlayClip("charged");
                SpawnParticle(color);
                yield return StartCoroutine(LerpScale(new Vector2(1.2f, 0.8f), color, 0.2f));
                
                spriteRenderer.color = color;
                rbody.AddForce(dir * 15, ForceMode2D.Impulse);

                yield return new WaitForSeconds(0.3f);
                ResetAttack();
            }
        }
    }

    IEnumerator PerformRangeAttack(Vector3 post, Vector3 dir, string mod = ""){
        if (isAttacking == false) {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg,
                offsetAngle = Random.Range(-15, 15);
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward),
                    offsetRotation = Quaternion.AngleAxis(offsetAngle, Vector3.forward);
            Vector3 originalPost = post;
            Transform projectile;
            SmallProjectileScript script;

            isAttacking = true;
            rotation = offsetRotation * rotation;
            post = post + (offsetRotation * dir * 1);

            switch(mod){
                case "medium": projectile = Instantiate(medProjectPrefab, post, rotation); break;
                default: projectile = Instantiate(smallProjectPrefab, post, rotation); break; 
            }

            script = projectile.GetComponent<SmallProjectileScript>();

            script.SetColor(new Color(1, .6f, .6f));
            script.SetOrigin(transform.Find("Character"));
            script.GetComponent<Animator>().SetTrigger("emerge");

            yield return new WaitForSeconds(0.5f);

            if (projectile != null) {
                script.ToggleParticle(true);
                script.SetSpeed(3);
                script.SetLife(6);
                script.StartCoroutine(script.DelayedSetTarget(target, 0));
                script.StartCoroutine(script.LoseTargetAt(2));
            }

            ResetAttack();
        }
    }

    IEnumerator PerformBombardment(Vector2 post, Vector2 dir){
        if (isAttacking == false) {
            Transform bigEff;
            SmallEffectScript script; 

            post += (dir * 1);

            bigEff = Instantiate(bigEffPrefab, post, Quaternion.identity);
            script = bigEff.GetComponent<SmallEffectScript>();
            isAttacking = true;

            products.Add(bigEff.gameObject);
            script.SetColor(new Color(1, .6f, .6f));
            script.TriggerAnim("emerge");
            PlayClip("charged");

            yield return new WaitForSeconds(2);

            script.TriggerAnim("explosion");

            yield return new WaitForSeconds(0.3f);
            PlayClip("explosion_2");

            yield return new WaitForSeconds(0.7f);

            products.Remove(bigEff.gameObject);
            Destroy(bigEff.gameObject);
            ResetAttack();
        }
    }

    public IEnumerator AvoidTarget(Vector3 post){
        if (isAvoiding == false){
            isAvoiding = true;
            avoidPost = post;

            yield return new WaitForSeconds(1);
            isAvoiding = false;
        }
    }
}


// RaycastHit2D hit = Physics2D.Raycast(transform.position, direction * -1f, 10, wallMask);
                        
// if (hit.collider != null) {
//     if (Vector2.Distance(transform.position, hit.point) >= 1){ direction *= -1f; }
//     else { direction *= 0; }
// }
// else { direction *= -1f;  }  