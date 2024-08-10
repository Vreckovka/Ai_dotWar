using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VNeuralNetwork;

namespace MoonLanding_NeuralNetwork
{
  public class Target : AIWpfObject
  {
    TextBlock textBlock = new TextBlock();
    Ellipse elipse = new Ellipse();

    public double Health { get; set; } = 255;
    public int size = 10;

    public Target(INeuralNetwork neuralNetwork) : base(neuralNetwork)
    {

      var grid = new Grid();

      grid.Width = size;
      grid.Height = size;

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

    public override void Update(IEnumerable<AIWpfObject> targets, IEnumerable<AIWpfObject> siblings)
    {

      float[] inputs = new float[NeuralNetwork.InputCount];

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
      inputs[7] = actualPoint.X - (1000 - min);
      inputs[8] = actualPoint.Y - min;
      inputs[9] = actualPoint.Y - (1000 - min);
      inputs[10] = (float)Health;

      float[] output = NeuralNetwork.FeedForward(inputs);

      output = NEATManager.MapRangeToNegativeOneToOne(output);

      vector.X = output[0] + (output[2] * 2.5f);
      vector.Y = output[1] + (output[3] * 2.5f);

    
      if (actualPoint.X > min &&
        actualPoint.X + width < 1000 - min &&
        actualPoint.Y > min &&
        actualPoint.Y + height < 1000 - min)
      {
        if (Math.Abs(vector.X) <= 0 && Math.Abs(vector.Y) <= 0)
        {
          Health -= 0.5;
        }
        else
        {
          NeuralNetwork.AddFitness(1);

          Health += 1;
        }
      }
      else
      {
        Health -= 20;
      }
    }

    public void ChangeColor()
    {
      textBlock.Text = Health.ToString();
      var hp = Health;

      if (hp < 0)
        hp = 0;

      var r = (byte)Math.Abs(hp - 255);
      var g = (byte)hp;
      var b = (byte)0;

      if (hp > 255)
      {
        r = 0;
        g = 255;
        b = (byte)(hp - 255);

        if (Health > 510)
        {
          r = (byte)(hp - 510);
          g = 255;
          b = 255;
        }

        if (Health > 765)
        {
          r = 255;
          g = 255;
          r = 255;

          height = size + ((Health - 765) / 100.0);
          width = size + ((Health - 765) / 100.0);

          if (height > 65)
          {
            height = 65;
            width = 65;
          }

          point.Height = height;
          point.Width = width;
        }
      }

      SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(r, g, b));


      elipse.Fill = brush;
    }

   
  }
}

