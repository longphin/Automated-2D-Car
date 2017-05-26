using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Car2dController : MonoBehaviour {

	float speedForce = 15f;
	float torqueForce = -200f;
	float driftFactorSticky = 0.9f;
	float driftFactorSlippy = 1;
	float maxStickyVelocity = 2.5f;
    //float minSlippyVelocity = 1.5f;	// <--- Exercise for the viewer

    public List<SightObjects> sightList = new List<SightObjects>();

    // Use this for initialization
    void Start () {
        //StartCoroutine(waitForCarPhysics());
        int i = 0;
        foreach(SightObjects o in sightList)
        {
            o.index = i++;
        }
    }

	void Update() {
        // check for button up/down here, then set a bool that you will use in FixedUpdate
        //Raycasting(sightStartT, sightEndT, indicatorT);
        foreach(SightObjects o in sightList)
        {
            Debug.Log(o.index.ToString() + " " + o.GetDistToHit());
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Rigidbody2D rb = GetComponent<Rigidbody2D>();

		float driftFactor = driftFactorSticky;
		if(RightVelocity().magnitude > maxStickyVelocity) {
			driftFactor = driftFactorSlippy;
		}

		rb.velocity = ForwardVelocity() + RightVelocity()*driftFactor;

		if(Input.GetButton("Accelerate")) {
			rb.AddForce( transform.up * speedForce );

			// Consider using rb.AddForceAtPosition to apply force twice, at the position
			// of the rear tires/tyres
		}
		if(Input.GetButton("Brakes")) {
			rb.AddForce( transform.up * -speedForce/2f );
            
			// Consider using rb.AddForceAtPosition to apply force twice, at the position
			// of the rear tires/tyres
		}

		// If you are using positional wheels in your physics, then you probably
		// instead of adding angular momentum or torque, you'll instead want
		// to add left/right Force at the position of the two front tire/types
		// proportional to your current forward speed (you are converting some
		// forward speed into sideway force)
		float tf = Mathf.Lerp(0, torqueForce, rb.velocity.magnitude / 2);
		rb.angularVelocity = Input.GetAxis("Horizontal") * tf;
	}

	Vector2 ForwardVelocity() {
		return transform.up * Vector2.Dot( GetComponent<Rigidbody2D>().velocity, transform.up );
	}

	Vector2 RightVelocity() {
		return transform.right * Vector2.Dot( GetComponent<Rigidbody2D>().velocity, transform.right );
	}

    void Raycasting(Transform startpoint, Transform endpoint)
    {
        //Debug.DrawLine(sightStart.position, sightEnd.position, Color.green);

        var hit = Physics2D.Linecast(startpoint.position, endpoint.position, 1 << LayerMask.NameToLayer("Edges"));

        if (hit.collider != null)
        {
            //Debug.Log(hit.collider.name);
            Debug.DrawLine(startpoint.position, hit.point, Color.green);
        }
        else
        {
            //Debug.Log("No hit");
            Debug.DrawLine(startpoint.position, endpoint.position, Color.gray);
        }
    }

    /*
    IEnumerator waitForCarPhysics()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            sightStart.Translate(Vector2.zero);
            sightEnd.Translate(Vector2.zero);
            indicator.Translate(Vector2.zero);
            transform.Translate(Vector2.zero);

            Raycasting();
        }
    }
    */
}
