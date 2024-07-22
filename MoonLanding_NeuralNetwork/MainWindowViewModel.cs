using System.Reactive.Linq;
using System;

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Collections.ObjectModel;
using VCore.Standard.Helpers;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using LiveCharts;
using VNeuralNetwork;
using VCore.WPF.ViewModels;
using VCore.Standard.Factories.ViewModels;
using System.Windows.Shapes;
using LiveCharts.Wpf;

namespace MoonLanding_NeuralNetwork
{
  public class MainWindowViewModel : BaseMainWindowViewModel
  {
    public Canvas canvas;
    private readonly Random random = new Random();

    private int ghostsCount = 100;
    private int targetCount = 50;

    const int inputNumber = 11;

    int canvasWidth = 1000;
    int canvasHeight = 1000;
    SolidColorBrush ghostFill;

    public MainWindowViewModel(IViewModelsFactory viewModelsFactory) : base(viewModelsFactory)
    {
      ghostFill = (SolidColorBrush)new BrushConverter().ConvertFrom("#35ac60fc");
      GhostManager = new AIManager<Ghost>(viewModelsFactory);
      TargetManager = new AIManager<Target>(viewModelsFactory);
    }

    #region ChartData

    private ChartValues<int> chartData = new ChartValues<int>();

    public ChartValues<int> ChartData
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

    private AIManager<Ghost> GhostManager { get; set; }
    private AIManager<Target> TargetManager { get; set; }

    public IList<Target> Targets
    {
      get
      {
        return TargetManager.Agents;
      }
    }

    public IList<Ghost> Ghosts
    {
      get
      {
        return GhostManager.Agents;
      }
    }


    #region GenerationCount

    private int generationCount;

