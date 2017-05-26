using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightObjects : MonoBehaviour {
    private float distToHit;
    private static int SightObjectCount = 0;

    public int index { get; set; }
    public Transform startpoint, endpoint;

    void Start()
    {
        SightObjectCount += 1;
    }
	// Update is called once per frame
	void Update () {
        Raycasting();
    }

    void Raycasting()
    {
        //Debug.DrawLine(sightStart.position, sightEnd.position, Color.green);

        var hit = Physics2D.Linecast(startpoint.position, endpoint.position, 1 << LayerMask.NameToLayer("Edges"));

        if (hit.collider != null)
        {
            //Debug.Log(hit.collider.name);
            Debug.DrawLine(startpoint.position, hit.point, Color.green);
            distToHit = hit.distance;
        }
        else
        {
            //Debug.Log("No hit");
            Debug.DrawLine(startpoint.position, endpoint.position, Color.gray);
            distToHit = Vector2.Distance(startpoint.position, endpoint.position);
        }
    }

    public float GetDistToHit()
    {
        return (distToHit);
    }
}
