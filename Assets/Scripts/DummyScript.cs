using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyScript : MonoBehaviour
{
    [SerializeField] DialogUIScript dialogUIScript;
    [SerializeField] Rigidbody2D rbody;
    [SerializeField] Transform character;
    [SerializeField] SpriteRenderer spriteRenderer;
    ParticleSystem particle;
    Vector2 originPost;
    EffectsScript effectsScript;
    Canvas canvas;
    List<string> dialogs;

    void Awake(){
        effectsScript = GameObject.Find("Effects").GetComponent<EffectsScript>();
        canvas = transform.Find("Canvas").GetComponent<Canvas>();
        particle = transform.GetComponent<ParticleSystem>();
        dialogs = new List<string>(){
            "Sup, I'm called 'Dummy'. I can teach you how to use spell and stuff",
            "You can click 'Space' to select a skill and 'Right-Click' to start 'Casting'.",
            "Once you started casting, a circle will apear around you with a 'Pointer' orbitting you",
            "Each bar in the circle represent the number shown at the bottom of the screen",
            "Your goal is to sort the numbers in ascending order form left to right",
            "You can move the numbers by 'Right-Click'-ing when the 'Pointer' is in the region of any of the 4 bars",
            "'Right-Click'-ing when the 'Pointer' is NOT in any bar region, clicking 'Space' or insufficient mana will cancel the casting",
            "Sucessful casting will grant you 'Charge' for your spell",
            "That's it I guess. You can try those spells on me, I don't mind."
        };

        if (canvas != null) { canvas.enabled = false; }
    }

    // void FixedUpdate(){
    //     Vector2 direction = (originPost - (Vector2) transform.position).normalized;

    //     if (Vector2.Distance(transform.position, originPost) <= 0.1f){ direction *= 0; }

    //     rbody.velocity = direction * 3;
    // }

    public void SetOriginPost(Vector2 post){
        originPost = post;
    }

    void OnTriggerEnter2D(Collider2D col){
        switch(col.tag){
            case "Cursor": canvas.enabled = true; break;
            case "SmallProjectile": 
                TriggerDamage((int) col.GetComponent<SmallProjectileScript>().GetDamage());
                effectsScript.StartCoroutine(effectsScript.SpawnSmallEffect("small_explosion", col.transform.position, 1));
                Destroy(col.gameObject);
            break;
            case "MediumProjectile": 
                TriggerDamage((int) col.GetComponent<SmallProjectileScript>().GetDamage());
                effectsScript.StartCoroutine(effectsScript.SpawnSmallEffect("medium_explosion", col.transform.position, 1));
                Destroy(col.gameObject);
            break;
            case "EmmiterProjectile": 
                Destroy(col.gameObject);
            break;
        }
        // Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
        // effectsScript.StartCoroutine(effectsScript.SpawnSmallEffect("small_explosion", col.transform.position, 1));
    }

    void OnTriggerExit2D(Collider2D col){
        switch(col.tag){
            case "Cursor": canvas.enabled = false; break;
        }
    }

    void OnCollisionEnter2D(Collision2D col){
        switch(col.transform.tag){
            case "Player":
            case "Dummy":
                Physics2D.IgnoreCollision(transform.GetComponent<Collider2D>(), col.collider);
            break;
        }
    }

    public void PromptDialogs(){
        dialogUIScript.ToggleCanvas(true);
        dialogUIScript.SetSequenceTexts(dialogs);
        dialogUIScript.SetBackText("Back");
        dialogUIScript.SetNextText("Next");
        dialogUIScript.SetContext("Dialog");
    }

     void TriggerDamage(int damage){
        StartCoroutine(LerpScale(new Vector2(1.1f, 0.9f), 
            new Color(1, .3f, .3f), 0.15f));
        SpawnParticle(new Color(1, 0, 0));
        effectsScript.StartCoroutine(effectsScript.SpawnRateEffect(
            transform.position, -damage, 1));
    }

    public void SpawnParticle(Color color){
        ParticleSystem.MainModule main = particle.main;

        main.startColor = color;
        particle.Play();
    }

    public IEnumerator LerpScale(Vector2 newScale, Color newColor, float seconds){
        Vector2 originalScale = Vector2.one;
        Color originalColor = Color.white;
        float t = 0f;

        while (t <= 1.0) {
            t += Time.deltaTime / seconds;
            character.localScale = Vector2.Lerp(originalScale, newScale, t);
            spriteRenderer.color = Color.Lerp(originalColor, newColor, t);
            yield return null;
        }

        t = 0;

        while (t <= 1.0) {
            t += Time.deltaTime / seconds;
            character.localScale = Vector2.Lerp(newScale, originalScale, t);
            spriteRenderer.color = Color.Lerp(newColor, originalColor, t);
            yield return null;
        }

        character.localScale = originalScale;
    }
}
