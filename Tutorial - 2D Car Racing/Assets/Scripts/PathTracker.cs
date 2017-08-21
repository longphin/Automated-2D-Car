using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTracker {

    private double[] inputs;
    private double[] outputs;

	public PathTracker(int numInputs, int numOutputs)
    {
        inputs = new double[numInputs];
        outputs = new double[numOutputs];
    }
    public PathTracker(double[] inp, double[] outp)
    {
        inputs = new double[inp.Length];
        outputs = new double[outp.Length];
        inputs = inp;
        outputs = outp;
    }

    public void setInputs(double[] i)
    {
        i.CopyTo(inputs, 0);
    }
    public void setOutputs(double[] o)
    {
        o.CopyTo(outputs, 0);
    }

    public double[] getInputs()
    {
        return (inputs);
    }
    public double[] getOutputs()
    {
        return (outputs);
    }
    public double[] getOutputsTrue(double goodPath)
    {
        double[] res = new double[outputs.Length+1];
        outputs.CopyTo(res, 0);
        //res[0] = trueForward;
        res[0] = goodPath;
        return (res);
    }
}
