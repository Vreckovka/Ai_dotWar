using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VNeuralNetwork.NEAT
{
  public class NEATAlgorithm
  {
    public List<Genome> Genomes { get; private set; }
    public List<Species> Species { get; private set; }
    public int Generation { get; private set; }

    private int innovationNumber;

    Random random = new Random();

    public NEATAlgorithm(int initialPopulationSize, int numInputs, int numOutputs)
    {
      Genomes = new List<Genome>();
      Species = new List<Species>();
      innovationNumber = 0;
     

      for (int i = 0; i < initialPopulationSize; i++)
      {
        var nodeId = 0;

        Genome genome = new Genome();

        // Create input and output nodes
        for (int j = 0; j < numInputs; j++)
        {
          genome.Nodes.Add(new Node(nodeId++, NodeType.Input, 0));
        }
        for (int j = 0; j < numOutputs; j++)
        {
          genome.Nodes.Add(new Node(nodeId++, NodeType.Output, 0));
        }

        // Randomly connect inputs to outputs
        for (int j = 0; j < numInputs; j++)
        {
          for (int k = 0; k < numOutputs; k++)
          {
            genome.Genes.Add(new Gene(j, numInputs + k, random.NextDouble() * 2 - 1, innovationNumber++));
          }
        }

        Genomes.Add(genome);
      }

      Speciate();
    }

    public void Speciate()
    {
      var newSpecies = new List<Species>();

      foreach (var genome in Genomes)
      {
        bool foundSpecies = false;
        foreach (var species in Species)
        {
          if (IsSameSpecies(genome, species.Representative))
          {
            species.AddGenome(genome);
            foundSpecies = true;
            break;
          }
        }

        if (!foundSpecies)
        {
          newSpecies.Add(new Species(genome));
        }
      }

      // Add new species if there is room
      foreach (var species in newSpecies)
      {
        if (Species.Count < 10)
        {
          Species.Add(species);
        }
        else
        {
          // If species count reaches the maximum, replace the least fit species
          var leastFitSpecies = Species.OrderBy(s => s.AverageFitness).First();
          Species.Remove(leastFitSpecies);
          Species.Add(species);
        }
      }
    }

    public void UpdateGeneration()
    {
      double minFitness = double.MaxValue;

      // Step 1: Calculate fitness for all species and find the minimum fitness value
      foreach (var species in Species)
      {
        species.CalculateAverageFitness();
        if (species.AverageFitness < minFitness)
        {
          minFitness = species.AverageFitness;
        }
      }

      // Step 2: Normalize fitness values to ensure all are non-negative
      double offset = 0.0;
      if (minFitness <= 0)
      {
        offset = -minFitness + 1.0; // Offset by the negative minFitness plus a small value to avoid zero
      }

      // Step 3: Adjusted fitness and calculate total fitness
      double totalAdjustedFitness = 0.0;
      foreach (var species in Species)
      {
        species.AverageFitness += offset; // Normalize fitness by adding offset
        totalAdjustedFitness += species.AverageFitness;
      }

      // Step 4: Determine the total number of offspring to produce (could be less than the original population)
      int totalOffspringCount = Genomes.Count;
      if (totalAdjustedFitness < Genomes.Count)
      {
        totalOffspringCount = (int)Math.Max(1, totalAdjustedFitness); // Ensure at least 1 offspring is produced
      }

      // Step 5: Allocate offspring based on normalized fitness values
      List<Genome> newPopulation = new List<Genome>();
      int remainingOffspring = totalOffspringCount;

      foreach (var species in Species)
      {
        // Calculate offspring count with higher precision
        double exactOffspringCount = species.AverageFitness * totalOffspringCount / totalAdjustedFitness;

        // Ensure at least 1 offspring per species if fitness is contributing to total fitness
        int offspringCount = (int)Math.Max(1, Math.Floor(exactOffspringCount));

        // Cap offspring to remainingOffspring if it's too high
        offspringCount = Math.Min(offspringCount, remainingOffspring);

        species.ProduceOffspring(offspringCount, newPopulation, ref innovationNumber, random);
        remainingOffspring -= offspringCount;

        // Break if no offspring remaining
        if (remainingOffspring <= 0)
        {
          break;
        }
      }

      // Step 6: Handle any remaining offspring (in case rounding caused a shortfall)
      while (remainingOffspring > 0)
      {
        Species bestSpecies = Species.OrderByDescending(s => s.AverageFitness).First();
        bestSpecies.ProduceOffspring(1, newPopulation, ref innovationNumber,random);
        remainingOffspring--;
      }

      // Step 7: Replace the old population with the new population
      Genomes = newPopulation;
      Speciate();
      Generation++;
    }

    private double TotalAdjustedFitness()
    {
      double totalFitness = 0.0;
      foreach (var species in Species)
      {
        totalFitness += Math.Max(species.AverageFitness, 0.0); // Ensure non-negative contribution
      }
      return totalFitness;
    }

    private bool IsSameSpecies(Genome g1, Genome g2)
    {
      // Coefficients for the distance function
      double c1 = 1.0; // Weight for disjoint genes
      double c2 = 1.0; // Weight for excess genes
      double c3 = 0.4; // Weight for average weight differences

      // Compatibility threshold (can be tuned)
      double compatibilityThreshold = 3.0;

      // Total number of genes in both genomes
      int matchingGenes = 0;
      int disjointGenes = 0;
      int excessGenes = 0;
      double totalWeightDifference = 0.0;

      // Indexes for iterating through genes
      int i1 = 0;
      int i2 = 0;

      while (i1 < g1.Genes.Count && i2 < g2.Genes.Count)
      {
        Gene gene1 = g1.Genes[i1];
        Gene gene2 = g2.Genes[i2];

        if (gene1.InnovationNumber == gene2.InnovationNumber)
        {
          // Matching gene
          matchingGenes++;
          totalWeightDifference += Math.Abs(gene1.Weight - gene2.Weight);
          i1++;
          i2++;
        }
        else if (gene1.InnovationNumber < gene2.InnovationNumber)
        {
          // Gene1 is disjoint
          disjointGenes++;
          i1++;
        }
        else
        {
          // Gene2 is disjoint
          disjointGenes++;
          i2++;
        }
      }

      // Remaining genes are excess genes
      excessGenes += (g1.Genes.Count - i1) + (g2.Genes.Count - i2);

      // Calculate average weight difference
      double averageWeightDifference = matchingGenes == 0 ? 0 : totalWeightDifference / matchingGenes;

      // Normalize by the number of genes in the larger genome (or 1 if too small)
      int maxGenes = Math.Max(g1.Genes.Count, g2.Genes.Count);
      double normalizationFactor = maxGenes < 20 ? 1.0 : maxGenes;

      // Calculate the distance
      double distance = (c1 * disjointGenes / normalizationFactor) +
                        (c2 * excessGenes / normalizationFactor) +
                        (c3 * averageWeightDifference);

      // Determine if they are the same species
      return distance < compatibilityThreshold;
    }
  }
}
