using System;
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
    TextBlock textBlock = new TextBlock();
    Ellipse elipse = new Ellipse();
    public Target(NeuralNetwork neuralNetwork) : base(neuralNetwork)
    {

      var grid = new Grid();

      grid.Width = 15;
      grid.Height = 15;

      textBlock.Foreground = Brushes.White;
      textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
      textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
      textBlock.FontSize = 5;


      elipse.Fill = Brushes.Red;

      grid.Children.Add(elipse);
      // grid.Children.Add(textBlock);

      point = grid;

      width = point.Width;
      height = point.Height;
    }


    public bool IsDead;
    private Random random = new Random();

    public override void Update(IEnumerable<AIObject> targets, IEnumerable<AIObject> siblings)
    {

      float[] inputs = new float[net.layers[0]];

      float min = (float)width * 2f;
      var actualPoint = position;

      var targetPosition = GetCloseEntities(100, actualPoint, targets, out var targetsNumber);

      if (targetPosition != null)
      {
        inputs[0] = actualPoint.X > targetPosition.Value.X ? 1 : -1;
        inputs[1] = actualPoint.Y > targetPosition.Value.Y ? 1 : -1;

        inputs[2] = actualPoint.X - targetPosition.Value.X;
        inputs[3] = actualPoint.Y - targetPosition.Value.Y;
      }

      inputs[4] = targetsNumber;
      inputs[5] = siblings.Count();

      inputs[6] = actualPoint.X - min;
      inputs[7] = actualPoint.X - (1000 - min) ;
      inputs[8] = actualPoint.Y- min;
      inputs[9] = actualPoint.Y - (1000 - min);


      float[] output = net.FeedForward(inputs);

      vector.X = output[0] + (output[2] * 4.25f);
      vector.Y = output[1] + (output[3] * 4.22f);

      if (IsDead)
      {
        net.AddFitness(-1000);
        return;
      }


      if (actualPoint.X > min &&
        actualPoint.X < 1000 - min &&
        actualPoint.Y > min &&
        actualPoint.Y < 1000 - min)
      {
        if (Math.Abs(vector.X) <= 0 && Math.Abs(vector.Y) <= 0)
        {
          net.AddFitness(-1);
        }
        else
        {
          net.AddFitness(1);
        }
      }
      else
      {
        net.AddFitness(-100);
      }
    }

    public void ChangeColor()
    {
      var fitness = net.GetFitness();

      textBlock.Text = net.GetFitness().ToString();

      SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));

      if (fitness > 255)
      {
        brush = new SolidColorBrush(Color.FromRgb(50, 0, 255));

        var mod = fitness / 2;

        if (mod <= 255)
        {
          brush = new SolidColorBrush(Color.FromRgb(50, (byte)(255 - fitness), (byte)fitness));
        }
      }
      else if (fitness > 0)
      {
        brush = new SolidColorBrush(Color.FromRgb((byte)(255 - fitness), (byte)fitness, 0));
      }

      elipse.Fill = brush;
    }
  }
}

