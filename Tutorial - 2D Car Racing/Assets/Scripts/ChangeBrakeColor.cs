using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBrakeColor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    }

    void FixedUpdate()
    {
        if (Input.GetButton("Brakes"))
        {
            GetComponent<SpriteRenderer>().color = Color.red;
            // Consider using rb.AddForceAtPosition to apply force twice, at the position
            // of the rear tires/tyres
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }
}
