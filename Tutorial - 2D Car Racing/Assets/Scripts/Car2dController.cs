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
    private bool useAI = false; // AI with neural network not fully implemented
    private bool turnRight = false, turnLeft = false, turnForward = false, turnBackward = false;
    private float rightspeed = 0f;
    private float leftspeed = 0f;
    private int rightpressLength = 0, leftpressLength = 0;

    // sight directions
    private Vector2[] sightDirections = new Vector2[180-1]; // 180 points. The -1 is because array is 0-based

    public Transform testTransform;

    public List<SightObjects> sightList = new List<SightObjects>();

    // Use this for initialization
    void Start () {
        //StartCoroutine(waitForCarPhysics());
        int i = 0;
        foreach(SightObjects o in sightList)
        {
            o.index = i++;
        }

        InitializeNetwork(8, 2, 2, true, new ArcTan());
    }

	void Update() {
        // check for button up/down here, then set a bool that you will use in FixedUpdate
        //Raycasting(sightStartT, sightEndT, indicatorT);
        foreach(SightObjects o in sightList)
        {
            //Debug.Log(o.index.ToString() + " " + o.GetDistToHit());
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Rigidbody2D rb = GetComponent<Rigidbody2D>();

		float driftFactor = driftFactorSticky;
		if(RightVelocity().magnitude > maxStickyVelocity) {
			driftFactor = driftFactorSlippy;
		}

		rb.velocity = ForwardVelocity() + RightVelocity()*driftFactor;

		if(Input.GetButton("Accelerate") || (useAI == true && turnForward == true)) {
			rb.AddForce( transform.up * speedForce );

			// Consider using rb.AddForceAtPosition to apply force twice, at the position
			// of the rear tires/tyres
		}
		if(Input.GetButton("Brakes") || (useAI == true && turnBackward == true)) {
			rb.AddForce( transform.up * -speedForce/2f );
            
			// Consider using rb.AddForceAtPosition to apply force twice, at the position
			// of the rear tires/tyres
		}

		// If you are using positional wheels in your physics, then you probably
		// instead of adding angular momentum or torque, you'll instead want
		// to add left/right Force at the position of the two front tire/types
		// proportional to your current forward speed (you are converting some
		// forward speed into sideway force)
		float tf = Mathf.Lerp(0, torqueForce, rb.velocity.magnitude / 2);
        //rb.angularVelocity = Input.GetAxis("Horizontal") * tf;
        if(Input.GetButton("Right")) {
            rightpressLength += 1;
            leftspeed = 0f;
            rightspeed += .03f + 0.0001f*rightpressLength;
            if (rightspeed > 1) rightspeed = 1;

            rb.angularVelocity = rightspeed * tf;
        }
        else
        {
            rightpressLength -= 1;
            if (rightpressLength < 0) rightpressLength = 0;
            rightspeed = 0f;
        }
        if (Input.GetButton("Left"))
        {
            leftpressLength += 1;
            rightspeed = 0f;
            leftspeed += .03f + 0.0001f * leftpressLength;
            if (leftspeed > 1) leftspeed = 1;

            rb.angularVelocity = -leftspeed * tf;
        }
        else
        {
            leftpressLength -= 1;
            if (leftpressLength < 0) leftpressLength = 0;
            leftspeed = 0f;
        }

        // update neural network
        float angleIncrement = 2 * Mathf.PI / sightDirections.Length;
        float r = 10; // distance of vision

        float carAngle = Mathf.Atan2(transform.right.y, transform.right.x);

        for (int i=0; i<sightDirections.Length; i++)
        {
            float x = transform.position.x + r * Mathf.Cos(carAngle + angleIncrement * i);
            float y = transform.position.y + r * Mathf.Sin(carAngle + angleIncrement * i);

            Vector2 sightVec = new Vector2(x, y);
            //var hit = Physics2D.Raycast(transform.position, sightVec, 1 << LayerMask.NameToLayer("Edges"));
            var hit = Physics2D.Linecast(transform.position, sightVec, 1 << LayerMask.NameToLayer("Edges"));

            if (hit.collider != null)
            {
                //Debug.Log(hit.collider.gameObject.name);
                Debug.DrawLine(transform.position, hit.point, Color.red);
            }
            else
            {
                //Debug.Log("No hit");
                Debug.DrawLine(transform.position, sightVec, Color.green);
            }
        }
        
    }

	Vector2 ForwardVelocity() {
		return transform.up * Vector2.Dot( GetComponent<Rigidbody2D>().velocity, transform.up );
	}

	Vector2 RightVelocity() {
		return transform.right * Vector2.Dot( GetComponent<Rigidbody2D>().velocity, transform.right );
	}

    void Raycasting(Transform startpoint, Transform endpoint)
    {
        var hit = Physics2D.Linecast(startpoint.position, endpoint.position, 1 << LayerMask.NameToLayer("Edges"));

        if (hit.collider != null)
        {
            Debug.DrawLine(startpoint.position, hit.point, Color.green);
        }
        else
        {
            Debug.DrawLine(startpoint.position, endpoint.position, Color.gray);
        }
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
            for (int i = 0; i < inputNodes; i++)
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
            }
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
