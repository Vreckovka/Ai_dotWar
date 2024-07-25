using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VCore.Standard.Factories.ViewModels;

namespace VNeuralNetwork
{
  public class AIObject
  {
    public NeuralNetwork NeuralNetwork { get; set; }

    public AIObject(NeuralNetwork neuralNetwork)
    {
      NeuralNetwork = neuralNetwork;
    }

  }

  public class AIManager<TAIModel> where TAIModel: AIObject
  {
    public List<NeuralNetwork> Networks { get; private set; } = new List<NeuralNetwork>();
    public List<TAIModel> Agents { get; set; } = new List<TAIModel>();

    private readonly IViewModelsFactory viewModelsFactory;

    public AIManager(IViewModelsFactory viewModelsFactory)
    {
      this.viewModelsFactory = viewModelsFactory ?? throw new ArgumentNullException(nameof(viewModelsFactory));
    }

    public void Initilize(int[] layers, int agentCount)
    {
      for (int i = 0; i < agentCount; i++)
      {
        NeuralNetwork net = new NeuralNetwork(layers);
        net.Mutate();
        Networks.Add(net);
      }
    }

    #region CreateAgents

    public void CreateAgents()
    {
      Agents.Clear();

      for (int i = 0; i < Networks.Count; i++)
      {
        AddAgent(Networks[i]);
      }
    }

    #endregion

    #region AddAgent

    public TAIModel AddAgent(NeuralNetwork neuralNetwork)
    {
      var newAgent = viewModelsFactory.Create<TAIModel>(neuralNetwork);

      Agents.Add(newAgent);

      return newAgent;
    }

    #endregion

    #region UpdateGeneration

    public void UpdateGeneration()
    {
      Networks = Networks.OrderBy(x => x.Fitness).ToList();

      for (int i = 0; i < Networks.Count / 2; i++)
      {
        var successIndex = i + (Networks.Count / 2);

        var sucessNet = new NeuralNetwork(Networks[successIndex]);
        var failedNet = new NeuralNetwork(sucessNet);
        failedNet.Mutate();

        Networks[i] = failedNet;
        Networks[successIndex] = sucessNet;

        Agents[i].NeuralNetwork = failedNet;
        Agents[successIndex].NeuralNetwork = sucessNet;
      }

      for (int i = 0; i < Networks.Count; i++)
      {
        Networks[i].Fitness = 0f;
      }
    }

    #endregion

    public void LoadGeneration(string path)
    {
      var sucessNet = NeuralNetwork.LoadNeuralNetwork(path);

      for (int i = 0; i < Networks.Count / 2; i++)
      {
        var successIndex = i + (Networks.Count / 2);
        var failedNet = new NeuralNetwork(sucessNet);
        failedNet.Mutate();

        Networks[i] = failedNet;
        Networks[successIndex] = sucessNet;

        Agents[i].NeuralNetwork = failedNet;
        Agents[successIndex].NeuralNetwork = sucessNet;
      }

      for (int i = 0; i < Networks.Count; i++)
      {
        Networks[i].Fitness = 0f;
      }
    }
  }
}
