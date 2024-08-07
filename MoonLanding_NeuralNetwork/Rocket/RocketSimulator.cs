using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using VCore.Standard;
using VCore.Standard.Factories.ViewModels;
using VCore.WPF;
using VNeuralNetwork;

namespace NeuralNetwork_WPF.Rocket
{
  public class AiRocket : AIObject
  {
    public bool IsInCollistion { get; set; }
    public const int MaxHP = 100;
    public int HP { get; set; } = MaxHP;
    public double angle = 0;
    public Vector velocity = new Vector(0, 0);

    public AiRocket(NeuralNetwork neuralNetwork) : base(neuralNetwork)
    {
      Rectangle = new Rectangle();
      Rectangle.Height = 10;
      Rectangle.Width = 20;
      Rectangle.Fill = Brushes.Yellow;


      Rectangle.RenderTransform = new RotateTransform();

      RotateTransform rotateTransform = Rectangle.RenderTransform as RotateTransform;
      rotateTransform.Angle = -90;
      angle = rotateTransform.Angle;
    }

    public Rectangle Rectangle { get; set; }
  }

  public class RocketSimulator : ViewModel
  {
    private double gravity = 0.25;
    private DispatcherTimer timer;
    public double thrustPower = 0.35;
    public double rotationSpeed = 1.0;

    public Canvas canvas { get; set; }

    public Rectangle Target { get; set; }

    AIManager<AiRocket> aIManager;


    #region ChartData

    private ChartValues<float> chartData = new ChartValues<float>();

