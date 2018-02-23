using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment
{
    private static Environment m_instance;
    public Respawner GameRespawner { get; set; }
    //these dictionaries store how many times this enemy type has been attacked by a damge type
    public Dictionary<string, int> FireEleAttacked = new Dictionary<string, int>();
    public Dictionary<string, int> WolfAttacked = new Dictionary<string, int>();
    public Dictionary<string, int> MudCrabAttacked = new Dictionary<string, int>();

    //these dictionaries store the q values that the testing AI produces during the demo
    public Dictionary<string, float> FireEleQValues = new Dictionary<string, float>();
    public Dictionary<string, float> WolfQValues = new Dictionary<string, float>();
    public Dictionary<string, float> MudCrabQValues = new Dictionary<string, float>();
    private List<string> damageTypes;
    public bool loaded = false;
    //The Environment should be initialised as a singleton
    //Its methods are accessed via Instance
    public static Environment Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new Environment();
            }
            return m_instance;
        }
    }

    Environment()
    {
        //create an instance for the game respawner
        GameRespawner = GameObject.Instantiate(Resources.Load("Prefabs/Respawner") as GameObject).GetComponent<Respawner>();
        damageTypes = new List<string>() { "Water", "Earth", "Air", "Fire" };
        foreach (string type in damageTypes)
        {
            FireEleAttacked.Add(type, 0);
            WolfAttacked.Add(type, 0);
            MudCrabAttacked.Add(type, 0);
            FireEleQValues.Add(type, 0f);
            WolfQValues.Add(type, 0f);
            MudCrabQValues.Add(type, 0f);
        }
        /*
        //Stores all of the types of enemies in the game
        EnemyTable = new Dictionary<string, Enemy>
        {
            {"FireElemental", new FireElemental() },
            {"MudCrab", new MudCrab() },
            {"Wolf", new Wolf() }
        };
        */
        loaded = true;
    }

    public void SetQValue(string enemyType, string damageType, float qValue)
    {
        switch (enemyType)
        {
            case "Mud Crab":
                MudCrabQValues[damageType] = qValue;
                break;
            case "Wolf":
                WolfQValues[damageType] = qValue;
                break;
            case "Fire Elemental":
                FireEleQValues[damageType] = qValue;
                break;
            default:
                break;
        }
    }

    public void AddToDamageDictionary(string enemyType, string damageType)
    {
        switch (enemyType)
        {
            case "Mud Crab":
                MudCrabAttacked[damageType]++;
                break;
            case "Wolf":
                WolfAttacked[damageType]++;
                break;
            case "Fire Elemental":
                FireEleAttacked[damageType]++;
                break;
            default:
                break;
        }

    }

    private float m_lastDist;

    public float GetBestReward(string stateName)
    {
        float reward = -0.01f;

        return reward;
    }
}
