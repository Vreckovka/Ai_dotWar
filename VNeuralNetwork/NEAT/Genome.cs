using System;
using System.Collections.Generic;
using System.Linq;

namespace VNeuralNetwork.NEAT
{
  public class Genome
  {
    public List<Gene> Genes { get; private set; }
    public List<Node> Nodes { get; private set; }
    public double Fitness { get; set; }

    private static Random Random = new Random();

    public Genome()
    {
      Genes = new List<Gene>();
      Nodes = new List<Node>();
    }

    public Genome Copy()
    {
      Genome copy = new Genome();
      foreach (var node in Nodes)
      {
        copy.Nodes.Add(new Node(node.Id, node.Type, node.Bias));
      }
      foreach (var gene in Genes)
      {
        copy.Genes.Add(gene.Copy());
      }
      return copy;
    }

    public void MutateWeight()
    {
      // Simple weight mutation example
      foreach (var gene in Genes)
      {
        if (Random.NextDouble() < 0.8)
        {
          gene.Weight += Random.NextDouble() * 2 - 1; // Random mutation
        }
      }
    }

    public void MutateAddNode(ref int innovationNumber)
    {
      // Choose a random connection to split
      if (Genes.Count == 0) return;

      Gene geneToSplit = Genes[Random.Next(Genes.Count)];

      if (!geneToSplit.IsEnabled) return;

      // Disable the current gene
      geneToSplit.IsEnabled = false;

      var maxId = Nodes.Max(x => x.Id);

      // Create a new node
      Node newNode = new Node(++maxId, NodeType.Hidden, 0);
      Nodes.Add(newNode);

      // Create two new connections
      Gene gene1 = new Gene(geneToSplit.InNode, newNode.Id, 1.0, innovationNumber++);
      Gene gene2 = new Gene(newNode.Id, geneToSplit.OutNode, geneToSplit.Weight, innovationNumber++);

      Genes.Add(gene1);
      Genes.Add(gene2);
    }

    public void MutateAddConnection(ref int innovationNumber)
    {
      // Randomly connect two nodes
      if (Nodes.Count < 2) return;

      Node node1 = Nodes[Random.Next(Nodes.Count)];
      Node node2 = Nodes[Random.Next(Nodes.Count)];

      // Ensure node1 is not the same as node2 and no duplicate connection exists
      if (node1.Id == node2.Id || Genes.Exists(g => g.InNode == node1.Id && g.OutNode == node2.Id))
      {
        return;
      }

      Gene newGene = new Gene(node1.Id, node2.Id, Random.NextDouble() * 2 - 1, innovationNumber++);
      Genes.Add(newGene);
    }

    public void Crossover(Genome parent2, out Genome child)
    {
      child = new Genome();

      Dictionary<int, Gene> parent1Genes = new Dictionary<int, Gene>();
      foreach (var gene in Genes)
      {
        parent1Genes[gene.InnovationNumber] = gene;
      }

      foreach (var gene2 in parent2.Genes)
      {
        if (parent1Genes.TryGetValue(gene2.InnovationNumber, out Gene gene1))
        {
          // Matching genes
          child.Genes.Add(Random.NextDouble() < 0.5 ? gene1.Copy() : gene2.Copy());
        }
        else
        {
          // Disjoint or excess genes from the more fit parent (this assumes parent2 is the more fit)
          child.Genes.Add(gene2.Copy());
        }
      }

      // Add nodes from both parents to the child
      foreach (var node in Nodes)
      {
        child.Nodes.Add(new Node(node.Id, node.Type,node.Bias));
      }

      foreach (var node in parent2.Nodes)
      {
        if (!child.Nodes.Exists(n => n.Id == node.Id))
        {
          child.Nodes.Add(new Node(node.Id, node.Type, node.Bias));
        }
      }
    }

