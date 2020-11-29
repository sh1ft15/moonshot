using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectsScript : MonoBehaviour
{
    [SerializeField] Transform smallEffPrefab, deathPrefab, ratePrefab;
    [SerializeField] AudioScript audioScript;

    public IEnumerator SpawnSmallEffect(string name, Vector2 post, float delay = 1){
        Transform effect = Instantiate(smallEffPrefab, post, Quaternion.identity);
        SmallEffectScript script = effect.GetComponent<SmallEffectScript>();

        script.TriggerAnim(name);
        script.PlayClip(audioScript.GetClip("explosion_1"));
        yield return  new WaitForSeconds(delay);
        Destroy(effect.gameObject);
    }

    public IEnumerator SpawnDeathEffect(Vector2 post, float delay = 1){
        Transform effect = Instantiate(deathPrefab, post, Quaternion.identity);

        effect.GetComponent<Animator>().SetTrigger("emerge");
        yield return new WaitForSeconds(delay);
        Destroy(effect.gameObject);
    }

    public IEnumerator SpawnRateEffect(Vector2 post, int rate, float delay = 1){
        Transform effect = Instantiate(ratePrefab, post, Quaternion.identity);

        effect.GetComponent<Animator>().SetTrigger("emerge");
        effect.Find("Canvas/Text").GetComponent<Text>().text = rate.ToString();
        yield return new WaitForSeconds(delay);
        Destroy(effect.gameObject);
    }
}
