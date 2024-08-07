using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VCore.Standard.Factories.ViewModels;

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
      TestGeneticAlgorithm();
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

      float fitness = -100;
      NeuralNetwork net = null;

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
        if (manager.Generation % 100 == 0)
        {
          if (manager.Generation > 0)
            manager.UpdateGeneration();

          foreach (var agent in manager.Agents)
          {
            for (int i = 0; i < list.Count; i++)
            {
              var randomIndex = random.Next(0, list.Count);

              var values = list[randomIndex].Keys.First();
              var result = list[randomIndex].Values.First();

              AddFitnessLoss(agent, values, result);
            }
          }

        }

        manager.UpdateGeneration();

        foreach (var agent in manager.Agents)
        {
          for (int i = 0; i < list.Count; i++)
          {
            var values = list[i].Keys.First();
            var result = list[i].Values.First();

            AddFitnessLoss(agent, values, result);
          }
        }

        net = manager.Agents
        .OrderByDescending(x => x.NeuralNetwork.Fitness)
        .First().NeuralNetwork;

        fitness = net.Fitness;

        Debug.WriteLine($"Generation {manager.Generation} Fitness {fitness}");
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

    #region AddFitnessLoss

    private void AddFitnessLoss(AIObject agent, float[] inputs, float expected)
    {
      var output = agent.NeuralNetwork.FeedForward(inputs);
      var loss = Math.Abs(expected - output[0]) * -1;

      agent.NeuralNetwork.AddFitness(1 + loss);
    }

    #endregion
  }
}
