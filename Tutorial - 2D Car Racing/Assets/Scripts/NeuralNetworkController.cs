using System;
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
        weightedSum = 0;
        forwardConnectors = new List<Connector>();
        backwardConnectors = new List<Connector>();
        this.isBiasNode = isBiasNode;
        this.name = name;
        this.output = output;
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
    double cutoff();
    double activationFunctionNormalize(double x);
    double min();
    double max();
}
public class Sigmoid : IActivationFunction
{
    public double activationFunction(double x)
    {
        return (1 / (1 + Math.Pow(Math.E, -x)));
    }
    public double activationFunction_Prime(double x)
    {
        return (Math.Pow(Math.E, x) / Math.Pow(Math.Pow(Math.E, x) + 1, 2));
    }
    public double cutoff()
    {
        return (0.5d);
    }
    public double activationFunctionNormalize(double x)
    {
        return (2*(x - 0.5));
    }
    public double min()
    {
        return (0);
    }
    public double max()
    {
        return (1);
    }
}

public class HyperbolicTangent : IActivationFunction
{
    public double activationFunction(double x)
    {
        return ((1 - Math.Pow(Math.E, -2 * x)) / (1 + Math.Pow(Math.E, 2 * x)));
    }
    public double activationFunction_Prime(double x)
    {
        return (Math.Pow(Math.E, -2 * x) * (4 * Math.Pow(Math.E, 2 * x) - 2 * Math.Pow(Math.E, 4 * x) + 2) / Math.Pow(Math.Pow(Math.E, 2 * x) + 1, 2));
    }
    public double cutoff()
    {
        return (0d);
    }
    public double activationFunctionNormalize(double x)
    {
        return (x/(3-2*Math.Sqrt(2))); // assuming min = 0 (at x=0)
    }
    public double min()
    {
        return (-1000);
    }
    public double max()
    {
        return (3 - 2 * Math.Sqrt(2));
    }
}

public class ArcTan : IActivationFunction
{
    public double activationFunction(double x)
    {
        return (Math.Atan(x));
    }
    public double activationFunction_Prime(double x)
    {
        return (1 / (Math.Pow(x, 2) + 1));
    }
    public double cutoff()
    {
        return (0d);
    }
    public double activationFunctionNormalize(double x)
    {
        return (2*(x-0.5));///(3-2*Math.Sqrt(2))); // assuming min = 0
    }
    public double min()
    {
        return (-Math.PI / 2);
    }
    public double max()
    {
        return (Math.PI / 2);
    }
}

public class Sinusoidal : IActivationFunction
{
    public double activationFunction(double x)
    {
        return (Math.Sin(x));
    }
    public double activationFunction_Prime(double x)
    {
        return (Math.Cos(x));
    }
    public double cutoff()
    {
        return (0d);
    }
    public double activationFunctionNormalize(double x)
    {
        return (x);
    }
    public double min()
    {
        return (-1);
    }
    public double max()
    {
        return (1);
    }
}

public class Sinc : IActivationFunction
{
    public double activationFunction(double x)
    {
        if (x == 0) return (1);
        return (Math.Sin(x) / x);
    }
    public double activationFunction_Prime(double x)
    {
        if (x == 0) return (1);
        return (Math.Cos(x) / x - Math.Sin(x) / Math.Pow(x, 2));
    }
    public double cutoff()
    {
        return (0.6d);
        //return (0d);
    }
    public double activationFunctionNormalize(double x)
    {
        return ((x-(-0.2))/1.2);
    }
    public double min()
    {
        return (-.217234);
    }
    public double max()
    {
        return (1);
    }
}

public class Guassian : IActivationFunction
{
    public double activationFunction(double x)
    {
        return (Math.Pow(Math.E, -Math.Pow(x, 2)));
    }
    public double activationFunction_Prime(double x)
    {
        return (-2 * x * Math.Pow(Math.E, -Math.Pow(x, 2)));
    }
    public double cutoff()
    {
        return (0.5d);
        //return (0d);
    }
    public double activationFunctionNormalize(double x)
    {
        return (2*(x-0.5));
    }
    public double min()
    {
        return (0);
    }
    public double max()
    {
        return (1);
    }
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

