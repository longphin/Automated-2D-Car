using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour {
    public GameObject[] carSpawners;
    private CarsController[] carSpawnersScript;

    private int[] numberOfCars = new int[] { 17, 17 };
    private float percentageAsElite = .2f; // The creme de la creme of each generation is the top NumberOfCars * percentageToKeep.
    private float percentageOfEliteChildren = .4f; // The percentage of the next generation made up of elite children.
    private float percentageToTransfer = .2f; // The top x% will be kept in the next generation.
    private float mutationRate = .01f;
    private float timer = 0f;
    private float timeBetweenCreate = 0.25f;
    //private float timeBetweenNeuralNetworkCreate = 0.1f;

    // do not set
    private int[] numberOfCarsCreated;
    private int[] numberAsElite;// { get { return(Mathf.CeilToInt((float)NumberOfCars * percentageAsElite)); } }
    private int[] numberOfEliteChildren;// { get { return(Mathf.CeilToInt((float)NumberOfCars * percentageOfEliteChildren)); } }
    private int[] numberToTransfer;// { get { return(Mathf.CeilToInt((float)NumberOfCars * percentageToTransfer)); } }
    private Vector2[] position;
    private Quaternion[] rotation;
    private List<GameObject>[] cars; // This is a list of all of the cars.
    private List<CarController>[] carsScript;
    private List<CarStats> shuffledGroup = new List<CarStats>();
    private List<CarStats>[] eliteGroup;// = new List<CarStats>();
    private int[] numberOfCarsReset;
    private List<NeuralNetwork_new>[] newGenerationNeuralNetworks;// = new List<NeuralNetwork_new>();

    // Use this for initialization
    void Start () {
        //numberAsElite = Mathf.CeilToInt((float)CarsControllerHelper.NumberOfCars * percentageAsElite);
        //numberOfEliteChildren = Mathf.CeilToInt((float)CarsControllerHelper.NumberOfCars * percentageOfEliteChildren);
        //numberToTransfer = Mathf.CeilToInt((float)CarsControllerHelper.NumberOfCars * percentageToTransfer);
        
        // allocate space
        int numberOfSpawners = carSpawners.Length;
        numberOfCarsReset = new int[numberOfSpawners];
        numberAsElite = new int[numberOfSpawners];
        numberOfEliteChildren = new int[numberOfSpawners];
        numberToTransfer = new int[numberOfSpawners];
        cars = new List<GameObject>[numberOfSpawners];
        carsScript = new List<CarController>[numberOfSpawners];
        eliteGroup = new List<CarStats>[numberOfSpawners];
        numberOfCarsCreated = new int[numberOfSpawners];
        newGenerationNeuralNetworks = new List<NeuralNetwork_new>[numberOfSpawners];
        // initialize values
        for (int i = 0; i < numberOfSpawners; i++)
        {
            cars[i] = new List<GameObject>();
            carsScript[i] = new List<CarController>();
            newGenerationNeuralNetworks[i] = new List<NeuralNetwork_new>();
            eliteGroup[i] = new List<CarStats>();
            numberAsElite[i] = Mathf.CeilToInt((float)numberOfCars[i] * percentageAsElite);
            numberOfEliteChildren[i] = Mathf.CeilToInt((float)numberOfCars[i] * percentageOfEliteChildren);
            numberToTransfer[i] = Mathf.CeilToInt((float)numberOfCars[i] * percentageToTransfer);
        }

        carSpawnersScript = new CarsController[numberOfSpawners];
        position = new Vector2[numberOfSpawners];
        rotation = new Quaternion[numberOfSpawners];
        for(int i = 0; i<numberOfSpawners; i++)
        {
            position[i] = carSpawners[i].transform.position;
            rotation[i] = carSpawners[i].transform.rotation;
            carSpawnersScript[i] = carSpawners[i].GetComponent<CarsController>();
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (CarsControllerHelper.NeedCreateCars) // Create cars phase
        {
            timer += Time.deltaTime;

            if (timer >= timeBetweenCreate)
            {
                createCar();

                timer = 0;
            }
        }
        // Indicate that cars and neural networks need to be created phase.
        else if (!CarsControllerHelper.NeedProcessNeuralNetworks
                && !CarsControllerHelper.NeedResetCars
                && CarsControllerHelper.NumberOfActiveCars <= 0) // set need to reset flag on
        {
            preprocessNeuralNetworks();
        } else if (CarsControllerHelper.NeedProcessNeuralNetworks) // else, need to create neural networks
        {
            processNeuralNetworks();
        }
        else if(CarsControllerHelper.NeedResetCars)
        {
            resetCars();
        }
    }

    private NeuralNetwork_new MakeChildNeuralNetwork(NeuralNetwork_new NN1, NeuralNetwork_new NN2, int checkpointsDone1, int checkpointsDone2)
    {
        return (new NeuralNetwork_new(NN1, NN2, mutationRate, checkpointsDone1, checkpointsDone2));
    }

    private void createCar()
    {
        int numberOfSpawnersFinished = 0;
        int i = 0;
        while(i < carSpawners.Length && CarsControllerHelper.NeedCreateCars)
        {
            if (numberOfCarsCreated[i] >= numberOfCars[i])
            {
                i += 1;
                numberOfSpawnersFinished += 1;
                continue;
            }

            var newCar = (GameObject)Instantiate(Resources.Load("Car_new"), position[i], rotation[i]);
            var carScript = newCar.GetComponent<CarController>();
            carScript.setId(CarsControllerHelper.NumberOfCarsCreated);
            carScript.setTrack(carSpawnersScript[i].getInnerTrack());
            carScript.setIdSpawner(i);
            
            cars[i].Add(newCar);
            carsScript[i].Add(carScript);
            numberOfCarsCreated[i] += 1;
            CarsControllerHelper.NumberOfActiveCars += 1;
            CarsControllerHelper.NumberOfCarsCreated += 1;
            if (numberOfCarsCreated[i] >= numberOfCars[i])
            {
                numberOfSpawnersFinished += 1;
                i += 1;
                continue;
            }

            i += 1;
        }

        if(numberOfSpawnersFinished >= carSpawners.Length)
        {
            CarsControllerHelper.NeedCreateCars = false;
        }
    }

    private void preprocessNeuralNetworks()
    {
        // clear shuffle group
        this.shuffledGroup.Clear();

        // Get all cars and order them, regardless of spawner.
        List<CarStats> carStats = new List<CarStats>();
        for (int i = 0; i < carSpawners.Length; i++)
        {
            // clear elite group for each spawner
            this.eliteGroup[i].Clear();

            for (int j = 0; j < cars[i].Count; j++)
            {
                // set car's alpha to 100% opacity
                //int thisCarId = CarsControllerHelper.carsScript[i].getId();
                cars[i][j].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);

                // get car's checkpoint distance
                int reachedCheckpoint = carsScript[i][j].getCheckpoint();
                float distanceToNextCheckpoint = carsScript[i][j].distanceToNextCheckpoint();
                float lifetime = carsScript[i][j].getLifetime();
                //List<float> checkpointTimes = carControllerScript.getCheckpointTimes();
                float lastCheckpointTime = carsScript[i][j].getLastCheckpointTime();
                carStats.Add(new CarStats(j, reachedCheckpoint, distanceToNextCheckpoint, lifetime, lastCheckpointTime, i));
            }
        }
        this.shuffledGroup = carStats
            .OrderByDescending(a => a.getLastCheckpoint())
            .ThenBy(a => a.getDistNeeded()) // then order by distance to next checkpoint
            .ThenBy(a => a.getLastCheckpointTime())
            .ToList();

        // For each spawner, grab the most elite.
        for(int i = 0; i<carSpawners.Length; i++)
        {
            this.eliteGroup[i].AddRange(
                shuffledGroup
                    .Where(a => a.getIdSpawner() == i)
                    .Take(numberAsElite[i])
                    .ToList()
            );

            // clear out the neural networks list so they can be processed fresh.
            newGenerationNeuralNetworks[i].Clear();
        }
        
        CarsControllerHelper.NeedProcessNeuralNetworks = true;
        //Debug.Log("Process neural networks");
    }

    private void processNeuralNetworks()
    {
        // transfer
        for(int i = 0; i<carSpawners.Length; i++)
        {
            // transfer these exactly
            for(int j = 0; j<numberAsElite[i]; j++)
            {
                int carId = eliteGroup[i][j].getIdCar();
                newGenerationNeuralNetworks[i].Add(carsScript[i][carId].getNeuralNetwork());
                //Debug.Log("elite [" + i.ToString() + "] [" + j.ToString() + "]");
            }

            // make elite children
            for(int j = 0; j<numberOfEliteChildren[i]; j++)
            {
                int parentSpawner1 = i; // Ensure at least 1 parent comes from the current spawner
                int parentSpawner2 = Utils.GetRandomInt(0, carSpawners.Length); // The second parent can come from any spawner.
                int parentIndex1 = Utils.GetRandomInt(0, eliteGroup[parentSpawner1].Count);
                int parentIndex2 = Utils.GetRandomInt(0, eliteGroup[parentSpawner2].Count);
                var parentScript1 = carsScript[parentSpawner1][eliteGroup[parentSpawner1][parentIndex1].getIdCar()];
                var parentScript2 = carsScript[parentSpawner2][eliteGroup[parentSpawner2][parentIndex2].getIdCar()];
                newGenerationNeuralNetworks[i].Add(
                    MakeChildNeuralNetwork(
                        parentScript1.getNeuralNetwork(),
                        parentScript2.getNeuralNetwork(),
                        parentScript1.getScore(),
                        parentScript2.getScore()
                        ));
                //Debug.Log("elite child [" + parentSpawner1.ToString() + "] [" + parentIndex1.ToString() + "] + ["
                //    + parentSpawner2.ToString() + "] [" + parentIndex2.ToString() + "]");
            }

            // Combine the rest based from any units
            for(int j =0; j<numberOfCarsCreated[i] - numberAsElite[i] - numberOfEliteChildren[i]; j++)
            {
                int parentIndex1 = Utils.GetRandomInt(0, shuffledGroup.Count);
                int parentIndex2 = Utils.GetRandomInt(0, shuffledGroup.Count);
                int parentSpawner1 = shuffledGroup[parentIndex1].getIdSpawner();
                int parentSpawner2 = shuffledGroup[parentIndex2].getIdSpawner();
                var parentScript1 = carsScript[parentSpawner1][shuffledGroup[parentIndex1].getIdCar()];
                var parentScript2 = carsScript[parentSpawner2][shuffledGroup[parentIndex2].getIdCar()];
                newGenerationNeuralNetworks[i].Add(
                    MakeChildNeuralNetwork(
                        parentScript1.getNeuralNetwork(),
                        parentScript2.getNeuralNetwork(),
                        parentScript1.getScore(),
                        parentScript2.getScore()
                        ));
                //Debug.Log("regular child [" + parentSpawner1.ToString() + "] [" + parentIndex1.ToString() +"] + ["
                //    + parentSpawner2.ToString() + "] [" + parentIndex2.ToString() + "]");
            }
        }

        CarsControllerHelper.NeedProcessNeuralNetworks = false;
        CarsControllerHelper.NeedResetCars = true;

        numberOfCarsReset = new int[carSpawners.Length]; // reset the number of cars to 0
        CarsControllerHelper.incrementGeneration();
    }

    private void resetCars()
    {
        timer += Time.deltaTime;

        if (timer >= timeBetweenCreate)
        {
            //Debug.Log("    ...resetting car");
            int numberOfSpawnsFinished = 0;
            for (int i = 0; i < carSpawners.Length; i++)
            {
                if (numberOfCarsReset[i] >= numberOfCarsCreated[i])
                {
                    numberOfSpawnsFinished += 1;
                    continue;
                }

                int carToReset = numberOfCarsReset[i];
                carsScript[i][carToReset].ResetCar(newGenerationNeuralNetworks[i][0], position[i], rotation[i]);
                //carsScript[i][carToReset].setTrack(carSpawnersScript[i].getInnerTrack()); // Not needed because each car is reset within its own track
                newGenerationNeuralNetworks[i].RemoveAt(0);

                numberOfCarsReset[i] += 1;
                CarsControllerHelper.NumberOfActiveCars += 1;
                if (numberOfCarsReset[i]>=numberOfCarsCreated[i])
                {
                    numberOfSpawnsFinished += 1;
                    continue;
                }
            }

            if(numberOfSpawnsFinished >= carSpawners.Length)
            {
                CarsControllerHelper.generationIterated = false;
                CarsControllerHelper.NeedResetCars = false;
            }

            timer = 0;
        }
    }
}
