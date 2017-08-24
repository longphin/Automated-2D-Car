using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Car2dController : MonoBehaviour {

	float speedForce = 15f;
	float torqueForce = -200f;
	float driftFactorSticky = 0.9f;
	float driftFactorSlippy = 1;
	float maxStickyVelocity = 2.5f;

    private Network network;
    private static readonly char[] nodeNamePrefix = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
    //float minSlippyVelocity = 1.5f;	// <--- Exercise for the viewer
    private bool useAI = true; // AI with neural network not fully implemented

    public Transform testTransform;

    public List<SightObjects> sightList = new List<SightObjects>();

    private float fValue; // used for custom input smoothing

    // sight directions
    private static Int16 numberOfSights = 180;
    private static Int16 otherInputs = 5;
    private Int16 numberOfOutputs = 3;
    private static Int16 numberOfHiddenLayers = 2;
    //private Vector2[] sightDirections = new Vector2[numberOfSights];
    private float r = 10; // distance of vision
    private float angleIncrement = 2 * Mathf.PI / numberOfSights;

    private Vector2 startPosition;
    private Quaternion startAngle;
    private Vector2 startVelocity;

    private Rigidbody2D rb;

    private List<PathTracker> path = new List<PathTracker>();
    private bool pause = false;
    private double runDuration;
    private double maxDuration = .5;

    // Use this for initialization
    void Start () {
        runDuration = 0;
        rb = GetComponent<Rigidbody2D>();

        // get initial state
        startPosition = transform.position;
        startAngle = transform.rotation;
        startVelocity = rb.velocity;
        
        //StartCoroutine(waitForCarPhysics());
        int i = 0;
        foreach(SightObjects o in sightList)
        {
            o.index = i++;
        }

        InitializeNetwork(numberOfSights, numberOfOutputs, numberOfHiddenLayers, true, new ArcTan());
        inputSightDistancesToNeuralNetwork();
    }

	void Update() {

	}
	
	void FixedUpdate () {
        runDuration += Time.deltaTime;
        double[] trueOutput = new double[numberOfOutputs];

        trueOutput = network.GetOutputs();

        PathTracker pt = new PathTracker(network.GetInputs(), network.GetOutputs());
        path.Add(pt);
        
        if(runDuration>maxDuration)
        {
            // good path
            pause = true;
            PathTracker p = path[0];
            network.forwardPropogate(p.getInputs());
            network.backPropogate(new double[] { network.activationFunc.max() }, (runDuration > maxDuration ? maxDuration : (maxDuration - runDuration) / maxDuration)); //(runDuration > maxDuration ? 0.01 : Math.Abs(runDuration - maxDuration) / maxDuration));
            //Debug.Log((p.getOutputs()[0] - 1d).ToString() + " (error) " + runDuration.ToString());
            path.RemoveAt(0);
            pause = false;
        }
        
        double[] outputs = network.GetMovementOutputs();
        
		float driftFactor = driftFactorSticky;
		if(RightVelocity().magnitude > maxStickyVelocity) {
			driftFactor = driftFactorSlippy;
		}

		rb.velocity = ForwardVelocity() + RightVelocity()*driftFactor;
        
		if(Input.GetButton("Accelerate") || (useAI == true)) {
			rb.AddForce( transform.up * speedForce );

			// Consider using rb.AddForceAtPosition to apply force twice, at the position
			// of the rear tires/tyres
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
        float smoothing = CustomInputSmoothing((float)outputs[1], (float)outputs[2]);
        rb.angularVelocity = smoothing * tf;// * (outputs[1] < network.activationFunc.cutoff() ? -1f : 1f);//Input.GetAxis("Horizontal") * tf;
        //Debug.Log("smooth " + smoothing.ToString());
        inputSightDistancesToNeuralNetwork();
        network.printNN();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.layer == LayerMask.NameToLayer("Edges"))
        {
            double[] trueOutput = new double[numberOfOutputs];
            trueOutput = network.GetOutputs();

            PathTracker pt = new PathTracker(network.GetInputs(), network.GetOutputs());
            path.Add(pt);

            rb.angularVelocity = 0;
            transform.position = startPosition;
            transform.rotation = startAngle;
            rb.velocity = startVelocity;
            pause = true;
            foreach (PathTracker p in path)
            {
                network.forwardPropogate(p.getInputs());

                network.backPropogate(new double[] { network.activationFunc.min() }, (runDuration > maxDuration ? maxDuration : (maxDuration - runDuration) / maxDuration)); //(runDuration > maxDuration ? .01 : Math.Abs(runDuration - maxDuration) / maxDuration));
                //Debug.Log((p.getOutputs()[0] - 1d).ToString() + " (error) " + runDuration.ToString());
            }
            path.Clear();
            pause = false;

            runDuration = 0;

            //network.initializeWeightsMinor();
        }
    }

    private void inputSightDistancesToNeuralNetwork()
    {
        // update neural network
        float carAngle = Mathf.Atan2(transform.right.y, transform.right.x);
        double[] sightDistances = new double[numberOfSights+otherInputs];
        for (int i = 0; i < numberOfSights; i++)
        {
            float x = transform.position.x + r * Mathf.Cos(carAngle + angleIncrement * i);
            float y = transform.position.y + r * Mathf.Sin(carAngle + angleIncrement * i);

            Vector2 sightVec = new Vector2(x, y);
            var hit = Physics2D.Linecast(transform.position, sightVec, 1 << LayerMask.NameToLayer("Edges"));

            if (hit.collider != null)
            {
                Debug.DrawLine(transform.position, hit.point, Color.red);
                sightDistances[i] = hit.distance;
            }
            else
            {
                sightDistances[i] = r;
                Debug.DrawLine(transform.position, sightVec, Color.green);
            }
        }

        sightDistances[numberOfSights] = rb.velocity.x;
        sightDistances[numberOfSights + 1] = rb.velocity.y;
        sightDistances[numberOfSights + 2] = transform.rotation.x;
        sightDistances[numberOfSights + 3] = transform.rotation.y;
        sightDistances[numberOfSights + 4] = rb.angularVelocity;

        // rb.velocity, transform.rotation, rb.angularvelocity
        network.forwardPropogate(sightDistances);
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
            target = direction-rightDirection;
        }
        else target = Input.GetAxisRaw("Horizontal");
        
        fValue = Mathf.MoveTowards(fValue, target, sensitivity * Time.deltaTime);

        if (fValue < -1) fValue = -1;
        if (fValue > 1) fValue = 1;

        return (Mathf.Abs(fValue) < dead) ? 0f : fValue;
    }

	Vector2 ForwardVelocity() {
		return transform.up * Vector2.Dot( GetComponent<Rigidbody2D>().velocity, transform.up );
	}

	Vector2 RightVelocity() {
		return transform.right * Vector2.Dot( GetComponent<Rigidbody2D>().velocity, transform.right );
	}

    private void InitializeNetwork(int inputNodes, int outputNodes, int numHiddenLayers, bool addBias, IActivationFunction af)
    {
        network = new Network();
        network.activationFunc = af;

        // create network
        Layer inpLayer = new Layer();
        for (int i = 0; i < inputNodes; i++)
        {
            Node n = new Node(nodeNamePrefix[0] + i.ToString());
            inpLayer.AddNode(n);
        }
        network.AddLayer(inpLayer);
        // create hidden layers
        for (int numLayer = 0; numLayer < numHiddenLayers; numLayer++)
        {
            Layer hiddenLayer = new Layer();

            string res = new String(nodeNamePrefix[(numLayer + 1) % nodeNamePrefix.Length], (numLayer + 1) / nodeNamePrefix.Length + 1);
            for (int i = 0; i < outputNodes; i++)
            {
                Node n = new Node(res + i.ToString());
                hiddenLayer.AddNode(n);
            }
            network.AddLayer(hiddenLayer);
        }
        // create connectors from input to hidden layers, and from hidden to hidden layers
        for (int i = 0; i < numHiddenLayers; i++)
        {
            foreach (Node n in network.layers[i].nodes)
            {
                foreach (Node n2 in network.layers[i + 1].nodes)
                {
                    if (n2.isBiasNode == false)
                    {
                        Connector con = new Connector(n, n2, 0.5d);
                        n.AddForwardConnector(con);
                        n2.AddBackwardConnector(con);
                    }
                }
            }
        }
        // create output layer
        Layer outLayer = new Layer();

        string reso = new String(nodeNamePrefix[(numHiddenLayers + 1) % nodeNamePrefix.Length], (numHiddenLayers + 1) / nodeNamePrefix.Length + 1);
        for (int i = 0; i < outputNodes; i++)
        {
            Node n = new Node(reso + i.ToString());
            outLayer.AddNode(n);
        }
        network.AddLayer(outLayer);
        // create connectors from hidden layer to output layer
        foreach (Node n in network.layers[numHiddenLayers].nodes)
        {
            foreach (Node n2 in outLayer.nodes)
            {
                Connector con = new Connector(n, n2, 0.5d);
                n.AddForwardConnector(con);
                n2.AddBackwardConnector(con);
                //n2.output = 1;
            }
        }

        // create final output layer with 1 node
        Layer finalLayer = new Layer();
        Node finalnode = new Node("final");
        finalLayer.AddNode(finalnode);
        network.AddLayer(finalLayer);
        foreach(Node n in outLayer.nodes)
        {
            Connector con = new Connector(n, finalnode, 0.5d);
            n.AddForwardConnector(con);
            finalnode.AddBackwardConnector(con);
        }

        if (addBias == true)
        {
            Node biasnode = new Node("bias", 1d, true);
            // connect bias node to other nodes excluding input layer
            for (int i = 1; i < network.layers.Count; i++)
            {
                foreach (Node n in network.layers[i].nodes)
                {
                    Connector con = new Connector(biasnode, n, 0.5d);
                    biasnode.AddForwardConnector(con);
                    n.AddBackwardConnector(con);
                }
            }
            network.biasnode = biasnode;
        }
        else
        {
            network.biasnode = new Node("bias", 0d, true);
        }

        // initialize
        network.initializeWeights();
    }
}