    public double[] GetInputs()
    {
        double[] res = new double[layers[0].nodes.Count];
        int nonBiasNodeIterator = 0;
        for(int i = 0; i<layers[0].nodes.Count; i++)
        {
            Node node = layers[0].nodes[i];
            if (node.isBiasNode == false) // this check shouldn't be needed
            {
                res[nonBiasNodeIterator] = node.output;

                nonBiasNodeIterator += 1;
            }
        }
        return (res);
    }
    public double[] GetOutputs()
    {
        //layers[layers.Count - 1].nodes[1].output = layers[layers.Count - 1].nodes[1].output < 0 ? -1 : 1;
        double[] res = new double[layers[layers.Count-1].nodes.Count];
        int nonBiasNodeIterator = 0;
        for (int i = 0; i < layers[layers.Count - 1].nodes.Count; i++)
        {
            Node node = layers[layers.Count - 1].nodes[i];
            if (node.isBiasNode == false) // this check shouldn't be needed
            {
                res[nonBiasNodeIterator] = node.output;
                
                nonBiasNodeIterator += 1;
            }
        }

        return (res);
    }
    public double[] GetMovementOutputs()
    {
        //layers[layers.Count - 1].nodes[1].output = layers[layers.Count - 1].nodes[1].output < 0 ? -1 : 1;
        double[] res = new double[layers[layers.Count - 2].nodes.Count];
        int nonBiasNodeIterator = 0;
        for (int i = 0; i < layers[layers.Count - 2].nodes.Count; i++)
        {
            Node node = layers[layers.Count - 2].nodes[i];
            if (node.isBiasNode == false) // this check shouldn't be needed
            {
                res[nonBiasNodeIterator] = node.output;

                nonBiasNodeIterator += 1;
            }
        }

        return (res);
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

    public void initializeWeightsMinor()
    {
        foreach (Layer layer in layers)
        {
            foreach (Node node in layer.nodes)
            {
                foreach (Connector connector in node.forwardConnectors)
                {
                    connector.weight = connector.weight*GetRandomDouble(getrandom, 0d, 0.1d);//(double)getrandom.Next(1,100)/(double)200;
                }
            }
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

        // normalize outputs
        /*
        foreach(Node node in layers[layers.Count-1].nodes)
        {
            node.output = activationFunc.activationFunctionNormalize(node.output);
        }
        */
    }

    public void backPropogate(double[] output, double constant)
    {
        // initialize error layer
        double TotalError = 0;
        int nonBiasNodeIterator = 0;
        for (int i = 0; i < layers[layers.Count - 1].nodes.Count; i++)
        {
            Node node = layers[layers.Count - 1].nodes[i];
            if (node.isBiasNode == false) // this check shouldn't be needed
            {
                node.error = calcActivationFunc_Prime(node.weightedSum) * (output[nonBiasNodeIterator] - node.output - constant);// - constant;

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

    public double getError(double[] output)
    {
        double TotalError = 0d;
        int nonBiasNodeIterator = 0;
        for (int i = 0; i < layers[layers.Count - 1].nodes.Count; i++)
        {
            Node node = layers[layers.Count - 1].nodes[i];
            if (node.isBiasNode == false)
            {
                TotalError += Math.Pow(node.output - output[nonBiasNodeIterator], 2);
                nonBiasNodeIterator += 1;
            }
        }

        return (TotalError);
    }

    public void printNN()
    {
        foreach(Layer l in layers)
        {
            String s = "";
            foreach(Node n in l.nodes)
            {
                s += " " + n.name + " " + n.output.ToString();
                if(l.Equals(layers[layers.Count-2]))
                {
                    foreach(Connector con in n.forwardConnectors)
                    {
                        s += " (x" + con.weight.ToString() + ", ";
                    }
                    s += ")";
                }
            }
            Debug.Log(s);
        }
        Debug.Log("\n");
    }
}

public class NeuralNetworkController : MonoBehaviour
{
    void Start()
    {

    }
}
