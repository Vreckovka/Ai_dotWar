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

    Random random = new Random();
    private Neuron GetRandomGene(Neuron neuron, float mutationRate)
    {
      var newNeuron = new Neuron(neuron);
      var mutationAmount = 0.5f;

      if (getRandomDouble() < mutationRate)
      {
        newNeuron.Bias += (float)getRandomDouble() * mutationAmount * 2 - mutationAmount;
      }

      for (int i = 0; i < newNeuron.Weights.Count; i++)
      {
        if (getRandomDouble() < mutationRate)
        {
          double weight = newNeuron.Weights[i];

          var randomNumber = random.Next(0,7);

          if (randomNumber <= 1)
            weight *= -1f;
          else if (randomNumber <= 2)
            weight = Helper.RandomNumberBetween(-1f, 1f);
          else if (randomNumber <= 3)
            weight *= Helper.RandomNumberBetween(0f, 1f) + 1f;
          else if (randomNumber <= 4)
            weight *= Helper.RandomNumberBetween(0f, 1f);
          else if (randomNumber <= 5)
            weight += (getRandomDouble() * 2 - 1) * mutationAmount;
          else if (randomNumber <= 6)
            weight = GaussianRandom(0, mutationAmount);

          newNeuron.Weights[i] = (float)weight;
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

    private double GaussianRandom(double mean, double stdDev)
    {
      double u1 = getRandomDouble();
      double u2 = getRandomDouble();
      double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
      return mean + stdDev * randStdNormal;
    }

    #region UpdateNeuralNetwork

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

    #endregion

    #region UpdateDNA

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

    #endregion

  }
}
