using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallProjectileScript : MonoBehaviour
{   
    public float speed = 5, life = 5, damage = 1;
    Transform origin, target;
    Vector3 targetDir;
    ParticleSystem particle;
    TrailRenderer trailRenderer;
    SpriteRenderer spriteRenderer;
    float curAngle = 0;

    void Awake(){
        particle = transform.GetComponent<ParticleSystem>();
        trailRenderer = transform.GetComponent<TrailRenderer>();
        spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
    }

    void Update(){
        if (origin != null && origin.gameObject.activeSelf){
            if (target != null) { 
                targetDir = (target.position - transform.position).normalized; 

                curAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(curAngle, Vector3.forward);
            }

            transform.position += targetDir * speed * Time.deltaTime;

            life -= Time.deltaTime;

            if (life <= 0) { Destroy(gameObject); };
        }
        else { Destroy(gameObject); }
    }

    public void SetOrigin(Transform origin){
        this.origin = origin;
    }

    public void SetDirection(Vector3 dir){
        targetDir = dir;
    }

    public void SetLife(float life){
        this.life = life;
    }

    public void SetSpeed(float speed){
        this.speed = speed;
    }

    public void SetColor(Color color){
        ParticleSystem.MainModule particleMain = particle.main;

        spriteRenderer.color = color;
        trailRenderer.startColor = color;
        particleMain.startColor = color;
    }

    public Transform GetOrigin() { return this.origin; }

    public float GetDamage() { return damage; }

    public void ToggleParticle(bool status){
        if (particle == null) { return; }
        
        if (status) { particle.Play(); }
        else { particle.Stop(); }
    }

    public IEnumerator DelayedSetTarget(Transform target, float delay = 1){
        yield return new WaitForSeconds(delay);

        if (transform == null || target == null) { yield return null; }
        else {
            Vector3 tempDir = (target.position - transform.position).normalized; 
            float t = 0;
        
            while (t <= 1.0) {
                curAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

                t += Time.deltaTime / delay;
                targetDir = Vector2.Lerp(targetDir, tempDir, t);
                transform.rotation = Quaternion.AngleAxis(curAngle, Vector3.forward);
                yield return null;
            }

            transform.LookAt(target);
            this.target = target;
        }
    }

    public IEnumerator LoseTargetAt(float delay = 1){
        yield return new WaitForSeconds(delay);
        this.target = null;
    }
}
