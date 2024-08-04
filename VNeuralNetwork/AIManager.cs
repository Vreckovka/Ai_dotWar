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

  public class AIManager<TAIModel> where TAIModel : AIObject
  {
    GeneticAlgoNeuralNetwork genetic;
    public List<NeuralNetwork> Networks => genetic.Population.Select(x => x.NeuralNetwork).ToList();
    public List<TAIModel> Agents { get; set; } = new List<TAIModel>();

    protected readonly IViewModelsFactory viewModelsFactory;

    public AIManager(IViewModelsFactory viewModelsFactory)
    {
      this.viewModelsFactory = viewModelsFactory ?? throw new ArgumentNullException(nameof(viewModelsFactory));
    }

    public virtual void Initilize(int[] layers, int agentCount)
    {
      genetic = new GeneticAlgoNeuralNetwork(agentCount, layers);

      genetic.SeedGeneration();
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

    public virtual void UpdateGeneration()
    {
      genetic.CreateNewGeneration();

      for (int i = 0; i < genetic.Population.Count; i++)
      {
        Agents[i].NeuralNetwork = genetic.Population[i].NeuralNetwork;
      }
    }

    #endregion

    public void LoadGeneration(string path)
    {
      var sucessNet = NeuralNetwork.LoadNeuralNetwork(path);

      foreach(var pop in genetic.Population)
      {
        pop.NeuralNetwork = sucessNet;
        pop.UpdateDNA();
      }

      UpdateGeneration();
    }
  }
}
