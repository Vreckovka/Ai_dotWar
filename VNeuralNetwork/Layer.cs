namespace VNeuralNetwork
{
  using System.Collections.Generic;

  public class Layer
  {
    public List<Neuron> Neurons { get; private set; }

    public int neuronCount;
    public int inputCount;

    public Layer(int neuronCount, int inputCount)
    {
      this.neuronCount = neuronCount;
      this.inputCount = inputCount;

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
