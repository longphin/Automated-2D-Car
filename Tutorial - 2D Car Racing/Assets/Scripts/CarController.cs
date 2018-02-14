using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    float speedForce = 15f;
    float torqueForce = -200f;
    float driftFactorSticky = 0.9f;
    float driftFactorSlippy = 1;
    float maxStickyVelocity = 2.5f;
    
    public List<SightObjects> sightList = new List<SightObjects>();
    private GameObject Controller;

    private float fValue; // used for custom input smoothing

    // sight directions
    private static Int16 numberOfSights = 45;
    private static int TotalNumberOfInputs = 2 * numberOfSights + 2;
    private static int TotalNumberOfOutputs = 4;

    private float r = 10; // distance of vision
    private float angleIncrement = 2 * Mathf.PI / numberOfSights;

    private Vector3 startPosition;
    private Quaternion startAngle;
    private Vector2 startVelocity;

    private Rigidbody2D rb;

    private bool useAI = true;
    private bool pause = false;

    private NeuralNetwork_new thisNN;
    private int L = 3; // number of layers
    private int[] N = new int[] { TotalNumberOfInputs, TotalNumberOfInputs, TotalNumberOfOutputs }; // number of nodes in each layer

    private int lastCheckpoint = -1;
    private Vector2 deadPosition; // When the car hits a wall, it will be "dead" at that position.

    private int laps = 0; // [TODO] for each lap, increment this

    private float generationTimer = 0f; // this is how long the current generation has been running
    private float generationMaxTimer = 20f; // this is how long the current generation has to run

    //private BoxCollider2D collider;
    //private GameObject dummy;

    public void setCheckpoint(int newCheckpoint)
    {
        if (newCheckpoint == lastCheckpoint+1) // make sure the checkpoint was the next in line. This is to prevent going backwards at the start giving a big lead.
        {
            lastCheckpoint = newCheckpoint;
        }
    }

    public int getCheckpoint()
    {
        return (lastCheckpoint);
    }

    public int getScore()
    {
        return (lastCheckpoint + laps*CarsControllerHelper.checkpoints.Count);
    }

    public float distanceToNextCheckpoint()
    {
        if (deadPosition == null) throw new ArgumentNullException("dead position is null");

        return (CarsControllerHelper.distanceToNextCheckpoint(deadPosition, lastCheckpoint));
    }

    public void setController(GameObject o)
    {
        this.Controller = o;
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
        Vector2 initialrotation = transform.eulerAngles;

        rb = GetComponent<Rigidbody2D>();
        //collider = GetComponent<BoxCollider2D>();

        // get initial state
        startPosition = transform.position;
        startAngle = transform.rotation;
        startVelocity = rb.velocity;
        
        int i = 0;
        foreach (SightObjects o in sightList)
        {
            o.index = i++;
        }

        this.thisNN = new NeuralNetwork_new(L, N);
        forwardPropogate();
    }

    public void setNeuralNetwork(NeuralNetwork_new newNN)
    {
        this.thisNN = newNN;
    }

    public void ResetCar(NeuralNetwork_new NN)
    {
        this.thisNN = NN;
        transform.position = startPosition;
        transform.rotation = startAngle;
        rb.velocity = startVelocity;
        lastCheckpoint = -1;
        generationTimer = 0f;
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        pause = false;
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
        //float smoothing = CustomInputSmoothing((float)outputs[1], (float)outputs[2]); // [remake] need to re-add smoothing using the Neural Network's output
        double[] outputs = this.thisNN.getOutputs();
        float smoothing = CustomInputSmoothing((float)outputs[0], (float)outputs[1]);
        rb.AddForce(transform.up * (float)(outputs[2] - outputs[3]) * speedForce); // [TODO] is outputs[2] never negative? Cars do not seem to decelerate
        rb.angularVelocity = smoothing * tf;// * (outputs[1] < network.activationFunc.cutoff() ? -1f : 1f);//Input.GetAxis("Horizontal") * tf;

        //Debug.Log(smoothing.ToString() + " " + (outputs[2] - outputs[3]).ToString());
        //inputSightDistancesToNeuralNetwork(); // [remake] delete
        //network.printNN(); // [remake] delete
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

    // If the car collides with a wall or if it has been running for too long, then it will be marked as dead.
    private void setCarAsDead()
    {
        pause = true;
        rb.angularVelocity = 0;
        deadPosition = transform.position;
        
        CarsControllerHelper.InactivateCar();
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
            float x = transform.position.x + r * Mathf.Cos(carAngle + angleIncrement * i);
            float y = transform.position.y + r * Mathf.Sin(carAngle + angleIncrement * i);

            Vector2 sightVec = new Vector2(x, y);
            var hit = Physics2D.Linecast(transform.position, sightVec, 1 << LayerMask.NameToLayer("Edges"));
            
            if (hit.collider != null)
            {
                //Debug.DrawLine(transform.position, hit.point, Color.red);
                //totalSightDistances += 
                sights[i] = hit.distance; // how far from the wall until hit
                if (sights[i] > sightMax) sightMax = sights[i];
                if (sights[i] < sightMin) sightMin = sights[i];
                sights[numberOfSights + i - 1] = 1; // 1 if there is a wall
            }
            else
            {
                //totalSightDistances += 
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
            sights[i] = (sights[i] - sightMin) / (sightMax - sightMin) *2-2;// * 6 - 3; // normalize sight range to [-1, 1]
            //sights[i] = (sights[i] - averageSight) / totalSightDistances * 3;
            //sights[i] = Utils.GetRandomDbl()*6-3; // [TODO] temporary. Delete this.
        }

        sights[numberOfSights * 2] = rb.velocity.x;
        sights[numberOfSights * 2 + 1] = rb.velocity.y;
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
        return transform.up * Vector2.Dot(GetComponent<Rigidbody2D>().velocity, transform.up);
    }

    Vector2 RightVelocity()
    {
        return transform.right * Vector2.Dot(GetComponent<Rigidbody2D>().velocity, transform.right);
    }

    public NeuralNetwork_new getNeuralNetwork()
    {
        return (thisNN);
    }
}
