using VNeuralNetwork;
using System.Text.Json;
using System.IO;

namespace VNeuralNetwork
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class NeuralNetwork : IComparable<NeuralNetwork>
  {
    public List<Layer> Layers { get; private set; }
    public float Fitness { get; set; }

    public NeuralNetwork()
    {
    }

    public NeuralNetwork(int[] layerSizes)
    {
      Layers = new List<Layer>();
      for (int i = 1; i < layerSizes.Length; i++)
      {
        Layers.Add(new Layer(layerSizes[i], layerSizes[i - 1]));
      }
    }

    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
      Layers = new List<Layer>();

      for (int i = 0; i < copyNetwork.Layers.Count; i++)
      {
        Layers.Add(new Layer(copyNetwork.Layers[i].neuronCount, copyNetwork.Layers[i].inputCount));
      }

      CopyWeights(copyNetwork.Layers);
    }

    private void CopyWeights(List<Layer> layers)
    {
      for (int i = 0; i < layers.Count; i++)
      {
        for (int j = 0; j < layers[i].Neurons.Count; j++)
        {
          Layers[i].Neurons[j].Bias = layers[i].Neurons[j].Bias;

          for (int k = 0; k < layers[i].Neurons[j].Weights.Count; k++)
          {
            Layers[i].Neurons[j].Weights[k] = layers[i].Neurons[j].Weights[k];
          }
        }
      }
    }

    public float[] FeedForward(float[] inputs)
    {
      var outputs = inputs;
      foreach (var layer in Layers)
      {
        outputs = layer.Activate(outputs.Select(x => (double)x).ToList()).Select(x => (float)x).ToArray();
      }
      return outputs;
    }

    public void AddFitness(float fit)
    {
      Fitness += fit;
    }

    public int CompareTo(NeuralNetwork other)
    {
      if (other == null) return 1;

      if (Fitness > other.Fitness)
        return 1;
      else if (Fitness < other.Fitness)
        return -1;
      else
        return 0;
    }

    public void SaveNeuralNetwork(string path)
    {
      var json = JsonSerializer.Serialize(this);

      File.WriteAllText(path, json);
    }

    public static NeuralNetwork LoadNeuralNetwork(string path)
    {
      var json = File.ReadAllText(path);
      var net = JsonSerializer.Deserialize<NeuralNetwork>(json);

      return new NeuralNetwork(net);
    }
  }
}
