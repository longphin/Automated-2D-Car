using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeDetection : MonoBehaviour {
    public Transform sightStart, sightEnd, indicator;
    public bool spotted = false;

    void Update()
    {
        Raycasting();
    }

    void Start()
    {
        //StartCoroutine(RaycastingCoroutine());
    }

    void Raycasting()
    {
        sightStart.Translate(Vector3.zero);
        sightEnd.Translate(Vector3.zero);
        indicator.Translate(Vector3.zero);

        Debug.DrawLine(sightStart.position, sightEnd.position, Color.green);

        //var hitInner = Physics2D.Linecast(sightStart.position, sightEnd.position, 1 << LayerMask.NameToLayer("Edges"));
        var hit = Physics2D.Linecast(sightStart.position, sightEnd.position, 1 << LayerMask.NameToLayer("Edges"));

        indicator.position = hit.point;
        spotted = hit.collider != null ? true : false;
    }

    IEnumerator RaycastingCoroutine()
    {
        yield return new WaitForFixedUpdate();
        Raycasting();
    }
}
