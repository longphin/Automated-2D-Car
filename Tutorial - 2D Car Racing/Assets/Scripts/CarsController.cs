using System;
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

public class CarStats : IComparable<CarStats>
{
    private int IdCar;
    private int lastCheckpoint;
    private float distToNextCheckpoint;
    private float lifetime;
    private float lastCheckpointTime;
    private int IdSpawner;
    private bool finishedLap;
    private float lapTime;
    private List<float> checkpointTimes = new List<float>();

    public CarStats(int id, int checkpoint, float distToNextCheckpoint, float lifetime, int IdSpawner, bool finishedLap, float lapTime, List<float> checkpointTimes, float lastCheckpointTime)
    {
        this.IdCar = id;
        this.lastCheckpoint = checkpoint;
        this.distToNextCheckpoint = distToNextCheckpoint;
        this.lifetime = lifetime;
        this.IdSpawner = IdSpawner;
        this.finishedLap = finishedLap;
        this.lapTime = lapTime;
        this.checkpointTimes = checkpointTimes;
        this.lastCheckpointTime = lastCheckpointTime;
    }

    public void setLastCheckpointTotalTime(float time)
    {
        lastCheckpointTime = time;
    }
    public float getLastCheckpointTotalTime()
    {
        return (lastCheckpointTime);
    }
    public float getLapTime()
    {
        return (lapTime);
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

    public bool getFinishedLap()
    {
        return (finishedLap);
    }

    public List<float> getCheckpointTimes()
    {
        return (checkpointTimes);
    }

    /*
    public int CompareTo(CarStats other)
    {
        // If cars are not even close, then assign them a rank
        if (getCheckpointTimes().Count < other.getCheckpointTimes().Count - 1) return (1);
        if (other.getCheckpointTimes().Count < getCheckpointTimes().Count - 1) return (-1);

        // If cars are relatively close, then rank them by times
        int minIter = Utils.Min(getCheckpointTimes().Count, other.getCheckpointTimes().Count);
        for (int i = 0; i < minIter; i++)
        {
            if (getCheckpointTimes()[i] < other.getCheckpointTimes()[i]) return (1);
            if (other.getCheckpointTimes()[i] < getCheckpointTimes()[i]) return (-1);
        }

        return (0);
    }
    */

    /*
    public int CompareTo(CarStats other)
    {
        var otherCheckpointTimes = other.getCheckpointTimes();
        if (checkpointTimes.Count == 0 && otherCheckpointTimes.Count == 0) return (0);
        if (checkpointTimes.Count > otherCheckpointTimes.Count) return (-1);
        if (checkpointTimes.Count < otherCheckpointTimes.Count) return (1);

        if (checkpointTimes[checkpointTimes.Count - 1] > otherCheckpointTimes[otherCheckpointTimes.Count - 1]) return (1);
        if (checkpointTimes[checkpointTimes.Count - 1] < otherCheckpointTimes[otherCheckpointTimes.Count - 1]) return (-1);
        return (0);
    }
    */

    public int CompareTo(CarStats other)
    {
        double epsilon = .15;
        var otherTimes = other.getCheckpointTimes();

        int best = 0; // number of times this car was better
        int bested = 0; // number of times the other car was better

        if(checkpointTimes.Count > otherTimes.Count)
        {
            best = checkpointTimes.Count - otherTimes.Count;
        }
        else if(checkpointTimes.Count < otherTimes.Count)
        {
            bested = otherTimes.Count - checkpointTimes.Count;
        }

        for(int i = 0; i<Utils.Min(checkpointTimes.Count, otherTimes.Count); i++)
        {
            if (checkpointTimes[i] - otherTimes[i] > epsilon)
            {
                best += 1;
            }else if(checkpointTimes[i] - otherTimes[i] > epsilon)
            {
                bested += 1;
            }
        }

        if (best > bested) return (-1);
        if (bested > best) return (1);

        // neither had the best time majority, so look at the distance to the next point
        if (distToNextCheckpoint > other.getDistNeeded()) return (1);
        if (distToNextCheckpoint < other.getDistNeeded()) return (-1);
        return (0);
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
