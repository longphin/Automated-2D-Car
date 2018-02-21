using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#region Activation functions
public interface ActivationFunction
{
    double[] activate(double[] x);
    double[,] initializeWeightMatrix(int n, int m);
    double[] initializeBias(int n);
}

public class AF_Relu : ActivationFunction
{
    public double[] activate(double[] x)
    {
        double[] res = new double[x.Length];

        for (int i = 0; i < x.Length; i++)
        {
            res[i] = Utils.Max(0d, x[i]);
        }

        return (res);
    }

    public double[,] initializeWeightMatrix(int n, int m) // creates an n by m matrix
    {
        double[,] weightMatrix = new double[n, m]; // matrix of thislayer.nodes x prevlayer.nodes
        for (int j = 0; j < n; j++)
        {
            for (int k = 0; k < m; k++)
            {
                double randWeight = Utils.GetRandomDbl()*Math.Sqrt(2/m);
                weightMatrix[j, k] = randWeight;
            }
        }

        // bias vector
        double[] bias = new double[n];
        for (int j = 0; j < n; j++)
        {
            bias[j] = Utils.GetRandomDbl();
        }

        return (weightMatrix);
    }

    public double[] initializeBias(int n)
    {
        // bias vector
        double[] bias = new double[n];
        for (int j = 0; j < n; j++)
        {
            bias[j] = Utils.GetRandomDbl();
        }

        return (bias);
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

    public double[,] initializeWeightMatrix(int n, int m) // creates an n by m matrix
    {
        double[,] weightMatrix = new double[n, m]; // matrix of thislayer.nodes x prevlayer.nodes
        for (int j = 0; j < n; j++)
        {
            for (int k = 0; k < m; k++)
            {
                double randWeight = Utils.GetRandomDbl() * Math.Sqrt(1/m);
                weightMatrix[j, k] = randWeight;
            }
        }
        return (weightMatrix);
    }

    public double[] initializeBias(int n)
    {
        // bias vector
        double[] bias = new double[n];
        for (int j = 0; j < n; j++)
        {
            bias[j] = Utils.GetRandomDbl();
        }

        return (bias);
    }
}
#endregion

// Layers
public class Layer_new
{
    private double[,] weightMatrix;
    private double[] bias;
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

    public Layer_new(int n, int m, ActivationFunction af)
    {
        this.weightMatrix = af.initializeWeightMatrix(n, m);
        this.bias = af.initializeBias(n);

        this.z = new double[n];
        this.a = new double[n];
    }

    public string weightsAsString()
    {
        int n = weightMatrix.GetLength(0);
        int m = weightMatrix.GetLength(1);

        string res = n.ToString() + " " + m.ToString();

        for(int i = 0; i<n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                res += " " + weightMatrix[i, j].ToString();
            }
        }

        return (res);
    }

    public string biasAsString()
    {
        string res = String.Empty;

        for(int i = 0; i<bias.Length; i++)
        {
            res += " " + bias[i].ToString();
        }

        return (res);
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

    public double[] getBias()
    {
        return (bias);
    }

    public double[,] getWeightMatrix()
    {
        return (weightMatrix);
    }
}

// Neural network remake
public class NeuralNetwork_new
{
    List<Layer_new> layers = new List<Layer_new>();

    int L; // number of layers
    int[] N; // number of nodes per layer

    // Use this for initialization
    void Start()
    {
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
        for (int i = 0; i < nodes1.Length; i++)
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
            else newLayer.setAF(new AF_Relu());
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
        return (layers[layer].getWeightMatrix());
    }
    public double[] getBias(int layer)
    {
        return (layers[layer].getBias());
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
                        weightMatrix[j, k] = Utils.GetRandomDbl() * 2 - 1; // make between [-1,1]
                    }
                    else
                    {
                        //weightMatrix[j, k] = (weightMatrix1[j, k] + weightMatrix2[j, k]) / 2d; // take the average

                        if (Utils.GetRandomDbl() < (0.5d + 0.1d * (checkpointsDone1 - checkpointsDone2))) // Get weight from parent 1
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
                    bias[j] = Utils.GetRandomDbl() * CarsControllerHelper.carMaxSightRange * 2 - CarsControllerHelper.carMaxSightRange; // make between [-carMaxSightRange, carMaxSightRange]
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
        for (int i = 1; i < L; i++)
        {
            if (i == L - 1)
            {
                Layer_new newLayer = new Layer_new(N[i], N[i - 1], new AF_Tanh());
                layers.Add(newLayer); // output layer
            }
            else
            {
                Layer_new newLayer = new Layer_new(N[i], N[i - 1], new AF_Relu());
                layers.Add(newLayer); // hidden layer
            }
        }
    }

    public void forwardPropogate(Layer_new input)
    {
        // Initialize new inputs
        setInputLayer(input);

        // Perform forward propogation.
        for (int i = 1; i < layers.Count; i++)
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

    public void printNN()
    {
        for(int i = 0; i<layers.Count; i++)
        {
            double[] a = layers[i].getA();
            Debug.Log("layer " + i.ToString());

            for (int j =0; j<a.Length; j++)
            {
                Debug.Log(a[j].ToString());
            }
        }
    }

    //[TODO]
    public void writeNeuralNetworkToFile()
    {
        string path = @"Assets\Resources\bestNN.txt";

        /*
        // Write data to file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine("line");
        writer.Close();
        */
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
        {
            for (int i = 1; i < layers.Count; i++)
            {
                file.WriteLine(layers[i].weightsAsString());
                file.WriteLine(layers[i].biasAsString());
            }
        }
    }
    //[TODO]
    public void readNeuralNetworkFromFile()
    {

    }
}
