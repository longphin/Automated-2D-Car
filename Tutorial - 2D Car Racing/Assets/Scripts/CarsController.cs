using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CarsControllerHelper
{
    public static int NumberOfCars = 0; // This is the number of cars created.
    public static int NumberOfActiveCars = 0; // This is the number of active cars.
    public static List<GameObject> cars = new List<GameObject>(); // This is a list of all of the cars.
    private static int generation = 0; // This is the current generation count;
    public static bool generationIterated = false; // This will be true if the current generation count has been incremented by 1.
    public static float carMaxSightRange = 10f; // This is the range that each car can see. It is also used to normalize values.

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
}

public class CarStats
{
    private int IdCar;
    private int lastCheckpoint;
    private float distToNextCheckpoint;
    private float lifetime;
    //private List<float> checkpointTimes;
    private float lastCheckpointTime;

    public CarStats(int id, int checkpoint, float distToNextCheckpoint, float lifetime, float lastCheckpointTime)//List<float> checkpointTimes)
    {
        this.IdCar = id;
        this.lastCheckpoint = checkpoint;
        this.distToNextCheckpoint = distToNextCheckpoint;
        this.lifetime = lifetime;
        //this.checkpointTimes = checkpointTimes;
        this.lastCheckpointTime = lastCheckpointTime;
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
}

public class CarsController : MonoBehaviour
{
    private int NumberOfCars = 30;
    private float percentageAsElite = .2f; // The creme de la creme of each generation is the top NumberOfCars * percentageToKeep.
    private float percentageOfEliteChildren = .4f; // The percentage of the next generation made up of elite children.
    private float percentageToTransfer = .1f; // The top x% will be kept in the next generation.
    private float mutationRate = .01f;
    private int NumberOfCarsCreated = 0;
    private bool needToCreateCars = true;
    private float timer = 0f;
    private float timeBetweenCreate = 0.25f;
    private bool needToResetCars = false;
    private List<GameObject> cars = new List<GameObject>();
    private List<CarController> carsScript = new List<CarController>();
    private List<NeuralNetwork_new> newGenerationNeuralNetworks = new List<NeuralNetwork_new>();
    private int carToReset = 0;

    // do not set
    private int numberAsElite;// { get { return(Mathf.CeilToInt((float)NumberOfCars * percentageAsElite)); } }
    private int numberOfEliteChildren;// { get { return(Mathf.CeilToInt((float)NumberOfCars * percentageOfEliteChildren)); } }
    private int numberToTransfer;// { get { return(Mathf.CeilToInt((float)NumberOfCars * percentageToTransfer)); } }
    private double[] probabilityOfSelection; // will determine the weights for each car being selected for the next evolution
    private Vector2 position;
    private Quaternion rotation;

    public GameObject innerTrack;

    // Use this for initialization
    void Start () {
        numberAsElite = Mathf.CeilToInt((float)NumberOfCars * percentageAsElite);
        numberOfEliteChildren = Mathf.CeilToInt((float)NumberOfCars * percentageOfEliteChildren);
        numberToTransfer = Mathf.CeilToInt((float)NumberOfCars * percentageToTransfer);

        position = transform.position;
        rotation = transform.rotation;

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
            // Check if cars need to be reset.
            bool resetTimer = false;
            resetTimer = createCars();
            resetTimer = resetCars() || resetTimer;

            if (resetTimer)
            {
                timer = 0f; // cars need to be reset
            }
        }
    }

    private bool createCars() // returns true if at least 1 car was created
    {
        if (needToCreateCars && timer >= timeBetweenCreate)
        {
            var newCar = (GameObject)Instantiate(Resources.Load("Car_new"), transform.position, transform.rotation);
            newCar.GetComponent<CarController>().setTrack(innerTrack);
            var carScript = newCar.GetComponent<CarController>();
            carScript.setTrack(innerTrack);

            cars.Add(newCar);
            carsScript.Add(carScript);
            CarsControllerHelper.cars.Add(newCar);
            CarsControllerHelper.NumberOfCars += 1;
            CarsControllerHelper.NumberOfActiveCars += 1;
            NumberOfCarsCreated += 1;

            if (NumberOfCarsCreated >= NumberOfCars) needToCreateCars = false;

            return (true);
        }
        return (false);
    }

    private NeuralNetwork_new MakeChildNeuralNetwork(NeuralNetwork_new NN1, NeuralNetwork_new NN2, int checkpointsDone1, int checkpointsDone2)
    {
        return (new NeuralNetwork_new(NN1, NN2, mutationRate, checkpointsDone1, checkpointsDone2));
    }

    private NeuralNetwork_new MakeCloneNeuralNetwork(NeuralNetwork_new NN)
    {
        return (new NeuralNetwork_new(NN));
    }

