using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    private static System.Random randomGenerator = new System.Random(1991);

    public static List<double> ArrayMultiplication(List<double> x, List<double> y)
    {
        if (x.Count != y.Count) throw new InvalidOperationException("Cannot multiply arrays of different sizes");

        List<double> res = new List<double>();
        for(int i=0; i<x.Count; i++)
        {
            res.Add(x[i] * y[i]);
        }

        return (res);
    }

    public static List<double> AddConstantToArray(List<double> x, double c)
    {
        for(int i=0; i<x.Count; i++)
        {
            x[i] = x[i] + c;
        }

        return (x);
    }

    public static double Max(double x1, double x2)
    {
        if (x1 > x2) return (x1);
        return (x2);
    }

    public static double GetRandomDbl()
    {
        return (randomGenerator.NextDouble());
    }

    public static List<double> MatrixTimesVector(List<List<double>> matrix, List<double> vec)
    {
        List<double> res = new List<double>();

        foreach(List<double> row in matrix)
        {
            for(int i = 0; i<row.Count; i++)
            {
                res.Add(row[i] * vec[i]);
            }
        }

        return (res);
    }
}

public interface ActivationFunction
{
    List<double> activate(List<double> x);
}

public class AF_Relu : ActivationFunction
{
    public List<double> activate(List<double> x)
    {
        List<double> res = new List<double>();

        for(int i=0; i<x.Count; i++)
        {
            res.Add(Utils.Max(0d, x[i]));
        }

        return (res);
    }
}

public class AF_Tanh : ActivationFunction
{
    public List<double> activate(List<double> x)
    {
        List<double> res = new List<double>();

        for (int i = 0; i < x.Count; i++)
        {
            //res.Add(Utils.Max(0d, x[i]));
            var ex = Math.Exp(2 * x[i]);
            if ((ex + 1) == 0) throw new DivideByZeroException("Sigmoid cannot divide by 0.");
            res.Add((ex - 1) / (ex + 1));
        }

        return (res);
    }
}

// Layers
public class Layer_new
{
    public List<List<double>> weightMatrix = new List<List<double>>();
    public double bias = 0d; // [TODO] change to a vector of length N[i]
    private ActivationFunction af = new AF_Relu();
    private List<double> z = new List<double>();
    private List<double> a = new List<double>();
    
    // Constructor for non-input layers.
    public Layer_new(List<List<double>> weights, double bias)
    {
        this.weightMatrix = weights;
        this.bias = bias;
    }

    // Constructor for input layer only.
    public Layer_new(List<double> a)
    {
        this.a = a;
    }

    public void setAF(ActivationFunction af)
    {
        this.af = af;
    }

    // [TODO] When doing forward propogation, use layer[i].calculateFit(layer[i-1].getA())
    public void calculateFit(List<double> a)
    {
        this.z = Utils.MatrixTimesVector(weightMatrix, a); // [TODO] add a bias vector
        /*
        this.z = Utils.AddConstantToArray(
                    Utils.ArrayMultiplication(weights, a), bias
                );
        */
    }

    public void calculateAF()
    {
        this.a = this.af.activate(this.z);
    }

    public List<double> getA()
    {
        return (this.a);
    }

    public void forwardPropogate(List<double> prevLayera)
    {
        calculateFit(prevLayera);
        calculateAF();
    }
}

// Neural network remake
// [TODO] need to finish
public class NeuralNetwork_new : MonoBehaviour {
    List<Layer_new> layers = new List<Layer_new>();

    int L;// = 3; // number of layers
    int[] N;// = new int[] { 45, 45, 2 }; // number of nodes per layer [TODO] make it possible for layers for have different n. This should have length L

	// Use this for initialization
	void Start () {
        //initializeLayers();
	}
	
    public NeuralNetwork_new(int L, int[] N)
    {
        this.L = L;
        this.N = N;

        initializeLayers();
    }

    private void initializeLayers()
    {
        layers.Add(new Layer_new(new List<double> { 1.0 })); // input layer
        for(int i=1; i<L; i++)
        {
            List<List<double>> weightMatrix = new List<List<double>>();
            for(int j=0; j<N[i-1]; j++)
            {
                List<double> row = new List<double>();
                for(int k=0; k<N[i]; k++)
                {
                    row.Add(Utils.GetRandomDbl());
                }
                weightMatrix.Add(row);
            }
            
            Layer_new newLayer = new Layer_new(weightMatrix, 1.0);
            if (i == L - 1) newLayer.setAF(new AF_Tanh());
            layers.Add(newLayer); // hidden layer
        }
        // output layer
    }

    // [TODO] add forward propogation

	// Update is called once per frame
	void Update () {
		
	}

    public void forwardPropogate(Layer_new input)
    {
        // Initialize new inputs
        setInputLayer(input);

        // Perform forward propogation.
        for(int i=1; i<layers.Count; i++)
        {
            layers[i].forwardPropogate(layers[i - 1].getA());
        }
    }

    private void setInputLayer(Layer_new input)
    {
        if (layers.Count <= 0) throw new IndexOutOfRangeException("Neural Network has no layers to put an input in.");

        layers[0] = input;
    }

    public void printOutputs()
    {
        Debug.Log(layers[layers.Count - 1].getA()[0].ToString() + ", " + layers[layers.Count - 1].getA()[1].ToString());
    }

    public List<double> getOutputs()
    {
        return (layers[layers.Count - 1].getA());
    }
}
