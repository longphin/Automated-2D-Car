using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CarsControllerHelper
{
    public static int NumberOfCars = 0;
    public static int NumberOfActiveCars = 0;
    public static List<GameObject> cars = new List<GameObject>();
    private static int generation = 0;
    private static bool generationIterated = true; // will be true if the current generation has already incremented the generation count

    public static Vector2[] innerPoints; // [TODO] remove

    public static List<Transform> checkpoints = new List<Transform>();
    //public static EdgeCollider2D innerPath;

    public static void InactivateCar()
    {
        NumberOfActiveCars -= 1;
        Debug.Log(NumberOfActiveCars.ToString());
    }
    
    public static void incrementGeneration()
    {
        if (!generationIterated)
        {
            generation += 1;
            generationIterated = true;
        }
    }

    public static int GetCheckpointId(Transform checkpoint)
    {
        for(int i=0; i<checkpoints.Count; i++)
        {
            if (checkpoints[i].Equals(checkpoint)) return (i);
        }
        return (-1);
    }
    /*
    public static bool Checkpoint(Vector2 point)
    {
        foreach(var pathpoint in innerPoints)
        {
            if(pathpoint.Equals(point))
            {
                return (true);
            }
        }
        return (false);
    }
    */
}

public class CarsController : MonoBehaviour
{
    private int NumberOfCars = 10;
    private int NumberOfCarsCreated = 0;
    private bool needToCreateCars = true;
    private float timer = 0f;
    private float timeBetweenCreate = 0.25f;
    private bool needToResetCars = false;
    private List<GameObject> cars = new List<GameObject>();
    private int carToReset = 0;

    public EdgeCollider2D innerPath;

    //private List<Transform> checkpoints = new List<Transform>();

    // Use this for initialization
    void Start () {
        //CarsControllerHelper.innerPath = innerPath;
        CarsControllerHelper.innerPoints = innerPath.points;
        /*
		for(int i = 0; i<NumberOfCars; i++)
        {
            // Create a new car object.
            var newCar = (GameObject)Instantiate(Resources.Load("Car_new"), transform.position, transform.rotation);
            
            CarsControllerHelper.cars.Add(newCar);         
            CarsControllerHelper.NumberOfCars += 1;
            CarsControllerHelper.NumberOfActiveCars += 1;
        }
        */

        foreach (var node in innerPath.GetComponentsInChildren<Transform>())//GetComponentInChildren<Transform>)
        {
            if(node != innerPath.transform)
            {
                CarsControllerHelper.checkpoints.Add(node);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (needToResetCars || needToCreateCars || CarsControllerHelper.NumberOfActiveCars <= 0)
        {
            timer += Time.deltaTime;
            bool resetTimer = false;

            resetTimer = createCars();
            resetTimer = resetCars() || resetTimer;

            if (resetTimer) timer = 0f;
        }
    }

    private bool createCars() // returns true if at least 1 car was created
    {
        if (needToCreateCars && timer >= timeBetweenCreate)
        {
            var newCar = (GameObject)Instantiate(Resources.Load("Car_new"), transform.position, transform.rotation);

            cars.Add(newCar);
            CarsControllerHelper.cars.Add(newCar);
            CarsControllerHelper.NumberOfCars += 1;
            CarsControllerHelper.NumberOfActiveCars += 1;
            NumberOfCarsCreated += 1;

            if (NumberOfCarsCreated >= NumberOfCars) needToCreateCars = false;

            return (true);
        }
        return (false);
    }

    private bool resetCars() // returns true if at least 1 car was reset
    {

        if (!needToCreateCars && !needToResetCars && CarsControllerHelper.NumberOfActiveCars <= 0)
        {
            carToReset = 0;
            needToResetCars = true;
            CarsControllerHelper.incrementGeneration();

            // [TODO] evolution step.

        }

        if (needToResetCars && timer >= timeBetweenCreate)
        {
            cars[carToReset].GetComponent<CarController>().ResetCar();

            CarsControllerHelper.NumberOfActiveCars += 1;
            carToReset += 1;
            if (carToReset >= NumberOfCars)
            {
                needToResetCars = false;
            }

            return (true);
        }

        return (false);
    }
}
