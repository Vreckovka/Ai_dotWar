using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace MoonLanding_NeuralNetwork
{
  public abstract class AIObject
  {
    public FrameworkElement point;
    public Vector2 vector = new Vector2(1, 1);

    #region position

    private Vector2 positionx;

    public Vector2 position
    {
      get { return positionx; }
      set
      {
        if (value != positionx)
        {
          positionx = value;
        }
      }
    }

    #endregion

    public NeuralNetwork net;

    public double width;
    public double height;

    public AIObject(NeuralNetwork neuralNetwork)
    {
      net = neuralNetwork;
    }

    public abstract void Update(IEnumerable<AIObject> targets, IEnumerable<AIObject> siblings);

    public Vector2 GetPosition()
    {
     return new Vector2(
          (float)(position.X + (width / 2)),
          (float)(position.Y + (height / 2)));

    }

    protected Vector2? GetCloseEntities(double distance ,Vector2 actualPoint, IEnumerable<AIObject> targets, out int number)
    {
      var orderedTargets = targets
      .Select(x => new { target = x, distance = Vector2.Distance(actualPoint, x.GetPosition()) })
      .OrderBy(x => x.distance);


      var closeTargets = orderedTargets.Where(x => x.distance < distance);

      Vector2? targetPosition = null;

      if (closeTargets.Any())
      {
        targetPosition = new Vector2(
         closeTargets.Average(x => x.target.GetPosition().X),
         closeTargets.Average(x => x.target.GetPosition().Y));
      }

      number = closeTargets.Count();

      return targetPosition;
    }

  }
}
