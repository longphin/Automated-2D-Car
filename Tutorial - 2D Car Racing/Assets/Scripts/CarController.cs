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

    private float fValue; // used for custom input smoothing

    // sight directions
    private static Int16 numberOfSights = 45;
    private static int TotalNumberOfInputs = 2 * numberOfSights;
    private static int TotalNumberOfOutputs = 2;

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
    private int[] N = new int[] { TotalNumberOfInputs, 45, TotalNumberOfOutputs }; // number of nodes in each layer

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

    public void ResetCar()
    {
        transform.position = startPosition;
        transform.rotation = startAngle;
        rb.velocity = startVelocity;
        pause = false;
        DelayTimer();
    }

    IEnumerator DelayTimer()
    {
        yield return new WaitForSeconds(1);
    }

    void Update()
    {

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

        if (Input.GetButton("Accelerate") || (useAI == true))
        {
            rb.AddForce(transform.up * speedForce);
        }
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
        List<double> outputs = this.thisNN.getOutputs();
        float smoothing = CustomInputSmoothing((float)outputs[0], (float)outputs[1]);
        rb.angularVelocity = smoothing * tf;// * (outputs[1] < network.activationFunc.cutoff() ? -1f : 1f);//Input.GetAxis("Horizontal") * tf;

        //inputSightDistancesToNeuralNetwork(); // [remake] delete
        //network.printNN(); // [remake] delete
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Edges"))
        {
            if (!pause) // don't want to trigger twice
            {
                pause = true;
                rb.angularVelocity = 0;

                CarsControllerHelper.InactivateCar();
            }
        }
    }

    public void startCar()
    {

    }

    private void forwardPropogate()
    {
        List<double> sights = new List<double>();

        // update neural network
        float carAngle = Mathf.Atan2(transform.right.y, transform.right.x);
        //int otherInputs = 2; // [remake] temporarily added. OtherInputs should not be defined in car controller?
        //double[] sightDistances = new double[numberOfSights * 2 + otherInputs];
        for (int i = 0; i < numberOfSights; i++)
        {
            float x = transform.position.x + r * Mathf.Cos(carAngle + angleIncrement * i);
            float y = transform.position.y + r * Mathf.Sin(carAngle + angleIncrement * i);

            Vector2 sightVec = new Vector2(x, y);
            var hit = Physics2D.Linecast(transform.position, sightVec, 1 << LayerMask.NameToLayer("Edges"));

            if (hit.collider != null)
            {
                //Debug.DrawLine(transform.position, hit.point, Color.red);
                sights.Add(hit.distance); // how far from the wall until hit
                sights.Add(1); // 1 if there is a wall
                //sightDistances[i] = hit.distance;
                //sightDistances[numberOfSights + i - 1] = 1;
            }
            else
            {
                sights.Add(r);
                sights.Add(0);
                //sightDistances[i] = r;
                //sightDistances[numberOfSights + i - 1] = 0;
            }
        }

        this.thisNN.forwardPropogate(new Layer_new(sights));
        /*
        sights.Add(rb.velocity.x);
        sights.Add(rb.velocity.y);
        sights.Add(transform.rotation.x);
        sights.Add(transform.rotation.y);
        sights.Add(rb.angularVelocity);
        */

        //sightDistances[numberOfSights * 2] = rb.velocity.x;
        //sightDistances[numberOfSights * 2 + 1] = rb.velocity.y;
        //sightDistances[numberOfSights * 2 + 2] = transform.rotation.x;
        //sightDistances[numberOfSights * 2 + 3] = transform.rotation.y;
        //sightDistances[numberOfSights * 2 + 4] = rb.angularVelocity;

        //network.forwardPropogate(sightDistances); // [remake]
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
