using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace MoonLanding_NeuralNetwork
{
  public abstract class AIObject
  {
    public Shape point;
    public Vector2 vector = new Vector2(1, 1);
    public Vector2 position = new Vector2();
    public NeuralNetwork net;
    public AIObject(NeuralNetwork neuralNetwork)
    {
      net = neuralNetwork;
    }

    public abstract void Update(IEnumerable<AIObject> targets, IEnumerable<AIObject> siblings);

    public Vector2 GetPosition()
    {
      var targetVec = new Vector2(
        (float)Canvas.GetLeft(point), 
        (float)Canvas.GetTop(point));


     return new Vector2(
          (float)(targetVec.X + (point.Width / 2)),
          (float)(targetVec.Y + (point.Height / 2)));

    }

    protected Vector2 GetCloseEntities(Vector2 actualPoint, IEnumerable<AIObject> targets)
    {
      var orderedTargets = targets
      .Select(x => new { target = x, distance = Vector2.Distance(actualPoint, x.GetPosition()) })
      .OrderBy(x => x.distance);


      var closeTargets = orderedTargets.Where(x => x.distance < 10);
      Vector2 targetPosition = new Vector2(1000, 1000);

      if (closeTargets.Any())
      {
        targetPosition = new Vector2(
         closeTargets.Average(x => x.target.GetPosition().X),
         closeTargets.Average(x => x.target.GetPosition().Y));
      }

      return targetPosition;
    }

  }
}
