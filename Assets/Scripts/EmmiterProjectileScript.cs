using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmmiterProjectileScript : MonoBehaviour
{
    [SerializeField] Transform smallProjectilePrefab;
    float initAngle = 0;

    void Start(){
        StartCoroutine(ShootProjectile(0.5f));
    }

    public IEnumerator ShootProjectile(float delay = 1){
        Transform projectile = Instantiate(smallProjectilePrefab, transform.position, Quaternion.identity);
        Vector3 target = GetPostAtAngle(transform.position, -initAngle, 1);
        SmallProjectileScript script = projectile.GetComponent<SmallProjectileScript>();
        Debug.Log(initAngle);
        projectile.rotation = Quaternion.AngleAxis(-initAngle, Vector3.forward);
        script.SetOrigin(transform);
        script.SetDirection((target - transform.position).normalized);
        yield return new WaitForSeconds(delay);

        if (initAngle >= 360){ initAngle = 45; }
        else { initAngle += 45; }

        StartCoroutine(ShootProjectile(delay));
    }

    Vector3 GetPostAtAngle(Vector3 origin, float angle, float dist){
        float x = dist * Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = dist * Mathf.Sin(angle * Mathf.Deg2Rad);

        origin.x += x;
        origin.y += y;

        return origin;
    }
}
