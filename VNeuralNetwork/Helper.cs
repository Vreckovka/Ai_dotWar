using System;

namespace VNeuralNetwork
{
  public static class Helper
  {
    public static readonly Random random = new Random();

    public static float RandomNumberBetween(float minValue, float maxValue)
    {
      var next = random.NextDouble();

      return (float)(minValue + (next * (maxValue - minValue)));
    }
  }
}
