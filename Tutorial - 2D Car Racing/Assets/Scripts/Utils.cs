using System;

public static class Utils
{
    private static System.Random randomGenerator = new System.Random(1991);

    public static double[] ArrayMultiplication(double[] x, double[] y)
    {
        if (x.Length != y.Length) throw new InvalidOperationException("Cannot multiply arrays of different sizes");

        double[] res = new double[x.Length];

        for (int i = 0; i < x.Length; i++)
        {
            res[i] = x[i] * y[i];
        }

        return (res);
    }

    public static int Min(int x, int y)
    {
        if (x <= y) return (x);
        return (y);
    }

    public static double Max(double x1, double x2)
    {
        if (x1 > x2) return (x1);
        return (x2);
    }

    public static int Max(int x1, int x2)
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

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            double element = 0;
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                element += matrix[i, j] * vec[j];
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

    public static double[] AddConstantToVector(double[] x, double c)
    {
        double[] res = new double[x.Length];

        for(int i = 0; i<x.Length; i++)
        {
            res[i] = x[i] + c;
        }

        return (res);
    }
}
