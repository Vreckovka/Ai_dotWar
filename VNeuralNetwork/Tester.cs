using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VCore.Standard.Factories.ViewModels;
using VNeuralNetwork.NEAT;

namespace VNeuralNetwork
{
  public class Tester
  {
    private readonly IViewModelsFactory viewModelsFactory;

    public Tester(IViewModelsFactory viewModelsFactory)
    {
      this.viewModelsFactory = viewModelsFactory ?? throw new ArgumentNullException(nameof(viewModelsFactory));
    }

    public void Test()
    {
      //TestBackPropagation();
      //TestGeneticAlgorithm();

      //TestNeatAlgorithm();
      
      //TestNeatSharp();
      //TestNeatManager();
    }

    #region TestBackPropagation

    private void TestBackPropagation()
    {
      NeuralNetwork net = new NeuralNetwork(new int[] { 3, 25, 25, 1 });
      float learningRate = 0.015f;

      for (int i = 0; i < 5000; i++)
      {
        net.BackPropagate(new float[] { 0, 0, 0 }, new float[] { 0 }, learningRate);
        net.BackPropagate(new float[] { 0, 0, 1 }, new float[] { 1 }, learningRate);
        net.BackPropagate(new float[] { 0, 1, 0 }, new float[] { 1 }, learningRate);
        net.BackPropagate(new float[] { 0, 1, 1 }, new float[] { 0 }, learningRate);
        net.BackPropagate(new float[] { 1, 0, 0 }, new float[] { 1 }, learningRate);
        net.BackPropagate(new float[] { 1, 0, 1 }, new float[] { 0 }, learningRate);
        net.BackPropagate(new float[] { 1, 1, 0 }, new float[] { 0 }, learningRate);
        net.BackPropagate(new float[] { 1, 1, 1 }, new float[] { 1 }, learningRate);
      }


      Debug.WriteLine($"[0, 0, 0] -> {net.FeedForward(new float[] { 0, 0, 0 })[0]:0.000} -> 0");
      Debug.WriteLine($"[0, 0, 1] -> {net.FeedForward(new float[] { 0, 0, 1 })[0]:0.000} -> 1");
      Debug.WriteLine($"[0, 1, 0] -> {net.FeedForward(new float[] { 0, 1, 0 })[0]:0.000} -> 1");
      Debug.WriteLine($"[0, 1, 1] -> {net.FeedForward(new float[] { 0, 1, 1 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 0, 0] -> {net.FeedForward(new float[] { 1, 0, 0 })[0]:0.000} -> 1");
      Debug.WriteLine($"[1, 0, 1] -> {net.FeedForward(new float[] { 1, 0, 1 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 1, 0] -> {net.FeedForward(new float[] { 1, 1, 0 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 1, 1] -> {net.FeedForward(new float[] { 1, 1, 1 })[0]:0.000} -> 1");
    }

    #endregion

    #region TestGeneticAlgorithm

    private void TestGeneticAlgorithm()
    {
      AIManager<AIObject> manager = new AIManager<AIObject>(viewModelsFactory, 0.1f);

      manager.InitializeManager(new int[] { 3, 6, 6, 1 }, 100);
      manager.CreateAgents();

      double fitness = -100;
      INeuralNetwork net = null;

      var random = new Random();


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


      while (fitness < -0.6)
      {
        foreach (var agent in manager.Agents)
        {
          for (int i = 0; i < list.Count; i++)
          {
            var values = list[i].Keys.First();
            var result = list[i].Values.First();

            var output = agent.NeuralNetwork.FeedForward(values);

            agent.NeuralNetwork.Fitness += (float)AddFitnessLoss(output, result);
          }
        }

        net = manager.Agents
       .OrderByDescending(x => x.NeuralNetwork.Fitness)
       .First().NeuralNetwork;

        fitness = net.Fitness;

        Debug.WriteLine($"Generation {manager.Generation} Fitness {fitness}");

        manager.UpdateGeneration();
      }



      Debug.WriteLine($"Fitness -> {fitness}");
      Debug.WriteLine($"[0, 0, 0] -> {net.FeedForward(new float[] { 0, 0, 0 })[0]:0.000} -> 0");
      Debug.WriteLine($"[0, 0, 1] -> {net.FeedForward(new float[] { 0, 0, 1 })[0]:0.000} -> 1");
      Debug.WriteLine($"[0, 1, 0] -> {net.FeedForward(new float[] { 0, 1, 0 })[0]:0.000} -> 1");
      Debug.WriteLine($"[0, 1, 1] -> {net.FeedForward(new float[] { 0, 1, 1 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 0, 0] -> {net.FeedForward(new float[] { 1, 0, 0 })[0]:0.000} -> 1");
      Debug.WriteLine($"[1, 0, 1] -> {net.FeedForward(new float[] { 1, 0, 1 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 1, 0] -> {net.FeedForward(new float[] { 1, 1, 0 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 1, 1] -> {net.FeedForward(new float[] { 1, 1, 1 })[0]:0.000} -> 1");
    }

