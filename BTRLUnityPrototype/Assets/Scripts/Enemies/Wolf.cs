using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Enemy {

	// Use this for initialization
	new void Start () {
        base.Start();
        m_resistances["Water"] = 1f;
        m_resistances["Earth"] = 0.5f;
        m_resistances["Air"] = 1f;
        m_resistances["Fire"] = 2f;
        m_maxHealth = 5000;
        m_health = 5000;
        m_name = "Wolf";
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
