using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxToEdgeIndicator : MonoBehaviour {
    public EdgeCollider2D EdgeCollider;
    public BoxCollider2D WheelCollider;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        FollowEdge();
    }

    void FollowEdge()
    {
        ColliderDistance2D edgePoint = EdgeCollider.Distance(WheelCollider);
        transform.position = edgePoint.pointA;
    }
}
