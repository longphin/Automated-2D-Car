using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodScript : MonoBehaviour {
    public new CameraFollow camera;
    public GameObject carPrefab;

    private List<GameObject> cars;

	// Use this for initialization
	void Start () {
        cars = new List<GameObject>();

        int NumCarsToCreate = 2;
        // create cars
        for(int i =0;i<NumCarsToCreate;i++)
        {
            GameObject obj = Instantiate(carPrefab) as GameObject;
            cars.Add(obj);

            Car2dController carScript = (Car2dController)obj.GetComponent(typeof(Car2dController));
            carScript.setGodScriptReference(this);
        }

        camera.setCameraTarget(cars[0].transform);
	}
	
	// Update is called once per frame
	void Update () {

	}
    
    public void startCars()
    {
        Debug.Log("reset cars");
        foreach (GameObject c in cars)
        {
            Car2dController carScript = (Car2dController)c.GetComponent(typeof(Car2dController));
            carScript.startCar();
        }
    }
}
