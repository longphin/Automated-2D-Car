using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	public Transform target;

	// Use this for initialization
	void Start () {}
	
	// Move camera every frame.
	void Update () {
		//transform.position = new Vector3( target.position.x, target.position.y, -10f );
	}

    // Set a new target for the camera.
    public void setCameraTarget(Transform t)
    {
        target = t;
    }
}
