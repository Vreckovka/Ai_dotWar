namespace VNeuralNetwork
{
  using System.Linq;
  using TradingBroker.MachineLearning;

  public class GeneticAlgoNeuralNetwork : BaseGeneticAlgorithm<Neuron, NeuralNetworkDNA>
  {
    private readonly int[] layers;

    #region Contructors

    public GeneticAlgoNeuralNetwork(
      int populationSize,
      int[] layers,
      float mutationRate = 0.01f) : base(
        populationSize,
        layers.Skip(1).Sum(x => x),
        null,
        null,
        null,
        mutationRate: mutationRate)
    {
      this.layers = layers;
    }

    #endregion

    public override void SeedGeneration()
    {
      base.SeedGeneration();

      Population.ForEach(x => x.UpdateDNA());
    }

    public override void CreateNewGeneration()
    {
      base.CreateNewGeneration();

      foreach (var gene in Population)
      {
        gene.UpdateNeuralNetwork();
      }
    }

    protected override NeuralNetworkDNA GetNewDNA(bool init)
    {
      var neuralNetwork = new NeuralNetwork(layers);

      var dna = new NeuralNetworkDNA(neuralNetwork, GetRandomDouble);

      return dna;
    }
  }
}
