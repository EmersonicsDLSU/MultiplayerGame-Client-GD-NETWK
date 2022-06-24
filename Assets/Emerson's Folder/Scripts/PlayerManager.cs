using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth = 100f;
    public int itemCount = 0;
    [HideInInspector]public int killCount = 0;
    [HideInInspector]public Text playerNameScore;
    public MeshRenderer model;

    [SerializeField]private Image healthBar;

    private void Update()
    {
        // update our health
        healthBar.fillAmount = health / maxHealth;
        // update our score
        playerNameScore.text = $"{username}: {killCount}";
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
    }
    // assigns the health
    public void SetHealth(float _health)
    {
        health = _health;
        // check if player dies
        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }
}