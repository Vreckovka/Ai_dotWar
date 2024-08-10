using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TradingBroker.MachineLearning;
using VNeuralNetwork;

namespace MoonLanding_NeuralNetwork
{

  
  public class Ghost : AIWpfObject
  {
    public AIWpfObject Target;
    private Random random = new Random();

    public Ghost(INeuralNetwork neuralNetwork) : base(neuralNetwork)
    {
      var ellipse = new Ellipse();
      ellipse.Width = 8;
      ellipse.Height = 8;

      point = ellipse;

      width = point.Width;
      height = point.Height;
    }

    public bool targetWasReached = false;

    public override void Update(IEnumerable<AIWpfObject> targets, IEnumerable<AIWpfObject> siblings)
    {
      float[] inputs = new float[NeuralNetwork.InputCount];

      var actualPoint = position;
      var castedTarget = (Target)Target;


      if (random.Next(0, 100) < 5)
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

      var targetPosition = Target?.GetPosition();


      if (targetPosition != null)
      {
        var actualDistance = Vector2.Distance(GetPosition(), targetPosition.Value);

        inputs[0] = actualPoint.X > targetPosition.Value.X ? -1 : 1;
        inputs[1] = actualPoint.Y > targetPosition.Value.Y ? -1 : 1;

        inputs[2] = actualPoint.X - targetPosition.Value.X;
        inputs[3] = actualPoint.Y - targetPosition.Value.Y;
      }


      float[] output = NeuralNetwork.FeedForward(inputs);

      output = NEATManager.MapRangeToNegativeOneToOne(output);

      vector.X = output[0] + (output[2] * 0.5f);
      vector.Y = output[1] + (output[3] * 0.5f);



      if (Target != null)
      {
        foreach (var target in targets
        .Where(x => Math.Abs(Vector2.Distance(actualPoint, x.GetPosition())) <= Target.width))
        {
          NeuralNetwork.AddFitness(100);

          if (target is Target targetTarget)
          {
            targetTarget.Health -= 2.5;
          }
        }
      }
    }
  }
}
