
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
using VCore.Standard.Helpers;

namespace VNeuralNetwork
{
  public class NeatAlgorithm : NeatEvolutionAlgorithm<NeatGenome>
  {
    public NeatAlgorithm() : base()
    {

    }

    public void UpdateGeneration()
    {
      this.CreateNewGeneration();
      GenomeList.OfType<NeatGenome>().ForEach(x => x.Fitness = 0);

      _currentGeneration++;
    }

    public void EvaluateGeneration()
    {
      this.PerformOneGeneration();

      _currentGeneration++;
    }
  }



  public class NEATManager<TAIModel> where TAIModel : AIObject
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

    private int inputCount;

    public void InitializeManager(int inputCount, int outputCount, int agentCount)
    {
      this.inputCount = inputCount;
      var _neatGenomeParams = new NeatGenomeParameters();
      _neatGenomeParams.FeedforwardOnly = true;
      _neatGenomeParams.ActivationFn = TanH.__DefaultInstance;

      // Create the evolution algorithm.
      NeatAlgorithm ea = new NeatAlgorithm();


      // Create a genome factory appropriate for the experiment.
      IGenomeFactory<NeatGenome> genomeFactory = new NeatGenomeFactory(inputCount, outputCount, _neatGenomeParams);

      // Create an initial population of randomly generated genomes.
      List<NeatGenome> genomeList = genomeFactory.CreateGenomeList(agentCount, 0u);


      // Initialize the evolution algorithm.
      ea.Initialize(null, genomeFactory, genomeList);


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
      for (int i = 0; i < neatAlgorithm.GenomeList.Count; i++)
      {
        Agents[i].NeuralNetwork = neatAlgorithm.GenomeList[i];
      }

      neatAlgorithm.UpdateGeneration();
    }
  }
}
