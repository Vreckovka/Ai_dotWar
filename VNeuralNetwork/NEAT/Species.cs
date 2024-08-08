using System;
using System.Collections.Generic;

namespace VNeuralNetwork.NEAT
{
  public class Species
  {
    public List<Genome> Genomes { get; private set; }
    public Genome Representative { get; private set; }
    public double AverageFitness { get; set; }

    public Species(Genome representative)
    {
      Genomes = new List<Genome>();
      Representative = representative;
      Genomes.Add(representative);
    }

    public void AddGenome(Genome genome)
    {
      Genomes.Add(genome);
    }

    public void CalculateAverageFitness()
    {
      double totalFitness = 0.0;
      foreach (var genome in Genomes)
      {
        totalFitness += genome.Fitness;
      }
      AverageFitness = totalFitness / Genomes.Count;
    }

    public void ProduceOffspring(
      int offspringCount, 
      List<Genome> newPopulation, 
      ref int innovationNumber, 
      Random random)
    {
      for (int i = 0; i < offspringCount; i++)
      {
        Genome parent1 = SelectParent(random);
        Genome parent2 = SelectParent(random);

        parent1.Crossover(parent2, out Genome child);

        // Apply mutations
        child.MutateWeight();
        if (random.NextDouble() < 0.03)
          child.MutateAddNode(ref innovationNumber);
        if (random.NextDouble() < 0.05)
          child.MutateAddConnection(ref innovationNumber);

        newPopulation.Add(child);
      }
    }

    private Genome SelectParent(Random random)
    {
      // Roulette wheel selection method to select a parent based on fitness
      double totalFitness = 0.0;
      foreach (var genome in Genomes)
      {
        totalFitness += Math.Max(genome.Fitness, 0.0); // Use max to ensure non-negative contribution
      }

      double randomPoint = random.NextDouble() * totalFitness;
      double currentSum = 0.0;

      foreach (var genome in Genomes)
      {
        currentSum += Math.Max(genome.Fitness, 0.0);
        if (currentSum >= randomPoint)
        {
          return genome;
        }
      }

      return Genomes[random.Next(Genomes.Count)];
    }
  }
}
