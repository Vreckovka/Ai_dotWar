namespace VNeuralNetwork
{
  using System.Collections.Generic;

  public class Layer
  {
    public List<Neuron> Neurons { get; set; }

    public int NneuronCount { get; set; }
    public int InputCount { get; set; }

    public Layer()
    {

    }

    public Layer(int neuronCount, int inputCount)
    {
      this.NneuronCount = neuronCount;
      this.InputCount = inputCount;

      Neurons = new List<Neuron>();
      for (int i = 0; i < neuronCount; i++)
      {
        Neurons.Add(new Neuron(inputCount));
      }
    }

    public List<double> Activate(List<double> inputs)
    {
      var outputs = new List<double>();
      foreach (var neuron in Neurons)
      {
        outputs.Add(neuron.Activate(inputs));
      }
      return outputs;
    }
  }
}
