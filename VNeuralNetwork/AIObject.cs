namespace VNeuralNetwork
{
  public interface INeuralNetwork
  {
    float[] FeedForward(float[] input);
    void AddFitness(float fitness);

    public float Fitness { get; set; }
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
