using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CarsControllerHelper
{
    public static int NumberOfCars = 0;
    public static int NumberOfActiveCars = 0;
}

public class CarsController : MonoBehaviour {
    public GameObject CarObject;
    public List<GameObject> cars;

	// Use this for initialization
	void Start () {
		for(int i = 0; i<3; i++)
        {
            cars.Add(Instantiate(CarObject));
            
            CarsControllerHelper.NumberOfCars += 1;
            CarsControllerHelper.NumberOfActiveCars += 1;
        }
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    private void RestartCars()
    {
        // [TODO] When a round is over, do the evolution and randomize some car weights.
        //Layer_new inpLayer = new Layer_new();
        //clone.GetComponent<CarController>().getNeuralNetwork().setInputLayer(inpLayer);
    }
}