    public double[] FeedForward(double[] inputs)
    {
      // Step 1: Initialize input nodes with input values
      int inputIndex = 0;
      foreach (var node in Nodes.Where(n => n.Type == NodeType.Input))
      {
        node.Value = inputs[inputIndex++];
      }

      // Step 2: Create a dictionary to store node values and initialize it with input node values
      Dictionary<int, double> nodeValues = Nodes.ToDictionary(n => n.Id, n => n.Type == NodeType.Input ? n.Value : 0.0);

      // Step 3: Iteratively update node values to handle cyclic connections
      bool valuesChanged;
      int maxIterations = 1000; // Limit the number of iterations to avoid infinite loops
      int iteration = 0;

      do
      {
        valuesChanged = false;
        iteration++;

        // Update node values based on incoming connections
        foreach (var node in Nodes.Where(n => n.Type != NodeType.Input))
        {
          double sum = 0.0;

          // Sum the weighted inputs from connected nodes
          foreach (var gene in Genes.Where(g => g.OutNode == node.Id && g.IsEnabled))
          {
            if (nodeValues.TryGetValue(gene.InNode, out double inputValue))
            {
              sum += node.Bias + (inputValue * gene.Weight);
            }
          }

          double newValue = Tanh(sum);

          // Update node value if it has changed
          if (Math.Abs(nodeValues[node.Id] - newValue) > 1e-6)
          {
            nodeValues[node.Id] = newValue;
            valuesChanged = true;
          }
        }

        //// Exit if maximum iterations are reached to avoid infinite loops
        //if (iteration >= maxIterations)
        //{
        //  throw new InvalidOperationException("FeedForward method exceeded maximum iterations, possible infinite loop.");
        //}

      } while (valuesChanged && iteration < maxIterations);

      // Step 4: Collect outputs from output nodes
      return Nodes.Where(n => n.Type == NodeType.Output).Select(n => nodeValues[n.Id]).ToArray();
    }

    private List<Node> TopologicalSort()
    {
      var inDegree = new Dictionary<int, int>();
      var adjacencyList = new Dictionary<int, List<int>>();

      // Initialize inDegree and adjacency list for all nodes
      foreach (var node in Nodes)
      {
        inDegree[node.Id] = 0;
        adjacencyList[node.Id] = new List<int>();
      }

      // Build the adjacency list and in-degree count
      foreach (var gene in Genes)
      {
        if (gene.IsEnabled)
        {
          // Ensure that inDegree and adjacencyList entries for both nodes are initialized
          if (!inDegree.ContainsKey(gene.InNode))
          {
            inDegree[gene.InNode] = 0;
            adjacencyList[gene.InNode] = new List<int>();
          }
          if (!inDegree.ContainsKey(gene.OutNode))
          {
            inDegree[gene.OutNode] = 0;
            adjacencyList[gene.OutNode] = new List<int>();
          }

          adjacencyList[gene.InNode].Add(gene.OutNode);
          inDegree[gene.OutNode]++;
        }
      }

      // Perform topological sort using Kahn's algorithm
      var zeroInDegreeNodes = new Queue<Node>(Nodes.Where(n => inDegree[n.Id] == 0));
      var sortedNodes = new List<Node>();

      while (zeroInDegreeNodes.Count > 0)
      {
        var node = zeroInDegreeNodes.Dequeue();
        sortedNodes.Add(node);

        foreach (var neighbor in adjacencyList[node.Id])
        {
          inDegree[neighbor]--;
          if (inDegree[neighbor] == 0)
          {
            zeroInDegreeNodes.Enqueue(Nodes.First(n => n.Id == neighbor));
          }
        }
      }

      if (sortedNodes.Count != Nodes.Count)
      {
        throw new InvalidOperationException("Graph has a cycle and cannot be topologically sorted.");
      }

      return sortedNodes;
    }

    private double Tanh(double x)
    {
      return Math.Tanh(x); // Built-in Tanh function
    }
  }
}

