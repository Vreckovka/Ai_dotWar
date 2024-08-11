
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.Phenomes;
using SharpNeat.Phenomes.NeuralNets;
using SharpNeat.Utility;
using VCore.Standard;
using VCore.Standard.Factories.ViewModels;

namespace VNeuralNetwork
{

  public class NEATManager : ViewModel
  {
    public static float[] ScaledLeakyReLU(float[] value, float c = 1)
    {
      float[] ouput = new float[value.Length];

      for (int i = 0; i < value.Length; i++)
      {
        ouput[i] = ScaledLeakyReLU(value[i],c);
      }

      return ouput;
    }

    public static float[] ScaledLeakyReLUNegative(float[] value, float c = 2)
    {
      float[] ouput = new float[value.Length];

      for (int i = 0; i < value.Length; i++)
      {
        ouput[i] = ScaledLeakyReLUNegative(value[i], c);
      }

      return ouput;
    }

    public static float ScaledLeakyReLU(float value, float c = 1)
    {
      // Scale the output to [0, 1] using a sigmoid-like function
      return (float)(1 / (1 + Math.Exp(-(value + c))));
    }

    public static float ScaledLeakyReLUNegative(float value, float scale = 2)
    {
      // Apply Leaky ReLU function
      double leakyReLUOutput = value;

      // Scale the output to [-1, 1]
      double sigmoidOutput = 1 / (1 + Math.Exp(-leakyReLUOutput));
      return (float)sigmoidOutput * scale - 1; // Transform to [-1, 1]
    }

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
        return value;
      }

