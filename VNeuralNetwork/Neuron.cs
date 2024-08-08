namespace VNeuralNetwork
{
  using System;
  using System.Collections.Generic;

  public class NeuronConnection
  {
    public int StartNode { get; set; }
    public int EndNode { get; set; }
    public double Weight { get; set; }
  }
  public class Neuron
  {
    public List<float> Weights { get; set; }
    public float Bias { get; set; }

    public Neuron()
    {

    }

    public Neuron(int inputCount)
    {
      var rand = new Random();
      Weights = new List<float>();
      for (int i = 0; i < inputCount; i++)
      {
        Weights.Add((float)rand.NextDouble() * 2 - 1); // Initialize weights between -1 and 1
      }
      Bias = (float)rand.NextDouble() * 2 - 1; // Initialize bias between -1 and 1
    }

    public Neuron(Neuron neuron)
    {
      Weights = new List<float>();

      for (int i = 0; i < neuron.Weights.Count; i++)
      {
        Weights.Add(neuron.Weights[i]);
      }

      Bias = neuron.Bias;
    }

    public double Activate(List<double> inputs)
    {
      double activation = Bias;
      for (int i = 0; i < inputs.Count; i++)
      {
        activation += inputs[i] * Weights[i];
      }

      return Tanh(activation);

      //return Sigmoid(activation);
    }

    private double Tanh(double x)
    {
      return Math.Tanh(x);
    }
  }
}
