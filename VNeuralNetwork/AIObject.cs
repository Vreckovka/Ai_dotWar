using SharpNeat.Genomes.Neat;

namespace VNeuralNetwork
{
  public interface INeuralNetwork
  {
    float[] FeedForward(float[] input);
    void AddFitness(float fitness);
    void ResetFitness();

    public float Fitness { get; }

    public void SaveNeuralNetwork(string path);

    public int InputCount { get; set; }
  }

  public class AIObject
  {
    public INeuralNetwork NeuralNetwork { get; set; }

    public AIObject(INeuralNetwork neuralNetwork)
    {
      NeuralNetwork = neuralNetwork;
    }
  }
}