    public ChartValues<float> ChartData
    {
      get { return chartData; }
      set
      {
        if (value != chartData)
        {
          chartData = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    public ObservableCollection<int> Labels { get; set; } = new ObservableCollection<int>();

    public RocketSimulator(IViewModelsFactory viewModelsFactory)
    {
      aIManager = new AIManager<AiRocket>(viewModelsFactory);


      new Tester(viewModelsFactory).Test();

      InitializeRocket();

      // Initialize timer
      timer = new DispatcherTimer();
      timer.Interval = TimeSpan.FromMilliseconds(20);
      timer.Tick += Timer_Tick;
    }

    private void InitializeRocket()
    {
      Target = new Rectangle();
      Target.Height = 5;
      Target.Width = 100;
      Target.Fill = Brushes.Red;

      aIManager.InitializeManager(new int[] { 7, 14, 14, 2 }, 50);
    }

    Random random = new Random();


    public double GetRandomDouble(double minValue, double maxValue)
    {
      if (minValue > maxValue)
      {
        throw new ArgumentException("minValue should be less than or equal to maxValue");
      }

      return random.NextDouble() * (maxValue - minValue) + minValue;
    }

    public void Start()
    {
      foreach (var agent in aIManager.Agents)
      {
        canvas.Children.Add(agent.Rectangle);

        Canvas.SetLeft(agent.Rectangle, 500);
        Canvas.SetTop(agent.Rectangle, 500);
      }

      canvas.Children.Add(Target);

      Canvas.SetLeft(Target, GetRandomDouble(Target.Width, canvas.ActualWidth - Target.Width - 10));
      Canvas.SetTop(Target, GetRandomDouble(200, 500));

      timer.Start();


      liveRockets.AddRange(aIManager.Agents);

      aIManager.Agents.ForEach(x => { x.HP = AiRocket.MaxHP; });
    }

    public bool IsCollidingWith(AiRocket target, AiRocket other)
    {
      return IsColliding(
        Canvas.GetLeft(target.Rectangle),
        Canvas.GetTop(target.Rectangle),
        target.Rectangle.Width,
        target.Rectangle.Height,
        ((RotateTransform)target.Rectangle.RenderTransform).Angle,
        Canvas.GetLeft(other.Rectangle),
        Canvas.GetTop(other.Rectangle),
        other.Rectangle.Width,
        other.Rectangle.Height,
        ((RotateTransform)other.Rectangle.RenderTransform).Angle);
    }

    private bool IsColliding(double x1, double y1, double width1, double height1, double angle1,
                                double x2, double y2, double width2, double height2, double angle2)
    {
      // Get the corners of the rectangles
      var corners1 = GetCorners(x1, y1, width1, height1, angle1);
      var corners2 = GetCorners(x2, y2, width2, height2, angle2);

      // Get the axes to project onto
      var axes = new Vector[]
      {
                GetEdgeAxis(corners1[0], corners1[1]),
                GetEdgeAxis(corners1[1], corners1[2]),
                GetEdgeAxis(corners2[0], corners2[1]),
                GetEdgeAxis(corners2[1], corners2[2])
      };

      // Check for overlap on all axes
      foreach (var axis in axes)
      {
        var projection1 = ProjectOntoAxis(corners1, axis);
        var projection2 = ProjectOntoAxis(corners2, axis);

        if (!IsOverlap(projection1, projection2))
        {
          return false; // No collision if there's a gap on any axis
        }
      }

      return true; // Collision if there are no gaps
    }

    private Point[] GetCorners(double x, double y, double width, double height, double angle)
    {
      var centerX = x + width / 2;
      var centerY = y + height / 2;

      var transform = new RotateTransform(angle, centerX, centerY);
      var corners = new Point[]
      {
                new Point(x, y),
                new Point(x + width, y),
                new Point(x + width, y + height),
                new Point(x, y + height)
      };

      for (int i = 0; i < corners.Length; i++)
      {
        corners[i] = transform.Transform(corners[i]);
      }

      return corners;
    }

    private Vector GetEdgeAxis(Point p1, Point p2)
    {
      var edge = p2 - p1;
      return new Vector(-edge.Y, edge.X); // Perpendicular to the edge
    }

    private Tuple<double, double> ProjectOntoAxis(Point[] corners, Vector axis)
    {
      double min = double.PositiveInfinity;
      double max = double.NegativeInfinity;

      foreach (var corner in corners)
      {
        double projection = (corner.X * axis.X + corner.Y * axis.Y) / axis.Length;
        min = Math.Min(min, projection);
        max = Math.Max(max, projection);
      }

      return Tuple.Create(min, max);
    }

    private bool IsOverlap(Tuple<double, double> projection1, Tuple<double, double> projection2)
    {
      return projection1.Item1 < projection2.Item2 && projection1.Item2 > projection2.Item1;
    }

    int tickCount = 0;
    int maxTick = 50000;
    List<AiRocket> liveRockets = new List<AiRocket>();
    private void Timer_Tick(object sender, EventArgs e)
    {
      tickCount++;

      foreach (var agent in liveRockets)
      {
        float[] inputs = new float[] {
          (float)agent.velocity.X,
          (float)agent.velocity.Y,
          (float)agent.angle,
          (float)Canvas.GetLeft(agent.Rectangle),
          (float)Canvas.GetTop(agent.Rectangle),
          (float)Canvas.GetLeft(Target),
          (float)Canvas.GetTop(Target),
        };

        var output = agent.NeuralNetwork.FeedForward(inputs);

        var power = output[0] * thrustPower;
        var angle = output[1] * rotationSpeed;

        if (power > 0)
          ApplyThrust(agent, power);


        agent.angle += angle;
        RotateRocket(agent);

        if (Canvas.GetTop(agent.Rectangle) < canvas.ActualHeight)
          ApplyGravity(agent);



        UpdatePosition(agent);
      }

      foreach (var target in liveRockets.Where(x => x.HP <= 0).ToList())
      {
        canvas.Children.Remove(target.Rectangle);
      }

      liveRockets = liveRockets.Where(x => x.HP > 0).ToList();

      var isTarget = liveRockets.Any(x => IsCollidingWith(x, GetTarget()));

      if (isTarget)
      {
        Target.Fill = Brushes.Blue;
      }
      else
      {
        Target.Fill = Brushes.Red;
      }


      if (liveRockets.Count == 0 || tickCount == maxTick)
        UpdateGeneration();

    }

    private void ApplyThrust(AiRocket aiRocket, double power)
    {
      // Calculate thrust direction based on current angle
      double radians = aiRocket.angle * (Math.PI / 180);
      Vector thrust = new Vector(Math.Cos(radians), Math.Sin(radians)) * power;
      aiRocket.velocity += thrust;
    }

    private void ApplyGravity(AiRocket aiRocket)
    {
      aiRocket.velocity.Y += gravity;
    }

    private void RotateRocket(AiRocket aiRocket)
    {
      RotateTransform rotateTransform = aiRocket.Rectangle.RenderTransform as RotateTransform;
      rotateTransform.Angle = aiRocket.angle;
    }

    private void UpdatePosition(AiRocket aiRocket)
    {
      double x = Canvas.GetLeft(aiRocket.Rectangle) + aiRocket.velocity.X;
      double y = Canvas.GetTop(aiRocket.Rectangle) + aiRocket.velocity.Y;

      bool hit = false;
      // Keep the rocket within the bounds of the canvas
      if (x <= 0)
      {
        aiRocket.velocity.X = 0;
        hit = true;

      }
      if (x >= canvas.ActualWidth)
      {      
        aiRocket.velocity.X = 0;
        hit = true;
      }

      if (y <= 0)
      {
        aiRocket.velocity.Y = 0;
        hit = true;
      }
      if (y >= canvas.ActualHeight - 100)
      {
        if (y >= canvas.ActualHeight ){
          aiRocket.velocity.Y = 0;
          aiRocket.velocity.X = 0;
        }

        hit = true;
      }

      if (!hit)
      {
        aiRocket.NeuralNetwork.AddFitness(1);

        aiRocket.Rectangle.Fill = Brushes.Yellow;
      }
      else
      {
        aiRocket.HP--;
        aiRocket.Rectangle.Fill = Brushes.Red;
      }



      if (IsCollidingWith(aiRocket, GetTarget()))
      {
        aiRocket.NeuralNetwork.AddFitness(5);

        //if(!aiRocket.IsInCollistion)
        //{
        //  aiRocket.velocity.Y = 0;
        //  aiRocket.velocity.X = 0;
        //}

        aiRocket.IsInCollistion = true;
      }
      else
      {
        aiRocket.IsInCollistion = false;
      }

      x = Canvas.GetLeft(aiRocket.Rectangle) + aiRocket.velocity.X;
      y = Canvas.GetTop(aiRocket.Rectangle) + aiRocket.velocity.Y;

      Canvas.SetLeft(aiRocket.Rectangle, x);
      Canvas.SetTop(aiRocket.Rectangle, y);
    }


    private AiRocket GetTarget()
    {
      var fake = new AiRocket(new NeuralNetwork());
      fake.Rectangle = Target;
      fake.angle = 0;
      fake.Rectangle.RenderTransform = new RotateTransform();

      return fake;
    }

    private void UpdateGeneration()
    {
      timer.Stop();

      VSynchronizationContext.InvokeOnDispatcher(() =>
      {
        ChartData.Add(aIManager.Agents.Max(x => x.NeuralNetwork.Fitness));
        Labels.Add(aIManager.Generation);
      });

      tickCount = 0;
      canvas.Children.Clear();
      aIManager.UpdateGeneration();
      timer.Start();

      Start();
    }
  }
}
