namespace VNeuralNetwork
{
  using System;
  using System.Collections.Generic;

  public class Neuron
  {
    public List<double> Weights { get; private set; }
    public double Bias { get; set; }

    public Neuron(int inputCount)
    {
      var rand = new Random();
      Weights = new List<double>();
      for (int i = 0; i < inputCount; i++)
      {
        Weights.Add(rand.NextDouble() * 2 - 1); // Initialize weights between -1 and 1
      }
      Bias = rand.NextDouble() * 2 - 1; // Initialize bias between -1 and 1
    }

    public Neuron(Neuron neuron)
    {
      Weights = new List<double>();

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

      return Math.Tanh(activation);

      //return Sigmoid(activation);
    }

    private double Sigmoid(double x)
    {
      return 1.0 / (1.0 + Math.Exp(-x));
    }
  }
}