    public int GenerationCount
    {
      get { return generationCount; }
      set
      {
        if (value != generationCount)
        {
          generationCount = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    #region SucessCount

    private int successCount;

    public int SucessCount
    {
      get { return successCount; }
      set
      {
        if (value != successCount)
        {
          successCount = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    #region BestFitness

    private double bestFitness;

    public double BestFitness
    {
      get { return bestFitness; }
      set
      {
        if (value != bestFitness)
        {
          bestFitness = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    #region TickCount

    private int tickCount;

    public int TickCount
    {
      get { return tickCount; }
      set
      {
        if (value != tickCount)
        {
          tickCount = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    #region BestTickCount

    private int bestTickCount;

    public int BestTickCount
    {
      get { return bestTickCount; }
      set
      {
        if (value != bestTickCount)
        {
          bestTickCount = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    public void Start()
    {
      GhostManager.Initilize(new int[] { 4, 8, 8, 4 }, ghostsCount);
      TargetManager.Initilize(new int[] { inputNumber, inputNumber * 2, inputNumber * 2, 4 }, targetCount);  

      CreateGhosts();
      CreateTargets();

      ScheduleAdGhosts();

      Observable.Interval(TimeSpan.FromSeconds(0.001))
    .ObserveOn(Application.Current.Dispatcher)
    .Subscribe(async (x) =>
    {
      Tick();
    });

      Observable.Interval(TimeSpan.FromSeconds(10)).ObserveOnDispatcher().Subscribe((x) =>
      {
        RaisePropertyChanged(nameof(ChartData));
        RaisePropertyChanged(nameof(Labels));
      });


    }

    private List<Target> liveTargets = new List<Target>();
    public void Tick()
    {
      TickCount++;

      foreach (var target in Targets.Where(x => x.Health <= 0).ToList())
      {
        canvas.Children.Remove(target.point);
        liveTargets.Remove(target);
        target.IsDead = true;
      }

      SucessCount = liveTargets.Count;

      var ghostsList = Ghosts.ToList();
      var targetList = liveTargets.ToList();

      var threads = new List<Task>();


      var list = ghostsList.SplitList(35);

      foreach (var split in list)
      {
        var thread = Task.Factory.StartNew(() =>
        {
          foreach (var ghost in split)
          {
            ghost.Update(liveTargets, liveTargets);
          }
        });

        threads.Add(thread);
      }

      var listT = Targets.SplitList(25);

      foreach (var split in listT)
      {
        var thread = Task.Factory.StartNew(() =>
        {
          foreach (var target in split)
          {
            target.Update(ghostsList, new List<AIWpfObject>());
          }
        });

        threads.Add(thread);
      }

      Task.WaitAll(threads.ToArray());

      foreach (var ghost in ghostsList)
      {

        var newX = Canvas.GetLeft(ghost.point) + ghost.vector.X;
        var newY = Canvas.GetTop(ghost.point) + ghost.vector.Y;

        if (newX > 0 && newX < canvasWidth - ghost.point.Width)
          Canvas.SetLeft(ghost.point, newX);

        if (newY > 0 && newY < canvasHeight - ghost.point.Height)
          Canvas.SetTop(ghost.point, newY);

        ghost.position = new Vector2((float)Canvas.GetLeft(ghost.point), (float)Canvas.GetTop(ghost.point));
      }

      var bestTarget = liveTargets.OrderByDescending(x => x.NeuralNetwork.Fitness).FirstOrDefault();

      foreach (var target in targetList)
      {
        var newX = Canvas.GetLeft(target.point) + target.vector.X;
        var newY = Canvas.GetTop(target.point) + target.vector.Y;

        if (random.Next(0, 10000) < 2)
        {
          AddTargetToUi(TargetManager.AddAgent(new NeuralNetwork(bestTarget.NeuralNetwork)));
          target.NeuralNetwork.AddFitness(1000);
        }

        target.ChangeColor();

        if (newX > 0 && newX < canvasWidth - target.point.Width)
          Canvas.SetLeft(target.point, newX);

        if (newY > 0 && newY < canvasHeight - target.point.Height)
          Canvas.SetTop(target.point, newY);

        target.position = new Vector2((float)Canvas.GetLeft(target.point), (float)Canvas.GetTop(target.point));
      }

      if (SucessCount == 0)
      {
        UpdateGeneration();
      }
    }


    #region ScheduleAdGhosts

    SerialDisposable serialDisposable = new SerialDisposable();
    private void ScheduleAdGhosts()
    {
      serialDisposable.Disposable?.Dispose();

      serialDisposable.Disposable = Observable.Interval(TimeSpan.FromSeconds(3))
     .ObserveOn(Application.Current.Dispatcher)
     .Subscribe((x) =>
     {
       var ticks = TickCount / 1000;
       var bestGhost = Ghosts.OrderByDescending(x => x.NeuralNetwork.Fitness).FirstOrDefault();

       ghostFill.Freeze();

       if (bestGhost != null && GhostManager.Agents.Count < 500)
       {
         for (int i = 0; i < ticks; i++)
         {
           AddGhostToUi(GhostManager.AddAgent(new NeuralNetwork(bestGhost.NeuralNetwork)));
         }
       }
     });
    }

    #endregion

    #region UpdateGeneration

    private void UpdateGeneration()
    {
      if (SucessCount > BestFitness)
      {
        BestFitness = SucessCount;
      }

      if (TickCount > BestTickCount)
      {
        BestTickCount = TickCount;
      }

      ChartData.Add(TickCount);
      Labels.Add(GenerationCount);


      GenerationCount++;
      TickCount = 0;

      GhostManager.UpdateGeneration();
      TargetManager.UpdateGeneration();

      CreateTargets();
      CreateGhosts();

      ScheduleAdGhosts();
    }

    #endregion

    #region CreateTargets

    private void CreateTargets()
    {
      for (int i = 0; i < Targets.Count; i++)
      {
        canvas.Children.Remove(Targets[i].point);
      }

      TargetManager.CreateAgents();

      liveTargets = new List<Target>();

      SucessCount = liveTargets.Count;

      foreach (var target in Targets)
      {
        AddTargetToUi(target);
      }
    }

    #endregion

    #region CreateTargets

    private void CreateGhosts()
    {
      for (int i = 0; i < Ghosts.Count; i++)
      {
        canvas.Children.Remove(Ghosts[i].point);
      }


      GhostManager.CreateAgents();

      foreach (var ghost in Ghosts)
      {
        AddGhostToUi(ghost);
      }

    }

    #endregion

    #region AddGhostToUi

    private void AddGhostToUi(Ghost newGhost)
    {
      ((Ellipse)newGhost.point).Fill = ghostFill;

      canvas.Children.Add(newGhost.point);

      var newX = random.Next(0, (int)(canvasWidth - newGhost.point.Width));
      var newY = random.Next(0, (int)(canvasHeight - newGhost.point.Height));

      Canvas.SetLeft(newGhost.point, newX);
      Canvas.SetTop(newGhost.point, newY);
      Canvas.SetZIndex(newGhost.point, 100);


      newGhost.position = new Vector2((float)newX, (float)newY);
    }

    #endregion

    #region AddTargetToUi

    private void AddTargetToUi(Target newTarget)
    {
      canvas.Children.Add(newTarget.point);

      var newX = random.Next(0, (int)(canvasWidth - (newTarget.point.Width * 3)));
      var newY = random.Next(0, (int)(canvasHeight - (newTarget.point.Height * 3)));

      newTarget.position = new Vector2((float)newX, (float)newY);
      liveTargets.Add(newTarget);
      Canvas.SetLeft(newTarget.point, newX);
      Canvas.SetTop(newTarget.point, newY);

    }

    #endregion

  }
}
