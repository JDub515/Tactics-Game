using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public abstract class BaseUnitController : MonoBehaviour {

    protected int health;
    protected int maxHealth;
    protected int energy;
    protected float moveDistance;
    protected float visionDistance;
    public bool invincible;

    protected GameObject resourceBar;
    protected Image healthBar;
    protected Rigidbody rb;

    public GameObject damageTextPrefab;
    public GameObject resourceBarsPrefab;
    public GameObject explosionPrefab;

    void LateUpdate() {
        resourceBar.transform.position = transform.position + 1.2f * Vector3.up + new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
        resourceBar.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }

    public void RecieveDamage(float damage) {
        int damageInt = Mathf.FloorToInt(damage);
        if (damageInt < 1 || invincible) {
            return;
        }
        health -= damageInt;
        healthBar.fillAmount = ((float)health / ((float)maxHealth + (6f/.94f))) + .03f;
        GameObject damageText = Instantiate(damageTextPrefab, transform.position + 1.5f * Vector3.up, Quaternion.LookRotation(Camera.main.transform.forward));
        damageText.transform.position += damageText.transform.right * Random.Range(-.5f, .5f) + damageText.transform.up * Random.Range(-.5f, .5f);
        damageText.GetComponent<TextMeshPro>().SetText(damageInt.ToString());
        if (health < 0) {
            HandleDeath();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.impulse.magnitude > 5) {
            RecieveDamage(collision.impulse.magnitude / 5f);
        }
    }

    public IEnumerator TempInvinicibility(float time) {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
    }

    protected abstract void HandleDeath();
}
