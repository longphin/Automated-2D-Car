using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    private static System.Random randomGenerator = new System.Random();

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

// Layers
public class Layer_new
{
    public List<double> weights = new List<double>();
    public double bias = 0d;
    private ActivationFunction af = new AF_Relu();
    private List<double> z = new List<double>();
    private List<double> a = new List<double>();
    
    // Constructor for non-input layers.
    public Layer_new(List<double> weights, double bias)
    {
        this.weights = weights;
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
        this.z = Utils.AddConstantToArray(
                    Utils.ArrayMultiplication(weights, a), bias
                );
    }

    public void calculateAF()
    {
        this.a = this.af.activate(this.z);
    }

    public List<double> getA()
    {
        return (this.a);
    }
}

// Neural network remake
// [TODO] need to finish
public class NeuralNetwork_new : MonoBehaviour {
    List<Layer_new> layers = new List<Layer_new>();
    int L = 3; // number of layers
    int n = 45; // number of nodes per layer [TODO] make it possible for layers for have different n

	// Use this for initialization
	void Start () {
        initializeLayers();
	}
	
    private void initializeLayers()
    {
        layers.Add(new Layer_new(new List<double> { 1.0 })); // input layer
        for(int i=0; i<L; i++)
        {
            List<double> weights = new List<double>();
            for(int j=0; j<n; j++)
            {
                weights.Add(Utils.GetRandomDbl());
            }
            layers.Add(new Layer_new(weights, 1.0));
        }
    }

    // [TODO] add forward propogation

	// Update is called once per frame
	void Update () {
		
	}

    public void setInputLayer(Layer_new input)
    {
        if (layers.Count <= 0) throw new IndexOutOfRangeException("Neural Network has no layers to put an input in.");

        layers[0] = input;
    }
}
