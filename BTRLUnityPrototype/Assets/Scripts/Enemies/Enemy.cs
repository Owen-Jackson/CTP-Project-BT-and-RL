using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    [SerializeField]
    protected Dictionary<string, float> m_resistances;
    [SerializeField]
    protected string m_name;    //Used to identify the enemy type
    [SerializeField]
    protected float m_maxHealth;
    [SerializeField]
    protected float m_health;
    [SerializeField]
    protected bool m_alive;
    [SerializeField]
    protected float m_timeToRespawn;
    [SerializeField]
    protected float m_respawnTimer;

    public float GetResistance(string rType)
    {
        if (m_resistances.ContainsKey(rType))
        {
            return m_resistances[rType];
        }
        else
        {
            return 1;
        }
    }

	// Use this for initialization
	public void Start () {
        m_resistances = new Dictionary<string, float>
        {
            //initialise elemental resistances, done as a multiplier - 0 = 100% resistance, 1 = no resistance, anything over 1 = weakness
            { "Water", 1f },
            { "Earth", 1f },
            { "Air", 1f },
            { "Fire", 1f }
        };
        m_alive = true;
        m_respawnTimer = 0;
        m_timeToRespawn = 1f;
    }
	
    public void Die()
    {
        m_alive = false;
        Environment.Instance.GameRespawner.StartRespawnTimer(this, m_timeToRespawn);
        //this.gameObject.SetActive(false);
    }

    public float TakeDamage(float damage)
    {
        m_health -= damage;
        if(m_health <= 0)
        {
            //Debug.Log(name + " defeated");
            Die();
        }
        return m_health;
    }

    public string GetName()
    {
        return m_name;
    }

    public bool IsAlive()
    {
        return m_alive;
    }

    public void Respawn()
    {
        m_health = m_maxHealth;
        m_alive = true;
        gameObject.SetActive(true);
    }

	// Update is called once per frame
	void Update () {
		
	}
}