    #endregion

    #region TestNeatAlgorithm

    public void TestNeatAlgorithm()
    {
      var neat = new NEATAlgorithm(50, 3, 1);
      Genome net = null;

      double fitness = -100;
      var random = new Random();

      var list = new List<Dictionary<double[], double>>()
      {
        new Dictionary<double[], double>(){ { new double[] { 0, 0, 0 }, 0 } },
        new Dictionary<double[], double>(){ { new double[] { 0, 0, 1 }, 1 } },
        new Dictionary<double[], double>(){{ new double[] { 0, 1, 0 }, 1 } },
        new Dictionary<double[], double>(){{ new double[] { 0, 1, 1 }, 0  } },
        new Dictionary<double[], double>(){{ new double[] { 1, 0, 0 }, 1  } },
        new Dictionary<double[], double>(){{ new double[] { 1, 0, 1 }, 0  } },
        new Dictionary<double[], double>(){{ new double[] { 1, 1, 0 }, 0  } },
        new Dictionary<double[], double>(){{ new double[] { 1, 1, 1 }, 1  } },
      };

      var testCases = new List<(double[] inputs, double expectedOutput)>
        {
            (new double[] {0, 0}, 0),
            (new double[] {0, 1}, 1),
            (new double[] {1, 0}, 1),
            (new double[] {1, 1}, 0)
        };


      while (neat.Generation < 1000)
      {

        foreach (var agent in neat.Genomes)
        {
          for (int i = 0; i < list.Count; i++)
          {
            var values = list[i].Keys.First();
            var result = list[i].Values.First();


            var output = agent.FeedForward(values);

            agent.Fitness += (float)AddFitnessLoss(output, result);
          }
        }

        net = neat.Genomes
       .OrderByDescending(x => x.Fitness)
       .First();

        fitness = net.Fitness;

        Debug.WriteLine($"Generation {neat.Generation} Fitness {fitness}");

        neat.UpdateGeneration();
      }



      Debug.WriteLine($"Fitness -> {fitness}");
      Debug.WriteLine($"[0, 0, 0] -> {net.FeedForward(new double[] { 0, 0, 0 })[0]:0.000} -> 0");
      Debug.WriteLine($"[0, 0, 1] -> {net.FeedForward(new double[] { 0, 0, 1 })[0]:0.000} -> 1");
      Debug.WriteLine($"[0, 1, 0] -> {net.FeedForward(new double[] { 0, 1, 0 })[0]:0.000} -> 1");
      Debug.WriteLine($"[0, 1, 1] -> {net.FeedForward(new double[] { 0, 1, 1 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 0, 0] -> {net.FeedForward(new double[] { 1, 0, 0 })[0]:0.000} -> 1");
      Debug.WriteLine($"[1, 0, 1] -> {net.FeedForward(new double[] { 1, 0, 1 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 1, 0] -> {net.FeedForward(new double[] { 1, 1, 0 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 1, 1] -> {net.FeedForward(new double[] { 1, 1, 1 })[0]:0.000} -> 1");
    }

    #endregion

    #region TestNeatSharp

