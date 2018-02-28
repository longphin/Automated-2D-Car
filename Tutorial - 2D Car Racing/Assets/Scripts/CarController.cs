using System;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    float speedForce = 15f;
    float torqueForce = -200f;
    float driftFactorSticky = 0.9f;
    float driftFactorSlippy = 1;
    float maxStickyVelocity = 2.5f;
    
    private float fValue; // used for custom input smoothing

    // sight directions
    private static Int16 numberOfSights = 45;
    private static int TotalNumberOfInputs = 2 * numberOfSights + 3;
    private static int TotalNumberOfOutputs = 4;

    private float r = CarsControllerHelper.carMaxSightRange; // Distance that the car can see
    private float angleIncrement = 2 * Mathf.PI / numberOfSights; // Determines the angle between each sight ray.

    private Rigidbody2D rb;

    private bool useAI = true;
    private bool pause = false;

    private NeuralNetwork_new thisNN;
    private int L = 3; // number of layers
    private int[] N = new int[] { TotalNumberOfInputs, TotalNumberOfInputs, TotalNumberOfOutputs }; // number of nodes in each layer

    private int lastCheckpoint = -1;
    private float lastCheckpointTime = 0;
    private Vector2 deadPosition; // When the car hits a wall, it will be "dead" at that position.
    private Quaternion deadRotation;

    private float generationTimer = 0f; // this is how long the current generation has been running
    private float generationMaxTimer = 5f; // this is how long the current generation has to run
    private float lifetime = 0f; // The full lifetime that the car has been running.
    private float timeUntilNextCheckpoint = 0;

    private InnerTrack innerTrackScript;
    
    private int IdCar;
    private int IdSpawner;

    private bool IsElite = false;
    private bool finishedLap = false;
    private float lapTime = 0;

    private List<float> checkpointTimes = new List<float>();

    public void setAngle(Quaternion angle)
    {

    }

    public void setIdSpawner(int id)
    {
        IdSpawner = id;
    }
    public void setCheckpoint(int newCheckpoint)
    {
        if (newCheckpoint == lastCheckpoint+1) // make sure the checkpoint was the next in line. This is to prevent going backwards at the start giving a big lead.
        {
            lastCheckpoint = newCheckpoint;
            checkpointTimes.Add(timeUntilNextCheckpoint);
            timeUntilNextCheckpoint = 0;
            lastCheckpointTime = lifetime;
        }
    }

    public bool getFinishedStatus()
    {
        return (finishedLap);
    }

    public int getId()
    {
        return (IdCar);
    }

    public void setId(int id)
    {
        this.IdCar = id;
    }

    public void setTrack(GameObject track)
    {
        //this.innerTrack = track;
        this.innerTrackScript = track.GetComponent<InnerTrack>();
    }

    public bool isCarDead()
    {
        return (pause);
    }
    public void resetTimer()
    {
        generationTimer = 0f;
    }
    public int getCheckpoint()
    {
        return (lastCheckpoint);
    }

    public int getScore()
    {
        return (lastCheckpoint+1);
    }
    public void setAsFinishedLap()
    {
        lastCheckpointTime = lifetime;
        finishedLap = true;
        lapTime = lifetime;
    }

    public float getLapTime()
    {
        return (lapTime);
    }

    public float distanceToNextCheckpoint()
    {
        return (innerTrackScript.getDistanceToNextCheckpoint(deadPosition, lastCheckpoint));
    }

    public void setL(int L)
    {
        this.L = L;
    }
    public void setN(int[] N)
    {
        this.N = N;
    }

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = 0;
        rb.velocity = Vector2.zero;

        this.thisNN = new NeuralNetwork_new(L, N);
        forwardPropogate();
    }

    public void setNeuralNetwork(NeuralNetwork_new newNN)
    {
        this.thisNN = newNN;
    }

    // This resets the car's state to when it was considered dead, but does not start it up again, so some values are not reset.
    public void ResetElite()
    {
        resetCarState();

        transform.position = deadPosition;
        transform.rotation = deadRotation;
        GetComponent<SpriteRenderer>().color = new Color(1f, .5f, .5f, 1f);
    }

    // This resets the car's entire state so it can re-do the track entirely.
    public void ResetCar(NeuralNetwork_new NN, Vector2 pos, Quaternion rotation)
    {
        resetCarState();

        this.thisNN = NN;
        transform.position = pos;
        transform.rotation = rotation;
        lastCheckpoint = -1;
        generationTimer = 0f;
        lapTime = 0;
        lifetime = 0f;
        timeUntilNextCheckpoint = 0;
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        finishedLap = false;
        checkpointTimes.Clear();
        lastCheckpointTime = 0;
        pause = false;
    }

    public float getLastCheckpointTime()
    {
        return (lastCheckpointTime);
    }
    private void resetCarState()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
    }

    public float getLifetime()
    {
        return (lifetime);
    }
    public List<float> getCheckpointTimes()
    {
        return (checkpointTimes);
    }

    void Update()
    {
        if (!pause && generationTimer >= generationMaxTimer)
        { 
            setCarAsDead();
        }
        else
        {
            generationTimer += Time.deltaTime;
            lifetime += Time.deltaTime;
            timeUntilNextCheckpoint += Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (pause == true) return;

        forwardPropogate();

        float driftFactor = driftFactorSticky;
        if (RightVelocity().magnitude > maxStickyVelocity)
        {
            driftFactor = driftFactorSlippy;
        }

        rb.velocity = ForwardVelocity() + RightVelocity() * driftFactor;

        /*
        if (Input.GetButton("Accelerate") || (useAI == true))
        {
            rb.AddForce(transform.up * speedForce);
        }
        */
        /*
		if(Input.GetButton("Brakes") || (useAI == true && outputs[0]<0)) {
			rb.AddForce( transform.up * -speedForce/2f );
            
			// Consider using rb.AddForceAtPosition to apply force twice, at the position
			// of the rear tires/tyres
		}
        */

        // If you are using positional wheels in your physics, then you probably
        // instead of adding angular momentum or torque, you'll instead want
        // to add left/right Force at the position of the two front tire/types
        // proportional to your current forward speed (you are converting some
        // forward speed into sideway force)
        float tf = Mathf.Lerp(0, torqueForce, rb.velocity.magnitude / 2);
        double[] outputs = this.thisNN.getOutputs();
        float smoothing = CustomInputSmoothing((float)outputs[0], (float)outputs[1]);
        //Debug.Log("smoothing: " + smoothing.ToString());
        if (Double.IsNaN(outputs[2]) || Double.IsNaN(outputs[3]))
        {
            thisNN.printNN();
        }

        rb.AddForce(transform.up * (float)(outputs[2] - outputs[3]) * speedForce);
        rb.angularVelocity = smoothing * tf;
        //rb.angularVelocity = 0;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Edges"))
        {
            if (!pause) // don't want to trigger twice
            {
                setCarAsDead();
            }
        }
    }

    public void setAsElite(bool status)
    {
        IsElite = status;
    }
    public bool getIsElite()
    {
        return (IsElite);
    }

    // If the car collides with a wall or if it has been running for too long, then it will be marked as dead.
    public void setCarAsDead()
    {
        pause = true;
        deadPosition = transform.position;
        deadRotation = transform.rotation;
        
        CarsControllerHelper.InactivateCar();
    }

    public Vector2 getDeadPosition()
    {
        return (deadPosition);
    }

    public Quaternion getDeadRotation()
    {
        return (deadRotation);
    }

    public void startCar()
    {

    }

    private void forwardPropogate()
    {
        double[] sights = new double[TotalNumberOfInputs];

        // update neural network
        float carAngle = Mathf.Atan2(transform.right.y, transform.right.x);
        //double totalSightDistances = 0;
        double sightMin = r + 1; // unreachable max value
        double sightMax = -1; // unreachable min value
        for (int i = 0; i < numberOfSights; i++)
        {
            float x = transform.position.x + r * Mathf.Cos(carAngle + angleIncrement * i/2);
            float y = transform.position.y + r * Mathf.Sin(carAngle + angleIncrement * i/2);

            Vector2 sightVec = new Vector2(x, y);
            var hit = Physics2D.Linecast(transform.position, sightVec, 1 << LayerMask.NameToLayer("Edges"));
            
            if (hit.collider != null)
            {
                //Debug.DrawLine(transform.position, hit.point, Color.red);
                sights[i] = hit.distance; // how far from the wall until hit
                if (sights[i] > sightMax) sightMax = sights[i];
                if (sights[i] < sightMin) sightMin = sights[i];
                sights[numberOfSights + i - 1] = 1; // 1 if there is a wall
            }
            else
            {
                //Debug.DrawLine(transform.position, sightVec, Color.green);
                sights[i] = r;
                if (sights[i] > sightMax) sightMax = sights[i];
                if (sights[i] < sightMin) sightMin = sights[i];
                sights[numberOfSights + i - 1] = -1;
            }
        }

        // normalize the distances
        //double averageSight = totalSightDistances / numberOfSights;
        if (sightMax - sightMin == 0) throw new DivideByZeroException("Somehow sight max = sight min.");

        for(int i=0; i<numberOfSights; i++)
        {
            sights[i] = (sights[i] - sightMin) / (sightMax - sightMin) *2-2;// normalize sight range to [-1, 1]

        }

        sights[numberOfSights * 2] = rb.velocity.x;
        sights[numberOfSights * 2 + 1] = rb.velocity.y;
        sights[numberOfSights * 2 + 2] = rb.angularVelocity/50;
        //sightDistances[numberOfSights * 2 + 2] = transform.rotation.x;
        //sightDistances[numberOfSights * 2 + 3] = transform.rotation.y;
        //sightDistances[numberOfSights * 2 + 4] = rb.angularVelocity;

        this.thisNN.forwardPropogate(new Layer_new(sights));
    }
    // Since GetAxis() is a built in Unity function that only works when key is held down, it cannot be used for script.
    // A customInputSmoothing is used to do the same thing, but can be used outside of input.
    // credit: fafase http://answers.unity3d.com/questions/958683/using-unitys-same-smoothing-from-getaxis-on-arrow.html
    private float CustomInputSmoothing(float direction, float rightDirection)
    {
        // this is to simulate Unity's key input smoothing
        float sensitivity = 3f;
        float dead = 0.001f;

        float target;
        if (useAI)
        {
            target = direction - rightDirection;
        }
        else target = Input.GetAxisRaw("Horizontal");

        fValue = Mathf.MoveTowards(fValue, target, sensitivity * Time.deltaTime);

        if (fValue < -1) fValue = -1;
        if (fValue > 1) fValue = 1;

        return (Mathf.Abs(fValue) < dead) ? 0f : fValue;
    }

    Vector2 ForwardVelocity()
    {
        return transform.up * Vector2.Dot(rb.velocity, transform.up);
    }

    Vector2 RightVelocity()
    {
        return transform.right * Vector2.Dot(rb.velocity, transform.right);
    }

    public NeuralNetwork_new getNeuralNetwork()
    {
        return (thisNN);
    }

    //[TODO]
    public void writeNeuralNetworkToFile()
    {
        thisNN.writeNeuralNetworkToFile();
    }
    //[TODO]
    public void readNeuralNetworkFromFile()
    {
        thisNN.readNeuralNetworkFromFile();
    }
}
