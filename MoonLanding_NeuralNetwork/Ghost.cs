using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MoonLanding_NeuralNetwork
{


  public class Ghost : AIObject
  {
    public AIObject Target;

    public Ghost(NeuralNetwork neuralNetwork) : base(neuralNetwork)
    {
      point = new Ellipse();
      point.Width = 10;
      point.Height = 10;
      point.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#35ac60fc"); ;

      width = point.Width;
      height = point.Height;
    }

    public bool targetWasReached = false;
    int tickCount = 0;

    public override void Update(IEnumerable<AIObject> targets, IEnumerable<AIObject> siblings)
    {
      tickCount++;
      float[] inputs = new float[net.layers[0]];

      var actualPoint = position;
      
      if (tickCount % 100 == 0)
      {
        Target = null;
      }


      if (Target == null)
      {
        var target = targets
          .Select(x => new { target = x, distance = Vector2.Distance(actualPoint, x.GetPosition()) })
          .OrderBy(x => x.distance)
          .FirstOrDefault();

        Target = target?.target;
      }

      var targetPosition = Target.GetPosition();
      

      var actualDistance = Vector2.Distance(GetPosition(), targetPosition);

      inputs[0] = actualPoint.X > targetPosition.X ? -1 : 1;
      inputs[1] = actualPoint.Y > targetPosition.Y ? -1 : 1;
      inputs[2] = actualPoint.X - targetPosition.X;
      inputs[3] = actualPoint.Y - targetPosition.Y;

      if(siblings != null)
      {
        var siblingsPosition = GetCloseEntities(actualPoint, siblings);
        inputs[4] = actualPoint.X - siblingsPosition.X;
        inputs[5] = actualPoint.Y - siblingsPosition.Y;
      }
     

      float[] output = net.FeedForward(inputs);

      vector.X = output[0] + (output[2]);
      vector.Y = output[1] + (output[3]);


      foreach (var target in targets.Where(x => Vector2.Distance(actualPoint, x.GetPosition()) < 5))
      {
          net.AddFitness(10);
          target.net.SetFitness(-10);
      }
    }
  }
}
