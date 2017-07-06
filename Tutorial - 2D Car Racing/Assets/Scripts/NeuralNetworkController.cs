﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector
{
    public Node From { get; set; }
    public Node To { get; set; }
    public double weight { get; set; }

    public Connector(Node From, Node To, double weight)
    {
        this.From = From;
        this.To = To;
        this.weight = weight;
    }
}

public class Node
{
    public double weightedSum { get; set; }
    public double output { get; set; }
    public List<Connector> forwardConnectors { get; set; }
    public List<Connector> backwardConnectors { get; set; }
    public string name { get; set; }
    public double error { get; set; }
    public bool isBiasNode { get; set; }

    public Node()
    {
        weightedSum = 0;
        forwardConnectors = new List<Connector>();
        backwardConnectors = new List<Connector>();
        isBiasNode = false;
        name = "undef";
    }

    public Node(string name)
    {
        weightedSum = 0;
        forwardConnectors = new List<Connector>();
        backwardConnectors = new List<Connector>();
        isBiasNode = false;
        this.name = name;
    }

    public Node(string name, double output, bool isBiasNode)
    {
        this.name = name;
        this.output = output;
        this.isBiasNode = isBiasNode;
    }

    public void AddForwardConnector(Connector newConnector)
    {
        forwardConnectors.Add(newConnector);
    }
    public void AddBackwardConnector(Connector newConnector)
    {
        backwardConnectors.Add(newConnector);
    }
}

public class Layer
{
    public List<Node> nodes { get; set; }

    public Layer()
    {
        nodes = new List<Node>();
    }

    public void AddNode(Node node)
    {
        nodes.Add(node);
    }
}

public interface IActivationFunction
{
    double activationFunction(double x);
    double activationFunction_Prime(double x);
}

public class Network
{
    public List<Layer> layers { get; set; }
    public IActivationFunction activationFunc { get; set; }
    private static readonly System.Random getrandom = new System.Random(250);
    public static double alpha { get; set; }
    public Node biasnode { get; set; }// = new Node();

    public Network()
    {
        layers =  new List<Layer>();
        alpha = 0.05d;
    }

    public void AddLayer(Layer layer)
    {
        layers.Add(layer);
    }

    private double GetRandomDouble(System.Random rnd, double min, double max)
    {
        return (rnd.NextDouble() * (max - min) + min);
    }

    public void initializeWeights()
    {
        foreach (Layer layer in layers)
        {
            foreach (Node node in layer.nodes)
            {
                foreach (Connector connector in node.forwardConnectors)
                {
                    connector.weight = GetRandomDouble(getrandom, -1d / Math.Sqrt((double)layer.nodes.Count), 1d / Math.Sqrt((double)layer.nodes.Count));//(double)getrandom.Next(1,100)/(double)200;
                }
            }
        }
        foreach (Connector con in biasnode.forwardConnectors)
        {
            con.weight = GetRandomDouble(getrandom, -.3, .3); //(double)getrandom.Next(1, 100) / (double)200;
        }
    }

    public void forwardPropogate(double[] input)
    {
        // initialize input layer
        int nonBiasNodeIterator = 0;
        for (int i = 0; i < layers[0].nodes.Count; i++)
        {
            if (layers[0].nodes[i].isBiasNode == false)
            {
                layers[0].nodes[i].output = input[nonBiasNodeIterator];
                nonBiasNodeIterator += 1;
            }
        }

        if (layers.Count < 2) return; // If there is only 1 layer (which shouldn't happen), then do nothing.

        // forward propogate
        for (int i = 1; i < layers.Count; i++)
        {
            foreach (Node node in layers[i].nodes)
            {
                if (node.isBiasNode == false)
                {
                    double sum = 0;
                    foreach (Connector con in node.backwardConnectors)
                    {
                        sum += con.weight * con.From.output;
                    }
                    node.weightedSum = sum;
                    node.output = calcActivationFunc(sum);
                }
            }
        }
    }

    public void backPropogate(double[] output)
    {
        // initialize error layer
        double TotalError = 0;
        int nonBiasNodeIterator = 0;
        for (int i = 0; i < layers[layers.Count - 1].nodes.Count; i++)
        {
            Node node = layers[layers.Count - 1].nodes[i];
            if (node.isBiasNode == false) // this check shouldn't be needed
            {
                node.error = calcActivationFunc_Prime(node.weightedSum) * (output[nonBiasNodeIterator] - node.output);

                TotalError += node.error;
                nonBiasNodeIterator += 1;
            }
        }
        // back propogate
        for (int i = layers.Count - 2; i >= 0; i--)
        {
            foreach (Node node in layers[i].nodes)
            {
                double sum = 0;
                foreach (Connector con in node.forwardConnectors)
                {
                    sum += con.weight * con.To.error;
                }
                node.error = calcActivationFunc_Prime(node.weightedSum) * sum;
            }
        }

        // update all weights in network using errors
        foreach (Layer layer in layers)
        {
            foreach (Node node in layer.nodes)
            {
                foreach (Connector con in node.forwardConnectors)
                {
                    con.weight += alpha * node.output * con.To.error;
                }
            }
        }
        // update weights for bias node
        foreach (Connector con in biasnode.forwardConnectors)
        {
            con.weight += alpha * con.To.error;
        }
    }

    public double calcActivationFunc(double x)
    {
        return (activationFunc.activationFunction(x));
    }
    public double calcActivationFunc_Prime(double x)
    {
        return (activationFunc.activationFunction_Prime(x));
    }
    /*
    public void Test(IDataTest data)
    {
        if (layers.Count < 2) return; // If there is only 1 layer (which shouldn't happen), then do nothing.

        for (int j = 0; j < data.inputList.Count; j++)
        {
            forwardPropogate(data.inputList[j]);

            // print out results
            double TotalError = 0d;
            int nonBiasNodeIterator = 0;
            for (int i = 0; i < layers[layers.Count - 1].nodes.Count; i++)
            {
                Node node = layers[layers.Count - 1].nodes[i];
                if (node.isBiasNode == false)
                {
                    //Console.WriteLine(node.name + " guess " + node.output.ToString() + " : " + data.outputList[j].output[nonBiasNodeIterator].ToString());
                    TotalError += Math.Pow(node.output - data.outputList[j].output[nonBiasNodeIterator], 2);
                    nonBiasNodeIterator += 1;
                }
            }
            //Console.WriteLine("Total Error: " + TotalError.ToString());
        }
    }
    */
}

public class NeuralNetworkController : MonoBehaviour
{
    void Start()
    {

    }
}