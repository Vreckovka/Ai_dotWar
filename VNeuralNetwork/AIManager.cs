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
    private List<NeuralNetwork> networks = new List<NeuralNetwork>();
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
        networks.Add(net);
      }
    }

    #region CreateAgents

    public void CreateAgents()
    {
      Agents.Clear();

      for (int i = 0; i < networks.Count; i++)
      {
        AddAgent(networks[i]);
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
      networks = networks.OrderBy(x => x.Fitness).ToList();

      for (int i = 0; i < networks.Count / 2; i++)
      {
        var successIndex = i + (networks.Count / 2);

        var sucessNet = new NeuralNetwork(networks[successIndex]);
        var failedNet = new NeuralNetwork(sucessNet);
        failedNet.Mutate();

        networks[i] = failedNet;
        networks[successIndex] = sucessNet;

        Agents[i].NeuralNetwork = failedNet;
        Agents[successIndex].NeuralNetwork = sucessNet;
      }

      for (int i = 0; i < networks.Count; i++)
      {
        networks[i].Fitness = 0f;
      }
    }

    #endregion
  }
}
