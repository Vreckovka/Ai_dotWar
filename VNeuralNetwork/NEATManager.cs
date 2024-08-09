
using System.Collections.Generic;
using System.Linq;
using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.Phenomes;
using SharpNeat.Phenomes.NeuralNets;
using VCore.Standard.Factories.ViewModels;

namespace VNeuralNetwork
{
  public class NeatAlgorithm : NeatEvolutionAlgorithm<NeatGenome>
  {
    public NeatAlgorithm() : base()
    {

    }

    public void UpdateGeneration()
    {
      this.PerformOneGeneration();

      _currentGeneration++;
    }
  }



  public class NEATManager<TAIModel>
  {
    NeatAlgorithm neatAlgorithm;
    IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = new NeatGenomeDecoder(NetworkActivationScheme.CreateAcyclicScheme());
    private readonly IViewModelsFactory viewModelsFactory;

    public List<INeuralNetwork> Networks => neatAlgorithm.GenomeList.Select(x => (INeuralNetwork)x).ToList();

    public List<TAIModel> Agents { get; set; } = new List<TAIModel>();


    public int Generation { get { return (int)neatAlgorithm.CurrentGeneration; } }

    public NEATManager(IViewModelsFactory viewModelsFactory)
    {
      this.viewModelsFactory = viewModelsFactory;
    }

    #region InitializeManager

    public void InitializeManager(int inputCount, int outputCount, int agentCount)
    {
      var _neatGenomeParams = new NeatGenomeParameters();
      _neatGenomeParams.FeedforwardOnly = true;
      _neatGenomeParams.ActivationFn = LeakyReLU.__DefaultInstance;

      // Create the evolution algorithm.
      NeatAlgorithm ea = new NeatAlgorithm();

      // Create a genome factory appropriate for the experiment.
      IGenomeFactory<NeatGenome> genomeFactory = new NeatGenomeFactory(inputCount, outputCount, _neatGenomeParams);

      // Create an initial population of randomly generated genomes.
      List<NeatGenome> genomeList = genomeFactory.CreateGenomeList(agentCount, 0u);

      // Create a genome list evaluator. This packages up the genome decoder with the genome evaluator.
      IGenomeListEvaluator<NeatGenome> innerEvaluator = new Evaluator();

      // Wrap the list evaluator in a 'selective' evaluator that will only evaluate new genomes. That is, we skip re-evaluating any genomes
      // that were in the population in previous generations (elite genomes). This is determined by examining each genome's evaluation info object.
      IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(
                                                                              innerEvaluator,
                                                                              SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());
      // Initialize the evolution algorithm.
      ea.Initialize(selectiveEvaluator, genomeFactory, genomeList);


      neatAlgorithm = ea;
    }

    #endregion

    #region CreateAgents

    public void CreateAgents()
    {
      Agents.Clear();

      for (int i = 0; i < neatAlgorithm.GenomeList.Count; i++)
      {
        AddAgent((NeatGenome)neatAlgorithm.GenomeList[i]);
      }
    }

    #endregion

    #region AddAgent

    public TAIModel AddAgent(NeatGenome neuralNetwork)
    {
      var newAgent = viewModelsFactory.Create<TAIModel>(neuralNetwork);

      Agents.Add(newAgent);

      return newAgent;
    }

    #endregion

    public void UpdateGeneration()
    {
      neatAlgorithm.UpdateGeneration();
    }
  }

  public class Evaluator : IGenomeListEvaluator<NeatGenome>
  {
    IGenomeDecoder<NeatGenome, IBlackBox> _genomeDecoder = new NeatGenomeDecoder(NetworkActivationScheme.CreateAcyclicScheme());

    public ulong EvaluationCount => throw new System.NotImplementedException();

    public bool StopConditionSatisfied => throw new System.NotImplementedException();

    public void Evaluate(IList<NeatGenome> genomeList)
    {
      foreach (NeatGenome genome in genomeList)
      {
        FastAcyclicNetwork phenome = (FastAcyclicNetwork)_genomeDecoder.Decode(genome);

        if (null == phenome)
        {   // Non-viable genome.
          genome.EvaluationInfo.SetFitness(0.0);
          genome.EvaluationInfo.AuxFitnessArr = null;
        }
        else
        {
          //FitnessInfo fitnessInfo = phenome.;
         // genome.EvaluationInfo.SetFitness(fitnessInfo._fitness);
         // genome.EvaluationInfo.AuxFitnessArr = fitnessInfo._auxFitnessArr;
        }
      }
    }

    public void Reset()
    {
      throw new System.NotImplementedException();
    }
  }
}
