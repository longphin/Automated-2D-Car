using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CarsControllerHelper
{
    //public static int NumberOfCars = 30; // This is the number of cars created.
    public static int NumberOfActiveCars = 0; // This is the number of active cars.
    private static int generation = 0; // This is the current generation count;
    public static bool generationIterated = false; // This will be true if the current generation count has been incremented by 1.
    public static float carMaxSightRange = 10f; // This is the range that each car can see. It is also used to normalize values.
    public static bool NeedCreateCars = true;
    public static bool NeedResetCars = false;
    public static bool NeedProcessNeuralNetworks = false;
    public static int NumberOfCarsCreated = 0;

    public static void InactivateCar()
    {
        NumberOfActiveCars -= 1;
    }
    
    public static void incrementGeneration()
    {
        if (!generationIterated)
        {
            generation += 1;
            Debug.Log("Generation " + generation.ToString());
            generationIterated = true;
        }
    }

    public static int getGeneration()
    {
        return (generation);
    }
}

public class CarStats
{
    private int IdCar;
    private int lastCheckpoint;
    private float distToNextCheckpoint;
    private float lifetime;
    //private List<float> checkpointTimes;
    private float lastCheckpointTime;
    private int IdSpawner;
    private bool finishedLap;

    public CarStats(int id, int checkpoint, float distToNextCheckpoint, float lifetime, float lastCheckpointTime, int IdSpawner, bool finishedLap)
    {
        this.IdCar = id;
        this.lastCheckpoint = checkpoint;
        this.distToNextCheckpoint = distToNextCheckpoint;
        this.lifetime = lifetime;
        //this.checkpointTimes = checkpointTimes;
        this.lastCheckpointTime = lastCheckpointTime;
        this.IdSpawner = IdSpawner;
        this.finishedLap = finishedLap;
    }

    public int getIdSpawner()
    {
        return (IdSpawner);
    }
    public int getLastCheckpoint()
    {
        return(lastCheckpoint);
    }

    public float getDistNeeded()
    {
        return (distToNextCheckpoint);
    }

    public float getLifetime()
    {
        return (lifetime);
    }

    public int getIdCar()
    {
        return (IdCar);
    }

    public float getLastCheckpointTime()
    {
        return (lastCheckpointTime);
    }

    public bool getFinishedLap()
    {
        return (finishedLap);
    }
}

public class CarsController : MonoBehaviour
{
    public GameObject innerTrack;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
    }

    public GameObject getInnerTrack()
    {
        return (innerTrack);
    }
    
}
