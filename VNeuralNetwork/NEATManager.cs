
using System.Collections.Generic;
using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.Phenomes;

namespace VNeuralNetwork
{
  public class NEATManager
  {
    NeatAlgorithm neatAlgorithm;

    public NEATManager(int inputCount, int outputCount)
    {
      neatAlgorithm = Init(inputCount, outputCount);
    }

    #region Init

    public NeatAlgorithm Init(int inputCount, int outputCount)
    {
      var _neatGenomeParams = new NeatGenomeParameters();
      _neatGenomeParams.FeedforwardOnly = true;
      _neatGenomeParams.ActivationFn = LeakyReLU.__DefaultInstance;

      // Create the evolution algorithm.
      NeatAlgorithm ea = new NeatAlgorithm();

      // Create a genome factory appropriate for the experiment.
      IGenomeFactory<NeatGenome> genomeFactory = new NeatGenomeFactory(inputCount, outputCount, _neatGenomeParams);

      // Create an initial population of randomly generated genomes.
      List<NeatGenome> genomeList = genomeFactory.CreateGenomeList(150, 0u);


      // Create IBlackBox evaluator.
      XorExperimentEvaluator evaluator = new XorExperimentEvaluator();

      // Create genome decoder.
      IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = new NeatGenomeDecoder(NetworkActivationScheme.CreateAcyclicScheme());

      // Create a genome list evaluator. This packages up the genome decoder with the genome evaluator.
      IGenomeListEvaluator<NeatGenome> innerEvaluator = new SerialGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator);

      // Wrap the list evaluator in a 'selective' evaluator that will only evaluate new genomes. That is, we skip re-evaluating any genomes
      // that were in the population in previous generations (elite genomes). This is determined by examining each genome's evaluation info object.
      IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(
                                                                              innerEvaluator,
                                                                              SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());
      // Initialize the evolution algorithm.
      ea.Initialize(selectiveEvaluator, genomeFactory, genomeList);


      return ea;
    } 

    #endregion
  }
}
