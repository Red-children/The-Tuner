using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth = 100;
    public TMP_Text healthText;
    public string HPUiText = "HP Text";
    public string AniHPChanged = "HPChanged";
    // private Animator animator;
    private Animator animator;
    private void Die()
    {
        gameObject.SetActive(false);
    }
    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        animator.Play(AniHPChanged);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthText.text = "HP: " + currentHealth + " / " + maxHealth;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Start()
    {
        animator = GameObject.Find(HPUiText).GetComponent<Animator>();
        healthText = GameObject.Find(HPUiText).GetComponent<TMP_Text>();
        if (healthText == null)
        {
            Debug.LogError("No TMP_Text component found on HP Text object");
            return;
        }
        healthText.text = "HP: " + currentHealth + " / " + maxHealth;
    }
}
