using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.Phenomes;
using SharpNeat.Phenomes.NeuralNets;
using SharpNeat.SpeciationStrategies;

namespace VNeuralNetwork
{

  public class XorExperiment
  {
    public NeatAlgorithm Init()
    {
      int inputCount = 3; // Number of input nodes
      int outputCount = 1; // Number of output nodes

      var _neatGenomeParams = new NeatGenomeParameters();

      var scheme = NetworkActivationScheme.CreateAcyclicScheme();

      _neatGenomeParams.FeedforwardOnly = scheme.AcyclicNetwork;
      _neatGenomeParams.ActivationFn = TanH.__DefaultInstance;
      _neatGenomeParams.ConnectionWeightRange = 1;

      // Create the evolution algorithm.
      NeatAlgorithm ea = new NeatAlgorithm();

      // Create a genome factory appropriate for the experiment.
      IGenomeFactory<NeatGenome> genomeFactory = new NeatGenomeFactory(inputCount, outputCount, _neatGenomeParams);

      // Create an initial population of randomly generated genomes.
      List<NeatGenome> genomeList = genomeFactory.CreateGenomeList(150, 0u);


      // Create IBlackBox evaluator.
      XorExperimentEvaluator evaluator = new XorExperimentEvaluator();

      // Create genome decoder.
      IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = new NeatGenomeDecoder(scheme);

      // Create a genome list evaluator. This packages up the genome decoder with the genome evaluator.
      IGenomeListEvaluator<NeatGenome> innerEvaluator = new SelectiveGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator);

      // Wrap the list evaluator in a 'selective' evaluator that will only evaluate new genomes. That is, we skip re-evaluating any genomes
      // that were in the population in previous generations (elite genomes). This is determined by examining each genome's evaluation info object.
      IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(
                                                                              innerEvaluator,
                                                                              SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());
      // Initialize the evolution algorithm.
      ea.Initialize(selectiveEvaluator, genomeFactory, genomeList);


      return ea;
    }
  }

  public class VoidEvaluator : IPhenomeEvaluator<IBlackBox>
  {
    public ulong EvaluationCount => 0;

    public bool StopConditionSatisfied => false;

    public FitnessInfo Evaluate(IBlackBox phenome)
    {
      throw new NotImplementedException();
    }

    public void Reset()
    {
      throw new NotImplementedException();
    }
  }

  public class XorExperimentEvaluator : IPhenomeEvaluator<IBlackBox>
  {
    public XorExperimentEvaluator()
    {
    }

    // Gets the total number of evaluations that have been performed.
    public ulong EvaluationCount { get; private set; }

    // Gets a value indicating whether some goal fitness has been achieved and that
    // the evolutionary algorithm/search should stop. This property's value can remain false
    // to allow the algorithm to run indefinitely.
    public bool StopConditionSatisfied { get; private set; }

    public FitnessInfo Evaluate(IBlackBox phenome)
    {
      var list = new List<Dictionary<float[], float>>()
      {
        new Dictionary<float[], float>(){ { new float[] { 0, 0, 0 }, 0 } },
        new Dictionary<float[], float>(){ { new float[] { 0, 0, 1 }, 1 } },
        new Dictionary<float[], float>(){{ new float[] { 0, 1, 0 }, 1 } },
        new Dictionary<float[], float>(){{ new float[] { 0, 1, 1 }, 0  } },
        new Dictionary<float[], float>(){{ new float[] { 1, 0, 0 }, 1  } },
        new Dictionary<float[], float>(){{ new float[] { 1, 0, 1 }, 0  } },
        new Dictionary<float[], float>(){{ new float[] { 1, 1, 0 }, 0  } },
        new Dictionary<float[], float>(){{ new float[] { 1, 1, 1 }, 1  } },
      };

      float fitness = 0;
      for (int i = 0; i < list.Count; i++)
      {
        phenome.InputSignalArray.Reset();

        var values = list[i].Keys.First();
        var result = list[i].Values.First();

        phenome.InputSignalArray[0] = values[0];
        phenome.InputSignalArray[1] = values[1];
        phenome.InputSignalArray[2] = values[2];

        phenome.Activate();

        var loss = Math.Abs(result - (float)phenome.OutputSignalArray[0]) * -1;

        fitness += 1 + loss;
      }

      return new FitnessInfo(fitness, fitness);
    }

   
    public void Reset()
    {
      //throw new NotImplementedException();
    }
  }
}