    private bool resetCars() // returns true if at least 1 car was reset
    {

        if (!needToCreateCars && !needToResetCars && CarsControllerHelper.NumberOfActiveCars <= 0)
        {
            carToReset = 0;
            CarsControllerHelper.incrementGeneration();

            // find the max 
            List<CarStats> carStats = new List<CarStats>();
            for (int i = 0; i < cars.Count; i++)
            {
                // set car's alpha to 100% opacity
                cars[i].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);

                // get car's checkpoint distance
                var carControllerScript = carsScript[i];
                int reachedCheckpoint = carControllerScript.getCheckpoint();
                float distanceToNextCheckpoint = carControllerScript.distanceToNextCheckpoint();
                float lifetime = carControllerScript.getLifetime();
                //List<float> checkpointTimes = carControllerScript.getCheckpointTimes();
                float lastCheckpointTime = carControllerScript.getLastCheckpointTime();
                carStats.Add(new CarStats(i, reachedCheckpoint, distanceToNextCheckpoint, lifetime, lastCheckpointTime));
            }
            
            
            var shuffled = carStats
                            .OrderByDescending(a => a.getLastCheckpoint()) // order by checkpoint
                            //.ThenBy(a => a.getLastCheckpointTime())
                            .ThenBy(a => a.getDistNeeded()) // then order by distance to next checkpoint
                            .ThenBy(a => a.getLastCheckpointTime())
                            //.ThenBy(a => a.getLifetime())
                            .ToList();

            /*
            var eliteGroup = carStats
                            .OrderByDescending(a => a.getLastCheckpoint()) // order by checkpoint
                            .ThenBy(a => a.getDistNeeded()) // then order by distance to next checkpoint
                            .ThenBy(a => a.getLifetime())
                            .Take(numberAsElite)
                            .ToList();
            */

            var eliteGroup = shuffled
                                .Take(numberAsElite)
                                .ToList();

            /*
            carStats.Sort();

            var shuffled = carStats.ToList();
            var eliteGroup = carStats
                                .Take(numberAsElite)
                                .ToList();
            */

            foreach(var cs in eliteGroup)
            {
                cars[cs.getIdCar()].GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 1f);
                Debug.Log("elite car: " + cs.getIdCar().ToString());
            }

            newGenerationNeuralNetworks.Clear();

            for (int i= 0; i<NumberOfCars; i++) // we will fill in newGenerationNeuralNetworks with the same number of cars
            {
                if (i < numberToTransfer)
                {
                    newGenerationNeuralNetworks.Add(
                        carsScript[eliteGroup[i].getIdCar()].getNeuralNetwork());
                    Debug.Log("transfer: " + eliteGroup[i].getIdCar().ToString());
                }
                else
                {
                    if (i < numberOfEliteChildren+numberToTransfer)
                    {
                        int parentIndex1 = Utils.GetRandomInt(0, eliteGroup.Count);
                        int parentIndex2 = Utils.GetRandomInt(0, eliteGroup.Count);
                        newGenerationNeuralNetworks.Add(
                            MakeChildNeuralNetwork(
                                carsScript[eliteGroup[parentIndex1].getIdCar()].getNeuralNetwork(),

                                carsScript[eliteGroup[parentIndex2].getIdCar()].getNeuralNetwork(),

                                carsScript[eliteGroup[parentIndex1].getIdCar()].getScore(),

                                carsScript[eliteGroup[parentIndex2].getIdCar()].getScore()
                                ));
                    }
                    else
                    {
                        int parentIndex1 = Utils.GetRandomInt(0, shuffled.Count);
                        int parentIndex2 = Utils.GetRandomInt(0, shuffled.Count);
                        newGenerationNeuralNetworks.Add(
                            MakeChildNeuralNetwork(
                                carsScript[shuffled[parentIndex1].getIdCar()].getNeuralNetwork(),

                                carsScript[shuffled[parentIndex2].getIdCar()].getNeuralNetwork(),

                                carsScript[shuffled[parentIndex1].getIdCar()].getScore(),

                                carsScript[shuffled[parentIndex2].getIdCar()].getScore()
                                ));
                    }
                }
            }

            needToResetCars = true;
        }

        if (needToResetCars && timer >= timeBetweenCreate)
        {
            if (newGenerationNeuralNetworks.Count <= 0) throw new MissingReferenceException("No child neural network");

            carsScript[carToReset].ResetCar(newGenerationNeuralNetworks[0], this.position, this.rotation);
            newGenerationNeuralNetworks.RemoveAt(0);

            CarsControllerHelper.NumberOfActiveCars += 1;
            carToReset += 1;
            if (carToReset >= NumberOfCars)
            {
                CarsControllerHelper.generationIterated = false;
                needToResetCars = false;
            }

            return (true);
        }

        return (false);
    }
}
