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

    public static int GetRandomInt()
    {
        return (randomGenerator.Next());
    }

    // return random int between [a, b)
    public static int GetRandomInt(int a, int b)
    {
        return (randomGenerator.Next(a, b));
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

    public static double[] AddVectors(double[] x, double[] y)
    {
        if (x.Length != y.Length) throw new InvalidOperationException("Cannot add arrays of different sizes");

        double[] res = new double[x.Length];

        for (int i = 0; i < x.Length; i++)
        {
            res[i] = x[i] + y[i];
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
    public double[] bias;
    private ActivationFunction af = new AF_Relu();
    private double[] z;
    private double[] a;
    
    // Constructor for non-input layers.
    public Layer_new(double[,] weights, double[] bias)
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
    
    public void calculateFit(double[] a)
    {
        this.z = Utils.AddVectors(Utils.MatrixTimesVector(weightMatrix, a), bias);
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
public class NeuralNetwork_new{
    List<Layer_new> layers = new List<Layer_new>();

    int L; // number of layers
    int[] N; // number of nodes per layer

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

    // constructor for combining 2 neural networks
    public NeuralNetwork_new(NeuralNetwork_new NN1, NeuralNetwork_new NN2, float mutationRate, int checkpointsDone1, int checkpointsDone2)
    {
        int[] nodes1 = NN1.getN();
        int[] nodes2 = NN2.getN();
        if (NN1.getL() != NN2.getL() || nodes1.Length != nodes2.Length) throw new ArgumentOutOfRangeException("Cannot combine neural networks of different sizes.");
        for(int i = 0; i<nodes1.Length; i++)
        {
            if (nodes1[i] != nodes2[i]) throw new ArgumentOutOfRangeException("Cannot combine neural networks of different layer sizes.");
        }

        this.L = NN1.getL();
        this.N = NN1.getN();

        initializeChildLayers(NN1, NN2, mutationRate, checkpointsDone1, checkpointsDone2);
    }

    // clone constructor
    public NeuralNetwork_new(NeuralNetwork_new NN)
    {
        copyLayers(NN);
    }

    private void copyLayers(NeuralNetwork_new NN)
    {
        layers.Add(new Layer_new(new double[] { 1.0 })); // input layer
        for (int i = 1; i < L; i++)
        {
            double[,] weightMatrix = new double[N[i], N[i - 1]]; // matrix of thislayer.nodes x prevlayer.nodes
            double[,] weightMatrix1 = NN.getWeightMatrix(i);

            for (int j = 0; j < N[i]; j++)
            {
                for (int k = 0; k < N[i - 1]; k++)
                {
                    weightMatrix[j, k] = weightMatrix1[j, k];
                }
            }

            // bias vector
            double[] bias = new double[N[i]];
            double[] bias1 = NN.getBias(i);
            for (int j = 0; j < N[i]; j++)
            {
                bias[j] = bias1[j];
            }

            Layer_new newLayer = new Layer_new(weightMatrix, bias); // output layer
            if (i == L - 1) newLayer.setAF(new AF_Tanh());
            layers.Add(newLayer); // hidden layer
        }
    }

    public int getL()
    {
        return (L);
    }

    public int[] getN()
    {
        return (N);
    }

    public double[,] getWeightMatrix(int layer)
    {
        return (layers[layer].weightMatrix);
    }
    public double[] getBias(int layer)
    {
        return (layers[layer].bias);
    }

    private void initializeChildLayers(NeuralNetwork_new NN1, NeuralNetwork_new NN2, float mutationRate, int checkpointsDone1, int checkpointsDone2)
    {
        layers.Add(new Layer_new(new double[] { 1.0 })); // input layer
        for (int i = 1; i < L; i++)
        {
            double[,] weightMatrix = new double[N[i], N[i - 1]]; // matrix of thislayer.nodes x prevlayer.nodes
            double[,] weightMatrix1 = NN1.getWeightMatrix(i);
            double[,] weightMatrix2 = NN2.getWeightMatrix(i);

            for (int j = 0; j < N[i]; j++)
            {
                for (int k = 0; k < N[i - 1]; k++)
                {
                    if (Utils.GetRandomDbl() < mutationRate) // mutate weight
                    {
                        weightMatrix[j, k] = Utils.GetRandomDbl()*2-1; // make between [-1,1]
                    }
                    else
                    {
                        //weightMatrix[j, k] = (weightMatrix1[j, k] + weightMatrix2[j, k]) / 2d; // take the average
                        
                        if (Utils.GetRandomDbl() < (0.5d + 0.1d*(checkpointsDone1-checkpointsDone2))) // Get weight from parent 1
                        {
                            weightMatrix[j, k] = weightMatrix1[j, k];
                        }
                        else // else, get weight from parent 2
                        {
                            weightMatrix[j, k] = weightMatrix2[j, k];
                        }
                        
                    }
                }
            }

            // bias vector
            double[] bias = new double[N[i]];
            double[] bias1 = NN1.getBias(i);
            double[] bias2 = NN2.getBias(i);
            for (int j = 0; j < N[i]; j++)
            {
                if (Utils.GetRandomDbl() < mutationRate) // mutation
                {
                    bias[j] = Utils.GetRandomDbl()*20-10; // make between [-10,10]
                }
                else
                {
                    //bias[j] = (bias1[j] + bias2[j]) / 2d; // take the average
                    
                    if (Utils.GetRandomDbl() < (0.5d + 0.1d * (checkpointsDone1 - checkpointsDone2))) // Get bias from parent 1
                    {
                        bias[j] = bias1[j];
                    }
                    else // Get bias from parent 2
                    {
                        bias[j] = bias2[j];
                    }
                    
                }
            }

            Layer_new newLayer = new Layer_new(weightMatrix, bias); // output layer
            if (i == L - 1) newLayer.setAF(new AF_Tanh());
            layers.Add(newLayer); // hidden layer
        }
        // output layer
    }

    private void initializeLayers()
    {
        layers.Add(new Layer_new(new double[] { 1.0 })); // input layer
        for(int i=1; i<L; i++)
        {
            double[,] weightMatrix = new double[N[i], N[i-1]]; // matrix of thislayer.nodes x prevlayer.nodes
            for(int j=0; j<N[i]; j++)
            {
                //double rowTotal = 0;
                for(int k=0; k<N[i-1]; k++)
                {
                    double randWeight = Utils.GetRandomDbl();
                    weightMatrix[j,k] = randWeight;
                    //rowTotal += randWeight;
                }
                // normalize the row so the total weight = 1
                /*
                for(int k=0; k<N[i-1]; k++)
                {
                    weightMatrix[j, k] = weightMatrix[j, k] / rowTotal;
                }
                */
            }

            // bias vector
            double[] bias = new double[N[i]];
            for(int j=0; j<N[i]; j++)
            {
                bias[j] = Utils.GetRandomDbl();
            }
            
            Layer_new newLayer = new Layer_new(weightMatrix, bias); // output layer
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
