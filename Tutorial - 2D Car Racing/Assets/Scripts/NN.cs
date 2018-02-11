using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    private static System.Random randomGenerator = new System.Random(1991);

    public static double[] ArrayMultiplication(double[] x, double[] y)
    {
        if(x.Length != y.Length) throw new InvalidOperationException("Cannot multiply arrays of different sizes");

        double[] res = new double[x.Length];

        for (int i = 0; i < x.Length; i++)
        {
            res[i] = x[i] * y[i];
        }

        return (res);
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

    public static double[] MatrixTimesVector(double[,] matrix, double[] vec)
    {
        double[] res = new double[matrix.GetLength(0)];

        for(int i=0; i<matrix.GetLength(0); i++)
        {
            double element = 0;
            for(int j=0; j<matrix.GetLength(1); j++)
            {
                element += matrix[i,j]*vec[j];
            }
            res[i] = element;
        }

        return (res);
    }
}

public interface ActivationFunction
{
    double[] activate(double[] x);
}

public class AF_Relu : ActivationFunction
{
    public double[] activate(double[] x)
    {
        double[] res = new double[x.Length];

        for(int i=0; i<x.Length; i++)
        {
            res[i] = Utils.Max(0d, x[i]);
        }

        return (res);
    }
}

public class AF_Tanh : ActivationFunction
{
    public double[] activate(double[] x)
    {
        double[] res = new double[x.Length];

        for (int i = 0; i < x.Length; i++)
        {
            var ex = Math.Exp(2 * x[i]);
            if ((ex + 1) == 0) throw new DivideByZeroException("Sigmoid cannot divide by 0.");
            res[i] = (ex - 1) / (ex + 1);
        }

        return (res);
    }
}

// Layers
public class Layer_new
{
    public double[,] weightMatrix;
    public double bias = 0d; // [TODO] change to a vector of length N[i]
    private ActivationFunction af = new AF_Relu();
    private double[] z;
    private double[] a;
    
    // Constructor for non-input layers.
    public Layer_new(double[,] weights, double bias)
    {
        this.weightMatrix = weights;
        this.bias = bias;

        this.z = new double[weights.GetLength(0)];
        this.a = new double[weights.GetLength(0)];
    }

    // Constructor for input layer only.
    public Layer_new(double[] a)
    {
        this.a = a;
    }

    public void setAF(ActivationFunction af)
    {
        this.af = af;
    }

    // [TODO] When doing forward propogation, use layer[i].calculateFit(layer[i-1].getA())
    public void calculateFit(double[] a)
    {
        this.z = Utils.MatrixTimesVector(weightMatrix, a); // [TODO] add a bias vector
    }

    public void calculateAF()
    {
        this.a = this.af.activate(this.z);
    }

    public double[] getA()
    {
        return (this.a);
    }

    public void forwardPropogate(double[] prevLayera)
    {
        calculateFit(prevLayera);
        calculateAF();
    }
}

// Neural network remake
// [TODO] need to finish
public class NeuralNetwork_new{
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
        layers.Add(new Layer_new(new double[] { 1.0 })); // input layer
        for(int i=1; i<L; i++)
        {
            double[,] weightMatrix = new double[N[i], N[i-1]]; // matrix of thislayer.nodes x prevlayer.nodes
            for(int j=0; j<N[i]; j++)
            {
                double rowTotal = 0;
                for(int k=0; k<N[i-1]; k++)
                {
                    double randWeight = Utils.GetRandomDbl();
                    weightMatrix[j,k] = randWeight;
                    rowTotal += randWeight;
                }
                // normalize the row so the total weight = 1
                for(int k=0; k<N[i-1]; k++)
                {
                    weightMatrix[j, k] = weightMatrix[j, k] / rowTotal;
                }
            }
            
            Layer_new newLayer = new Layer_new(weightMatrix, 1.0); // output layer
            if (i == L - 1) newLayer.setAF(new AF_Tanh());
            layers.Add(newLayer); // hidden layer
        }
        // output layer
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

    public double[] getOutputs()
    {
        return (layers[layers.Count - 1].getA());
    }
}
