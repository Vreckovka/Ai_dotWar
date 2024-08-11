using System.Linq;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using VCore.Standard.Helpers;

namespace VNeuralNetwork
{
  public class NeatAlgorithm : NeatEvolutionAlgorithm<NeatGenome>
  {
    public NeatAlgorithm() : base()
    {

    }

    public NeatAlgorithm(NeatEvolutionAlgorithmParameters eaParams) : base()
    {
      _eaParams = eaParams;
    }

    public void UpdateGeneration()
    {
      this.CreateNewGeneration();
      GenomeList.OfType<NeatGenome>().ForEach(x => x.ResetFitness());

      _currentGeneration++;
    }

    public void EvaluateGeneration()
    {
      this.PerformOneGeneration();

      _currentGeneration++;
    }
  }
}
