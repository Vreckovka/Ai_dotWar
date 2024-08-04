namespace VNeuralNetwork
{
  using System;
  using System.Linq;
  using TradingBroker.MachineLearning;

  public class NeuralNetworkDNA : DNA<Neuron>
  {
    public NeuralNetworkDNA(
      NeuralNetwork neuralNetwork,
     Func<double> getRandomDouble) : base(
       neuralNetwork.Layers.Sum(x => x.Neurons.Count),
       null,
       null,
       getRandomDouble,
       null,
       false)
    {
      NeuralNetwork = neuralNetwork;
    }

    public NeuralNetwork NeuralNetwork { get; set; }

    #region GetRandomGene

    private Neuron GetRandomGene(Neuron neuron, double mutationRate)
    {
      var newNeuron = new Neuron(neuron);
      var mutationAmount = 0.5;

      if (getRandomDouble() < mutationRate)
      {
        newNeuron.Bias += getRandomDouble() * mutationAmount * 2 - mutationAmount;
      }

      for (int i = 0; i < newNeuron.Weights.Count; i++)
      {
        if (getRandomDouble() < mutationRate)
        {
          double weight = newNeuron.Weights[i];

          float randomNumber = Helper.RandomNumberBetween(0, 100);

          if (randomNumber <= 2)
          {
            weight *= -1f;
          }
          else if (randomNumber <= 4)
          {
            weight = Helper.RandomNumberBetween(-0.5f, 0.5f);
          }
          else if (randomNumber <= 6)
          {
            weight *= Helper.RandomNumberBetween(0f, 1f) + 1f;
          }
          else if (randomNumber <= 8)
          {
            weight *= Helper.RandomNumberBetween(0f, 1f);
          }
          else
            weight += getRandomDouble() * mutationAmount * 2 - mutationAmount;

          newNeuron.Weights[i] = weight;
        }
      }

      return newNeuron;
    }

    #endregion

    #region Mutate

    public override void Mutate(float mutationRate)
    {
      for (int i = 0; i < Genes.Length; i++)
      {
        Genes[i] = GetRandomGene(Genes[i], mutationRate);
      }
    }

    #endregion

    #region GetScore

    protected override float GetScore(Neuron[] genes)
    {
      return NeuralNetwork.Fitness;
    } 

    #endregion

    public void UpdateNeuralNetwork()
    {
      int geneIndex = 0;
      for (int i = 0; i < NeuralNetwork.Layers.Count; i++)
      {
        var layer = NeuralNetwork.Layers[i];

        for (int j = 0; j < layer.Neurons.Count; j++)
        {
          layer.Neurons[j] = Genes[geneIndex];
          geneIndex++;
        }
      }
    }

    public void UpdateDNA()
    {
      int geneIndex = 0;
      for (int i = 0; i < NeuralNetwork.Layers.Count; i++)
      {
        var layer = NeuralNetwork.Layers[i];

        for (int j = 0; j < layer.Neurons.Count; j++)
        {
          Genes[geneIndex] = layer.Neurons[j];
          geneIndex++;
        }
      }
    }
  }
}
