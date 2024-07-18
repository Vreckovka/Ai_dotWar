using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MoonLanding_NeuralNetwork
{
  public class Target : AIObject
  {
    public Target(NeuralNetwork neuralNetwork) : base(neuralNetwork)
    {
      point = new Ellipse();
      point.Width = 15;
      point.Height = 15;
      point.Fill = Brushes.Red;
    }


    private int tickCount = 0;
    private bool finished;
    public override void Update(IEnumerable<AIObject> targets, IEnumerable<AIObject> siblings)
    {
      tickCount++;
      float[] inputs = new float[net.layers[0]];

      var actualPoint = new Vector2((float)Canvas.GetLeft(point), (float)Canvas.GetTop(point));

      var targetPosition = GetCloseEntities(actualPoint, targets);
     

      inputs[0] = actualPoint.X > targetPosition.X ? 1 : -1;
      inputs[1] = actualPoint.Y > targetPosition.Y ? 1 : -1;
      inputs[2] = actualPoint.X - targetPosition.X;
      inputs[3] = actualPoint.Y - targetPosition.Y;

      if(siblings != null)
      {
        var siblingsPosition = GetCloseEntities(actualPoint, targets);
        inputs[4] = actualPoint.X - siblingsPosition.X;
        inputs[5] = actualPoint.Y - siblingsPosition.Y;
      }

    

      float[] output = net.FeedForward(inputs);

      vector.X = output[0] + (output[2] * 2);
      vector.Y = output[1] + (output[3] * 2);

      net.AddFitness(tickCount);

    }

   
  }
}

