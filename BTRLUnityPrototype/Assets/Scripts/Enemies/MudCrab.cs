using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MudCrab : Enemy {

    // Use this for initialization
    new void Start () {
        base.Start();
        m_resistances["Water"] = 0.25f;
        m_resistances["Earth"] = 1f;
        m_resistances["Air"] = 1.5f;
        m_resistances["Fire"] = 0.75f;
        m_maxHealth = 2000;
        m_health = 2000;
        m_name = "Mud Crab";
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
