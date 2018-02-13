﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CarsControllerHelper
{
    public static int NumberOfCars = 0;
    public static int NumberOfActiveCars = 0;
    public static List<GameObject> cars = new List<GameObject>();
    private static int generation = 0;
    private static bool generationIterated = true; // will be true if the current generation has already incremented the generation count

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

    public static float distanceToNextCheckpoint(Vector2 position, int currentCheckpoint)
    {
        if(currentCheckpoint >= checkpoints.Count) // The next checkpoint is the beginning checkpoint
        {
            return (Vector2.Distance(position, checkpoints[0].position));
        }
        return (Vector2.Distance(position, checkpoints[currentCheckpoint+1].position));
    }
}

public class CarsController : MonoBehaviour
{
    private int NumberOfCars = 10;
    private float percentageToKeep = .3f; // The creme de la creme of each generation is the top NumberOfCars * percentageToKeep.
    private float mutationRate = .01f;
    private int NumberOfCarsCreated = 0;
    private bool needToCreateCars = true;
    private float timer = 0f;
    private float timeBetweenCreate = 0.25f;
    private bool needToResetCars = false;
    private List<GameObject> cars = new List<GameObject>();
    private List<NeuralNetwork_new> newGenerationNeuralNetworks = new List<NeuralNetwork_new>();
    private int carToReset = 0;

    // do not set
    private int EliteCount;
    private double[] probabilityOfSelection; // will determine the weights for each car being selected for the next evolution

    public EdgeCollider2D innerPath;

    // Use this for initialization
    void Start () {
        EliteCount = Mathf.CeilToInt((float)NumberOfCars * percentageToKeep);

        // Get a list of checkpoints
        foreach (var node in innerPath.GetComponentsInChildren<Transform>())
        {
            if(node != innerPath.transform)
            {
                CarsControllerHelper.checkpoints.Add(node);
            }
        }

        // Initialize probabilityOfSelection
        probabilityOfSelection = new double[NumberOfCars];
        int totalSpots = 0;
        for(int i=1; i<=NumberOfCars; i++)
        {
            totalSpots += i;
        }
        // Set the probabilityOfSelection as a proportion of these.
        double currentCumulativeSum = 0f;
        for (int i = 0; i < NumberOfCars; i++)
        {
            currentCumulativeSum += (double)(i + 1) / totalSpots;// * 2d;
            if (currentCumulativeSum >= 1)
            {
                probabilityOfSelection[i] = 1d;
                break; // remaining ranks will have 0 probability of selection
            }
            probabilityOfSelection[i] = currentCumulativeSum;
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

    private NeuralNetwork_new MakeChildNeuralNetwork(NeuralNetwork_new NN1, NeuralNetwork_new NN2)
    {
        return (new NeuralNetwork_new(NN1, NN2));
    }

    private bool resetCars() // returns true if at least 1 car was reset
    {

        if (!needToCreateCars && !needToResetCars && CarsControllerHelper.NumberOfActiveCars <= 0)
        {
            carToReset = 0;
            CarsControllerHelper.incrementGeneration();

            // find the max 
            List<float[]> maxCheckpointByCar = new List<float[]>(); // [TODO] instead of storing a mix of types, make this a List<custom object>
            //int maxCheckpoint = -1;
            for (int i = 0; i < cars.Count; i++)
            {
                var carControllerScript = cars[i].GetComponent<CarController>();
                int reachedCheckpoint = carControllerScript.getCheckpoint();
                float distanceToNextCheckpoint = carControllerScript.distanceToNextCheckpoint();
                maxCheckpointByCar.Add(new float[] { (float)i, (float)reachedCheckpoint, distanceToNextCheckpoint });
                //if (reachedCheckpoint > maxCheckpoint) maxCheckpoint = reachedCheckpoint;
            }
            /*
            var shuffled = maxCheckpointByCar
                            .FindAll(a => a[1]==maxCheckpoint)
                            .OrderBy(a => Utils.GetRandomInt())
                            .Take(EliteCount)
                            .ToList();
            */
            var shuffled = maxCheckpointByCar
                            .OrderByDescending(a => a[1]) // order by checkpoint
                            .ThenBy(a => a[2]) // then order by distance to next checkpoint
                            .ToList();
            newGenerationNeuralNetworks.Clear();
            // combine parents
            // Pick parent 1
            double parentChance1 = Utils.GetRandomDbl();
            double parentChance2 = Utils.GetRandomDbl();
            int parentIndex1 = -1;
            int parentIndex2 = -1;

            for (int i= 0; i<NumberOfCars; i++) // we will fill in newGenerationNeuralNetworks with the same number of cars
            { 
                for (int j = 0; j < probabilityOfSelection.Length; j++) // find the indexes of the parents to combine
                {
                    if (parentIndex1 == -1 && parentChance1 < probabilityOfSelection[j])
                        parentIndex1 = j;
                    if (parentIndex2 == -1 && parentChance2 < probabilityOfSelection[j])
                        parentIndex2 = j;

                    if (parentIndex1 != -1 && parentIndex2 != -1) break; // both parents were found
                }

                newGenerationNeuralNetworks.Add(
                    MakeChildNeuralNetwork(cars[parentIndex1].GetComponent<CarController>().getNeuralNetwork(), cars[parentIndex2].GetComponent<CarController>().getNeuralNetwork()));
            }

            needToResetCars = true;
        }

        if (needToResetCars && timer >= timeBetweenCreate)
        {
            if (newGenerationNeuralNetworks.Count <= 0) throw new MissingReferenceException("No child neural network");

            cars[carToReset].GetComponent<CarController>().ResetCar(newGenerationNeuralNetworks[0]);
            newGenerationNeuralNetworks.RemoveAt(0);

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