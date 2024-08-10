
using System;
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
using VCore.Standard;
using VCore.Standard.Factories.ViewModels;
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

  public class NEATManager : ViewModel
  {

    public static float[] MapRangeToNegativeOneToOne(float[] value)
    {
      float[] ouput = new float[value.Length];

      for (int i = 0; i < value.Length; i++)
      {
        ouput[i] = MapRangeToNegativeOneToOne(value[i]);
      }

      return ouput;
    }

    public static float MapRangeToNegativeOneToOne(float value)
    {
      // Ensure that value is within the expected range [0, 1].
      if (value < 0 || value > 1)
      {
        throw new ArgumentOutOfRangeException(nameof(value), "Value should be in the range [0, 1].");
      }

      // Map the value from [0, 1] to [-1, 1].
      return 2 * value - 1;
    }


  }

  public class NEATManager<TAIModel> : NEATManager where TAIModel : AIObject
  {
    NeatAlgorithm neatAlgorithm;

    private readonly IViewModelsFactory viewModelsFactory;
    private readonly NetworkActivationScheme networkActivationScheme;

    public List<INeuralNetwork> Networks => neatAlgorithm.GenomeList.Select(x => (INeuralNetwork)x).ToList();

    public List<TAIModel> Agents { get; set; } = new List<TAIModel>();


    public int Generation { get { return (int)(neatAlgorithm?.CurrentGeneration ?? 0); } }

    public NEATManager(IViewModelsFactory viewModelsFactory, NetworkActivationScheme networkActivationScheme)
    {
      this.viewModelsFactory = viewModelsFactory;
      this.networkActivationScheme = networkActivationScheme ?? throw new ArgumentNullException(nameof(networkActivationScheme));
    }

    #region InitializeManager

    private int inputCount;

    public void InitializeManager(int inputCount, int outputCount, int agentCount)
    {
      this.inputCount = inputCount;
      var _neatGenomeParams = new NeatGenomeParameters();
      _neatGenomeParams.FeedforwardOnly = networkActivationScheme.AcyclicNetwork;
      _neatGenomeParams.ActivationFn = LeakyReLU.__DefaultInstance;

      var parameters = new NeatEvolutionAlgorithmParameters();
      parameters.SpecieCount = (int)(agentCount * 0.2);

      // Create the evolution algorithm.
      NeatAlgorithm ea = new NeatAlgorithm(parameters);


      // Create genome decoder.
      IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = new NeatGenomeDecoder(networkActivationScheme);

      // Create a genome list evaluator. This packages up the genome decoder with the genome evaluator.
      IGenomeListEvaluator<NeatGenome> innerEvaluator = new SelectiveGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, null);

      // Wrap the list evaluator in a 'selective' evaluator that will only evaluate new genomes. That is, we skip re-evaluating any genomes
      // that were in the population in previous generations (elite genomes). This is determined by examining each genome's evaluation info object.
      IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(
                                                                              innerEvaluator,
                                                                              SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());

      // Create a genome factory appropriate for the experiment.
      IGenomeFactory<NeatGenome> genomeFactory = new NeatGenomeFactory(inputCount, outputCount, _neatGenomeParams);

      // Create an initial population of randomly generated genomes.
      List<NeatGenome> genomeList = genomeFactory.CreateGenomeList(agentCount, 0u);


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
        AddAgent(neatAlgorithm.GenomeList[i]);
      }
    }

    #endregion

    #region AddAgent

    public TAIModel AddAgent(INeuralNetwork neuralNetwork)
    {
      var newAgent = viewModelsFactory.Create<TAIModel>(neuralNetwork);
      neuralNetwork.InputCount = this.inputCount;

      Agents.Add(newAgent);

      return newAgent;
    }

    #endregion

    public void UpdateGeneration()
    {
      neatAlgorithm.UpdateGeneration();

      for (int i = 0; i < neatAlgorithm.GenomeList.Count; i++)
      {
        Agents[i].NeuralNetwork = neatAlgorithm.GenomeList[i];
        Agents[i].NeuralNetwork.ResetFitness();
      }


      RaisePropertyChanged(nameof(Generation));
    }
  }
}
