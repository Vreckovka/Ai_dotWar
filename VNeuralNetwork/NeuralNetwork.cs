
using System.Collections.Generic;
using System;
using VNeuralNetwork;

namespace VNeuralNetwork
{
  public class NeuralNetwork : IComparable<NeuralNetwork>
  {
    public int[] Layers { get; set; }
    public float[][] Neurons { get; set; }
    public float[][][] Weights { get; set; } 
    
    public float Fitness { get; set; }

    /// <summary>
    /// Initilizes and neural network with random weights
    /// </summary>
    /// <param name="layers">layers to the neural network</param>
    public NeuralNetwork(int[] layers)
    {
      //deep copy of layers of this network 
      this.Layers = new int[layers.Length];
      for (int i = 0; i < layers.Length; i++)
      {
        this.Layers[i] = layers[i];
      }


      //generate matrix
      InitNeurons();
      InitWeights();
    }

    /// <summary>
    /// Deep copy constructor 
    /// </summary>
    /// <param name="copyNetwork">Network to deep copy</param>
    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
      this.Layers = new int[copyNetwork.Layers.Length];
      for (int i = 0; i < copyNetwork.Layers.Length; i++)
      {
        this.Layers[i] = copyNetwork.Layers[i];
      }

      InitNeurons();
      InitWeights();
      CopyWeights(copyNetwork.Weights);
    }

    private void CopyWeights(float[][][] copyWeights)
    {
      for (int i = 0; i < Weights.Length; i++)
      {
        for (int j = 0; j < Weights[i].Length; j++)
        {
          for (int k = 0; k < Weights[i][j].Length; k++)
          {
            Weights[i][j][k] = copyWeights[i][j][k];
          }
        }
      }
    }

    /// <summary>
    /// Create neuron matrix
    /// </summary>
    private void InitNeurons()
    {
      //Neuron Initilization
      List<float[]> neuronsList = new List<float[]>();

      for (int i = 0; i < Layers.Length; i++) //run through all layers
      {
        neuronsList.Add(new float[Layers[i]]); //add layer to neuron list
      }

      Neurons = neuronsList.ToArray(); //convert list to array
    }

    /// <summary>
    /// Create weights matrix.
    /// </summary>
    private void InitWeights()
    {

      List<float[][]> weightsList = new List<float[][]>(); //weights list which will later will converted into a weights 3D array

      //itterate over all neurons that have a weight connection
      for (int i = 1; i < Layers.Length; i++)
      {
        List<float[]> layerWeightsList = new List<float[]>(); //layer weight list for this current layer (will be converted to 2D array)

        int neuronsInPreviousLayer = Layers[i - 1];

        //itterate over all neurons in this current layer
        for (int j = 0; j < Neurons[i].Length; j++)
        {
          float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

          //itterate over all neurons in the previous layer and set the weights randomly between 0.5f and -0.5
          for (int k = 0; k < neuronsInPreviousLayer; k++)
          {
            //give random weights to neuron weights
            neuronWeights[k] = Helper.RandomNumberBetween(-0.5f, 0.5f);
          }

          layerWeightsList.Add(neuronWeights); //add neuron weights of this current layer to layer weights
        }

        weightsList.Add(layerWeightsList.ToArray()); //add this layers weights converted into 2D array into weights list
      }

      Weights = weightsList.ToArray(); //convert to 3D array
    }

    /// <summary>
    /// Feed forward this neural network with a given input array
    /// </summary>
    /// <param name="inputs">Inputs to network</param>
    /// <returns></returns>
    public float[] FeedForward(float[] inputs)
    {
      //Add inputs to the neuron matrix
      for (int i = 0; i < inputs.Length; i++)
      {
        Neurons[0][i] = inputs[i];
      }

      //itterate over all neurons and compute feedforward values 
      for (int i = 1; i < Layers.Length; i++)
      {
        for (int j = 0; j < Neurons[i].Length; j++)
        {
          float value = 0f;

          for (int k = 0; k < Neurons[i - 1].Length; k++)
          {
            value += Weights[i - 1][j][k] * Neurons[i - 1][k]; //sum off all weights connections of this neuron weight their values in previous layer
          }

          Neurons[i][j] = (float)Math.Tanh(value); //Hyperbolic tangent activation
        }
      }

      return Neurons[Neurons.Length - 1]; //return output layer
    }

    /// <summary>
    /// Mutate neural network weights
    /// </summary>
    public void Mutate()
    {
      for (int i = 0; i < Weights.Length; i++)
      {
        for (int j = 0; j < Weights[i].Length; j++)
        {
          for (int k = 0; k < Weights[i][j].Length; k++)
          {
            float weight = Weights[i][j][k];

            //mutate weight value 
            float randomNumber = Helper.RandomNumberBetween(0f, 200f);

            if (randomNumber <= 2f)
            { //if 1
              //flip sign of weight
              weight *= -1f;
            }
            else if (randomNumber <= 4f)
            { //if 2
              //pick random weight between -1 and 1
              weight = Helper.RandomNumberBetween(-1f, 1f);
            }
            else if (randomNumber <= 6f)
            { //if 3
              //randomly increase by 0% to 100%
              float factor = Helper.RandomNumberBetween(0f, 1f) + 1f;
              weight *= factor;
            }
            else if (randomNumber <= 8f)
            { //if 4
              //randomly decrease by 0% to 100%
              float factor = Helper.RandomNumberBetween(0f, 1f);
              weight *= factor;
            }

            Weights[i][j][k] = weight;
          }
        }
      }
    }

    public void AddFitness(float fit)
    {
      Fitness += fit;
    }

    /// <summary>
    /// Compare two neural networks and sort based on fitness
    /// </summary>
    /// <param name="other">Network to be compared to</param>
    /// <returns></returns>
    public int CompareTo(NeuralNetwork other)
    {
      if (other == null) return 1;

      if (Fitness > other.Fitness)
        return 1;
      else if (Fitness < other.Fitness)
        return -1;
      else
        return 0;
    }
  }
}