    public void TestNeatSharp()
    {
      var ea = new XorExperiment().Init();
      double fitness = -100;
      IBlackBox net = null;
      IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = new NeatGenomeDecoder(NetworkActivationScheme.CreateAcyclicScheme());

      while (true)
      {
        ea.EvaluateGeneration();

        Debug.WriteLine($"Generation {ea.CurrentGeneration} Fitness { ea.CurrentChampGenome.EvaluationInfo.Fitness}");
      }


      Debug.WriteLine($"Fitness -> {fitness}");

      net = genomeDecoder.Decode(ea.CurrentChampGenome);

      Debug.WriteLine($"[0, 0, 0] -> {FeedForward(net, new double[] { 0, 0, 0 })[0]:0.000} -> 0");
      Debug.WriteLine($"[0, 0, 1] -> {FeedForward(net, new double[] { 0, 0, 1 })[0]:0.000} -> 1");
      Debug.WriteLine($"[0, 1, 0] -> {FeedForward(net, new double[] { 0, 1, 0 })[0]:0.000} -> 1");
      Debug.WriteLine($"[0, 1, 1] -> {FeedForward(net, new double[] { 0, 1, 1 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 0, 0] -> {FeedForward(net, new double[] { 1, 0, 0 })[0]:0.000} -> 1");
      Debug.WriteLine($"[1, 0, 1] -> {FeedForward(net, new double[] { 1, 0, 1 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 1, 0] -> {FeedForward(net, new double[] { 1, 1, 0 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 1, 1] -> {FeedForward(net, new double[] { 1, 1, 1 })[0]:0.000} -> 1");
    }

    #endregion

    private void TestNeatManager()
    {
      NEATManager<AIObject> manager = new NEATManager<AIObject>(viewModelsFactory);

      manager.InitializeManager(3,1, 200);
      manager.CreateAgents();

      double fitness = -100;
      INeuralNetwork net = null;

      var random = new Random();


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


      while (fitness < 7.5)
      {
        foreach (var agent in manager.Agents)
        {
          for (int i = 0; i < list.Count; i++)
          {
            var values = list[i].Keys.First();
            var result = list[i].Values.First();

            var output = agent.NeuralNetwork.FeedForward(values);

            var fitnessf = (float)AddFitness(output, result);
          

            agent.NeuralNetwork.Fitness += fitnessf;
          }
        }

        net = manager.Agents
       .OrderByDescending(x => x.NeuralNetwork.Fitness)
       .First().NeuralNetwork;

        fitness = net.Fitness;

        Debug.WriteLine($"Generation {manager.Generation} Fitness {fitness}");

        manager.UpdateGeneration();
      }



      Debug.WriteLine($"Fitness -> {fitness}");
      Debug.WriteLine($"[0, 0, 0] -> {net.FeedForward(new float[] { 0, 0, 0 })[0]:0.000} -> 0");
      Debug.WriteLine($"[0, 0, 1] -> {net.FeedForward(new float[] { 0, 0, 1 })[0]:0.000} -> 1");
      Debug.WriteLine($"[0, 1, 0] -> {net.FeedForward(new float[] { 0, 1, 0 })[0]:0.000} -> 1");
      Debug.WriteLine($"[0, 1, 1] -> {net.FeedForward(new float[] { 0, 1, 1 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 0, 0] -> {net.FeedForward(new float[] { 1, 0, 0 })[0]:0.000} -> 1");
      Debug.WriteLine($"[1, 0, 1] -> {net.FeedForward(new float[] { 1, 0, 1 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 1, 0] -> {net.FeedForward(new float[] { 1, 1, 0 })[0]:0.000} -> 0");
      Debug.WriteLine($"[1, 1, 1] -> {net.FeedForward(new float[] { 1, 1, 1 })[0]:0.000} -> 1");
    }

    private double[] FeedForward(IBlackBox blackBox, double[] inputs)
    {
      for (int i = 0; i < inputs.Length; i++)
      {
        blackBox.InputSignalArray[i] = inputs[i];
      }

      blackBox.Activate();

     var result =  new double[] { blackBox.OutputSignalArray[0] };

      return result;
    }

    #region AddFitnessLoss

    private double AddFitnessLoss(float[] output, float expected)
    {
      return Math.Abs(expected - output[0]) * -1;
    }

    #endregion

    #region AddFitnessLoss

    private double AddFitnessLoss(double[] output, double expected)
    {
      return 1 + Math.Abs(expected - output[0]) * -1;
    }

    #endregion

    #region AddFitness

    private double AddFitness(float[] output, float expected)
    {
      return 1 + Math.Abs(expected - output[0]) * -1;
    }

    #endregion

    private double TestFitness(double[] output, double expectedOutput)
    {
      // Define the XOR problem input-output pairs




      // Get the actual output
      double actualOutput = output[0];

      // Compute the absolute error
      double error = Math.Abs(expectedOutput - actualOutput);



      // Return fitness (inverse of total error)
      // Note: You might want to scale or adjust the fitness score as needed
      return 1.0f / (1.0f + error); // Adding 1 to avoid division by zero
    }
  }
}

