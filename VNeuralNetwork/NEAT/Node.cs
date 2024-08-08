using System;

namespace VNeuralNetwork.NEAT
{
  public class Node
  {
    public int Id { get; set; }
    public NodeType Type { get; set; }
    public double Value { get; set; }
    public double Bias { get; set; } // Optional, for bias nodes or adding bias to nodes
    public double Activation { get; set; } // Value after applying the activation function

    public Node(int id, NodeType type, double bias)
    {
      Id = id;
      Type = type;
      Value = 0.0;
      Bias = bias;
    }
  }
}
