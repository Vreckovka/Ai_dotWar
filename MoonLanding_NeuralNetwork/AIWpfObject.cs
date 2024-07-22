using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using VNeuralNetwork;

namespace MoonLanding_NeuralNetwork
{
  public abstract class AIWpfObject : AIObject
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

    public double width;
    public double height;

    public AIWpfObject(NeuralNetwork neuralNetwork) : base(neuralNetwork)
    {
    }

    public abstract void Update(IEnumerable<AIWpfObject> targets, IEnumerable<AIWpfObject> siblings);

    public Vector2 GetPosition()
    {
     return new Vector2(
          (float)(position.X + (width / 2)),
          (float)(position.Y + (height / 2)));

    }

    protected Vector2? GetCloseEntities(double distance ,Vector2 actualPoint, IEnumerable<AIWpfObject> targets, out int number)
    {
      var orderedTargets = targets
      .Select(x => new { target = x, distance = Vector2.Distance(actualPoint, x.GetPosition()) })
      .OrderBy(x => x.distance);


      var closeTargets = orderedTargets.Where(x => x.distance < distance);

      Vector2? targetPosition = null;

      if (closeTargets.Any())
      {
        targetPosition = new Vector2(
        GetMedian(closeTargets.Select(x => x.target.GetPosition().X).ToArray()),
         GetMedian(closeTargets.Select(x => x.target.GetPosition().Y).ToArray()));
      }

      number = closeTargets.Count();

      return targetPosition;
    }

    public static float GetMedian(float[] sourceNumbers)
    {
      //Framework 2.0 version of this method. there is an easier way in F4        
      if (sourceNumbers == null || sourceNumbers.Length == 0)
        throw new System.Exception("Median of empty array not defined.");

      //make sure the list is sorted, but use a new array
      float[] sortedPNumbers = (float[])sourceNumbers.Clone();
      Array.Sort(sortedPNumbers);

      //get the median
      int size = sortedPNumbers.Length;
      int mid = size / 2;
      float median = (size % 2 != 0) ? (float)sortedPNumbers[mid] : ((float)sortedPNumbers[mid] + (float)sortedPNumbers[mid - 1]) / 2;
      
      return median;
    }

  }
}
