using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VCore.Standard;
using VCore.Standard.Factories.ViewModels;

namespace VNeuralNetwork
{

  public class AIManager<TAIModel> : ViewModel where TAIModel : AIObject
  {
    GeneticAlgoNeuralNetwork genetic;
    public List<NeuralNetwork> Networks => genetic.Population.Select(x => x.NeuralNetwork).ToList();
    public List<TAIModel> Agents { get; set; } = new List<TAIModel>();

    protected readonly IViewModelsFactory viewModelsFactory;
    float mutationRate;

    public AIManager(IViewModelsFactory viewModelsFactory, float mutationRate = 0.01f)
    {
      this.viewModelsFactory = viewModelsFactory ?? throw new ArgumentNullException(nameof(viewModelsFactory));
      this.mutationRate = mutationRate;
    }

    #region Generation

    private int generation;

    public int Generation
    {
      get { return generation; }
      set
      {
        if (value != generation)
        {
          generation = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    #region InitializeManager

    public virtual void InitializeManager(int[] layers, int agentCount)
    {
      genetic = new GeneticAlgoNeuralNetwork(agentCount, layers, mutationRate);

      genetic.SeedGeneration();
      CreateAgents();
    }

    #endregion

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


      Agents.ForEach(x => x.NeuralNetwork.ResetFitness());
      Generation++;
    }

    #endregion

    #region LoadGeneration

    public void LoadGeneration(string path, int agentCount)
    {
      var sucessNet = NeuralNetwork.LoadNeuralNetwork(path);

      genetic = new GeneticAlgoNeuralNetwork(agentCount, sucessNet.LayerSizes, mutationRate);

      genetic.SeedGeneration();
      CreateAgents();

      foreach (var pop in genetic.Population)
      {
        pop.NeuralNetwork = sucessNet;
        pop.UpdateDNA();
      }

      UpdateGeneration();
      Generation = 0;
    }

    #endregion
  }
}
