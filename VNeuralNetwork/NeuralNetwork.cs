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
    public List<Layer> Layers { get; set; }
    public float Fitness { get; set; }

    public int[] LayerSizes { get; set; }

    #region Constructors

    public NeuralNetwork()
    {
    }

    public NeuralNetwork(int[] layerSizes)
    {
      LayerSizes = layerSizes;

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
        Layers.Add(new Layer(copyNetwork.Layers[i].NneuronCount, copyNetwork.Layers[i].InputCount));
      }

      CopyWeights(copyNetwork.Layers);
    }

    #endregion

    #region Methods

    #region CopyWeights

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

    #endregion

    #region FeedForward

    public float[] FeedForward(float[] inputs)
    {
      var outputs = inputs;
      foreach (var layer in Layers)
      {
        outputs = layer.Activate(outputs.Select(x => (double)x).ToList()).Select(x => (float)x).ToArray();
      }
      return outputs;
    }

    #endregion

    #region AddFitness

    public void AddFitness(float fit)
    {
      Fitness += fit;
    }

    #endregion

    #region CompareTo

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

    #endregion

    #region SaveNeuralNetwork

    public void SaveNeuralNetwork(string path)
    {
      var json = JsonSerializer.Serialize(this, options: new JsonSerializerOptions() { WriteIndented = true });

      File.WriteAllText(path, json);
    }

    #endregion

    #region LoadNeuralNetwork

    public static NeuralNetwork LoadNeuralNetwork(string path)
    {
      var json = File.ReadAllText(path);
      var net = JsonSerializer.Deserialize<NeuralNetwork>(json);

      return new NeuralNetwork(net);
    }

    #endregion

    #region BackPropagate

    public void BackPropagate(float[] inputs, float[] expected, float learningRate)
    {
      var outputs = new List<float[]> { inputs };

      foreach (var layer in Layers)
      {
        inputs = layer.Activate(inputs.Select(x => (double)x).ToList()).Select(x => (float)x).ToArray();
        outputs.Add(inputs);
      }

      float[] error = expected.Zip(outputs.Last(), (e, o) => e - o).ToArray();
      float[] delta = error.Zip(outputs.Last(), (e, o) => e * TanhDerivative(o)).ToArray();

      for (int i = Layers.Count - 1; i >= 0; i--)
      {
        var layer = Layers[i];
        var previousOutput = outputs[i];

        for (int j = 0; j < layer.Neurons.Count; j++)
        {
          var neuron = layer.Neurons[j];
          neuron.Bias += learningRate * delta[j];
          for (int k = 0; k < neuron.Weights.Count; k++)
          {
            neuron.Weights[k] += learningRate * delta[j] * previousOutput[k];
          }
        }

        if (i > 0)
        {
          float[] nextDelta = new float[Layers[i - 1].Neurons.Count];
          for (int j = 0; j < layer.Neurons.Count; j++)
          {
            for (int k = 0; k < layer.Neurons[j].Weights.Count; k++)
            {
              nextDelta[k] += delta[j] * layer.Neurons[j].Weights[k];
            }
          }
          delta = nextDelta.Zip(outputs[i], (nd, o) => nd * TanhDerivative(o)).ToArray();
        }
      }
    }

    #endregion

    private float Tanh(float x) => (float)Math.Tanh(x);

    private float TanhDerivative(float x) => 1 - x * x;

    #endregion
  }
}
