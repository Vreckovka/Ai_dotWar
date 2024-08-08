namespace VNeuralNetwork.NEAT
{
  public class Gene
  {
    public int InnovationNumber { get; set; }
    public int InNode { get; set; }
    public int OutNode { get; set; }
    public double Weight { get; set; }
    public bool IsEnabled { get; set; }

    public Gene(int inNode, int outNode, double weight, int innovationNumber)
    {
      InNode = inNode;
      OutNode = outNode;
      Weight = weight;
      InnovationNumber = innovationNumber;
      IsEnabled = true;
    }

    public Gene Copy()
    {
      return new Gene(InNode, OutNode, Weight, InnovationNumber) { IsEnabled = IsEnabled };
    }
  }
}