      // Map the value from [0, 1] to [-1, 1].
      return 2 * value - 1;
    }


  }

  public class NEATManager<TAIModel> : NEATManager where TAIModel : AIObject
  {
    public NeatAlgorithm NeatAlgorithm { get; set; }

    NeatGenome champ = null;

    private readonly IViewModelsFactory viewModelsFactory;
    private readonly NetworkActivationScheme networkActivationScheme;

    public List<INeuralNetwork> Networks => NeatAlgorithm.GenomeList.Select(x => (INeuralNetwork)x).ToList();

    public List<TAIModel> Agents { get; set; } = new List<TAIModel>();


    public int Generation { get { return (int)(NeatAlgorithm?.CurrentGeneration ?? 0); } }

    public NEATManager(
      IViewModelsFactory viewModelsFactory,
      NetworkActivationScheme networkActivationScheme,
      int inputCount,
      int outputCount,
      IActivationFunction activationFunction
      )
    {
      this.viewModelsFactory = viewModelsFactory;
      this.networkActivationScheme = networkActivationScheme ?? throw new ArgumentNullException(nameof(networkActivationScheme));
      this.inputCount = inputCount;
      this.outputCount = outputCount;
      this.activationFunction = activationFunction;
    }

    #region InitializeManager

    private int inputCount;
    private readonly int outputCount;
    private readonly IActivationFunction activationFunction;

    public void InitializeManager(int agentCount)
    {
      var parameters = new NeatEvolutionAlgorithmParameters();

      parameters.SpecieCount = Math.Max((int)(agentCount * 0.2), 1);

      NeatAlgorithm ea = new NeatAlgorithm(parameters);

      IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = new NeatGenomeDecoder(networkActivationScheme);

      IGenomeListEvaluator<NeatGenome> innerEvaluator = new SelectiveGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, null);

      IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(
                                                                              innerEvaluator,
                                                                              SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());

      var factory = champ != null ? champ.GenomeFactory : GetGenomeFactory();

      ea.Initialize(selectiveEvaluator, factory, GetGenomeList(factory, agentCount, champ));


      NeatAlgorithm = ea;
    }

    #endregion

    #region LoadPredifinedGenome

    public void LoadPredifinedGenome(int[] layers)
    {
      Random random = new Random();

      var geneList = new List<NeuronGene>();
      var connections = new List<ConnectionGene>();


      var innovationIdGenerator = new UInt32IdGenerator();

      geneList.Add(new NeuronGene(innovationIdGenerator.NextId, NodeType.Bias, 0));

      var inputCount = layers[0];
      var outputCount = layers.Last();

      var neatLayers = new List<List<NeuronGene>>();

      var inputLayer = GetLayer(innovationIdGenerator, inputCount, NodeType.Input);
      var outputLayer = GetLayer(innovationIdGenerator, outputCount, NodeType.Output);

      neatLayers.Add(inputLayer);

      foreach (var hiddenCount in layers.Skip(1).SkipLast(1))
      {
        neatLayers.Add(GetLayer(innovationIdGenerator, hiddenCount, NodeType.Hidden));
      }

      neatLayers.Add(outputLayer);

      for (int i = 0; i < neatLayers.Count - 1; i++)
      {
        foreach (var currentNeuron in neatLayers[i])
        {
          foreach (var neuron in neatLayers[i + 1])
          {
            var connection = new ConnectionGene(innovationIdGenerator.NextId, currentNeuron.Id, neuron.Id, random.NextDouble() * 2 - 1);

            connections.Add(connection);
          }
        }
      }

      geneList.AddRange(neatLayers.SelectMany(x => x).OrderBy(x => x.Id));

      var factory = GetGenomeFactory(innovationIdGenerator: innovationIdGenerator);

      champ = new NeatGenome(factory, 0u, 0u, new NeuronGeneList(geneList), new ConnectionGeneList(connections), inputCount, outputCount, true);
    }

    #endregion

    private List<NeuronGene> GetLayer(UInt32IdGenerator generator, int count, NodeType nodeType)
    {
      var newLayer = new List<NeuronGene>();

      for (int i = 0; i < count; i++)
      {
        var gene = new NeuronGene(generator.NextId, nodeType, 0);

        newLayer.Add(gene);
      }

      return newLayer;
    }

    #region CreateAgents

    public void CreateAgents()
    {
      Agents.Clear();

      for (int i = 0; i < NeatAlgorithm.GenomeList.Count; i++)
      {
        AddAgent(NeatAlgorithm.GenomeList[i]);
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
      NeatAlgorithm.UpdateGeneration();

      for (int i = 0; i < NeatAlgorithm.GenomeList.Count; i++)
      {
        Agents[i].NeuralNetwork = NeatAlgorithm.GenomeList[i];
        Agents[i].NeuralNetwork.ResetFitness();
      }


      RaisePropertyChanged(nameof(Generation));
    }

    public void SavePopulation(string path)
    {
      XmlWriterSettings xwSettings = new XmlWriterSettings();
      xwSettings.Indent = true;

      using (XmlWriter xw = XmlWriter.Create(path, xwSettings))
      {
        var best = (NeatGenome)Networks.OrderByDescending(x => x.Fitness).First();
        NeatGenomeXmlIO.WriteComplete(xw, best, false);
      }
    }

    public void LoadPopulation(string path)
    {
      List<NeatGenome> genomeList;

      using (XmlReader xr = XmlReader.Create(path))
      {
        genomeList = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, GetGenomeFactory());
      }

      champ = genomeList[0];
    }

    private NeatGenomeParameters GetParameters()
    {
      var _neatGenomeParams = new NeatGenomeParameters();
      _neatGenomeParams.FeedforwardOnly = networkActivationScheme.AcyclicNetwork;
      _neatGenomeParams.ActivationFn = activationFunction;

      return _neatGenomeParams;
    }

    private NeatGenomeFactory GetGenomeFactory(UInt32IdGenerator genomeIdGenerator = null, UInt32IdGenerator innovationIdGenerator = null)
    {
      if(genomeIdGenerator == null)
      {
        genomeIdGenerator = new UInt32IdGenerator();
      }

      if (innovationIdGenerator == null)
      {
        innovationIdGenerator = new UInt32IdGenerator();
      }

      return new NeatGenomeFactory(inputCount, outputCount, GetParameters(), genomeIdGenerator, innovationIdGenerator);
    }

    private List<NeatGenome> GetGenomeList(NeatGenomeFactory factory, int agentCount, NeatGenome champ = null)
    {
      if (champ != null)
      {
        return factory.CreateGenomeList(agentCount, 0u, champ);
      }
      else
      {
        return factory.CreateGenomeList(agentCount, 0u);
      }
    }
  }
}
