using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CarsControllerHelper
{
    public static int NumberOfCars = 0;
    public static int NumberOfActiveCars = 0;
    public static List<GameObject> cars = new List<GameObject>();
    private static int generation = 0;

    public static void InactivateCar()
    {
        NumberOfActiveCars -= 1;

        if(NumberOfActiveCars == 0)
        {
            ResetCars();
        }
    }

    private static void ResetCars()
    {
        generation += 1;

        foreach (var car in cars)
        {
            car.GetComponent<CarController>().ResetCar();
            NumberOfActiveCars += 1;
        }
    }
}

public class CarsController : MonoBehaviour
{
    /*
    public static int NumberOfCars = 10;
    public static int NumberOfActiveCars = 0;
    public static List<GameObject> cars = new List<GameObject>();
    private static int generation = 0;
    private static bool carsSpawning = false;
    */
    private int NumberOfCars = 20;

    // Use this for initialization
    void Start () {
		for(int i = 0; i<NumberOfCars; i++)
        {
            // Create a new car object.
            var newCar = (GameObject)Instantiate(Resources.Load("Car_new"), transform.position, transform.rotation);
            
            CarsControllerHelper.cars.Add(newCar);         
            CarsControllerHelper.NumberOfCars += 1;
            CarsControllerHelper.NumberOfActiveCars += 1;
        }
	}
	
	// Update is called once per frame
	void Update () {
        
    }
}
