using System.Reactive.Linq;
using System;
using System.Threading;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MoonLanding_NeuralNetwork
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    private int ghostsCount = 350;
    private int targetCount = 10;

    private int[] layers = new int[] { 6, 12, 12, 4 };
    private List<NeuralNetwork> ghostsNets = new List<NeuralNetwork>();
    private List<NeuralNetwork> targetsNets = new List<NeuralNetwork>();

    ObservableCollection<Ghost> Ghosts = new ObservableCollection<Ghost>();
    ObservableCollection<Target> Targets = new ObservableCollection<Target>();

    public event PropertyChangedEventHandler PropertyChanged;

    int canvasWidth = 1000;
    int canvasHeight = 1000;


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


    protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
      OnPropertyChanged(propertyName);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MainWindow()
    {
      InitializeComponent();

      for (int i = 0; i < ghostsCount; i++)
      {
        NeuralNetwork net = new NeuralNetwork(layers);
        net.Mutate();
        ghostsNets.Add(net);
      }

      for (int i = 0; i < targetCount; i++)
      {
        NeuralNetwork net = new NeuralNetwork(layers);
        net.Mutate();
        targetsNets.Add(net);
      }

      CreateGhosts(ghostsCount);
      CreateTargets(targetCount);

      DataContext = this;

      Observable.Interval(TimeSpan.FromSeconds(15))
      .ObserveOn(Application.Current.Dispatcher)
      .Subscribe((x) =>
      {
        UpdateGeneration();
      });


      Observable.Interval(TimeSpan.FromSeconds(0.05))
    .ObserveOn(Application.Current.Dispatcher)
    .Subscribe(async (x) =>
   {
     //await SemaphoreSlim.WaitAsync();
     Tick();

    // SemaphoreSlim.Release();
   });

    }

    private static readonly Random random = new Random();
    private SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);


    public static float RandomNumberBetween(float minValue, float maxValue)
    {
      var next = random.NextDouble();

      return (float)(minValue + (next * (maxValue - minValue)));
    }

    public async void Tick()
    {
      var ghostsList = Ghosts.ToList();
      var targetList = Targets.ToList();


      foreach (var ghost in ghostsList)
      {
        ghost.Update(targetList, null);
        var newX = Canvas.GetLeft(ghost.point) + ghost.vector.X;
        var newY = Canvas.GetTop(ghost.point) + ghost.vector.Y;

        if (newX > 0 && newX < canvasWidth - ghost.point.Width)
          Canvas.SetLeft(ghost.point, newX);

        if (newY > 0 && newY < canvasHeight - ghost.point.Height)
          Canvas.SetTop(ghost.point, newY);

        ghost.position = new Vector2((float)newX, (float)newY);
      }

      foreach (var target in targetList)
      {
        target.Update(ghostsList, targetList);

        var newX = Canvas.GetLeft(target.point) + target.vector.X;
        var newY = Canvas.GetTop(target.point) + target.vector.Y;


        var fitness = (byte)(target.net.GetFitness() / 7000);
        target.point.Fill = new SolidColorBrush(Color.FromRgb((byte)(255 - fitness), fitness, 0));

        if (newX > 0 && newX < target.point.Width)
          Canvas.SetLeft(target.point, newX);

        if (newY > 0 && newY < canvasHeight - target.point.Height)
          Canvas.SetTop(target.point, newY);

        target.position = new Vector2((float)newX, (float)newY);
      }
    }

    private void UpdateGeneration()
    {
      UpdateGhosts();
      UpdateTargets();

      GenerationCount++;
      CreateGhosts(ghostsCount);
      CreateTargets(targetCount);

      for (int i = 0; i < ghostsCount; i++)
      {
        ghostsNets[i].SetFitness(0f);
        Ghosts[i].Target = null;
      }
    }

    private void CreateGhosts(int count)
    {
      for (int i = 0; i < count; i++)
      {
        if (Ghosts.Count > 0)
        {
          var oldtarget = Ghosts[0];
          Ghosts.Remove(oldtarget);
          canvas.Children.Remove(oldtarget.point);
        }

      }

      for (int i = 0; i < count; i++)
      {
        var newGhost = new Ghost(ghostsNets[i]);

        Ghosts.Add(newGhost);
        canvas.Children.Add(newGhost.point);

        var newX = random.Next(0, (int)(canvasWidth - newGhost.point.Width));
        var newY = random.Next(0, (int)(canvasHeight - newGhost.point.Height));

        Canvas.SetLeft(newGhost.point, newX);
        Canvas.SetTop(newGhost.point, newY);
        Canvas.SetZIndex(newGhost.point, 100);


        newGhost.position = new Vector2((float)newX, (float)newY);
      }
    }

    private void UpdateGhosts()
    {
      ghostsNets = ghostsNets.OrderBy(x => x.GetFitness()).ToList();
      for (int i = 0; i < ghostsCount / 2; i++)
      {
        var successIndex = i + (ghostsCount / 2);

        var sucessNet = new NeuralNetwork(ghostsNets[successIndex]);
        var failedNet = new NeuralNetwork(sucessNet);
        failedNet.Mutate();

        ghostsNets[i] = failedNet;
        ghostsNets[successIndex] = sucessNet;

        Ghosts[i].net = failedNet;
        Ghosts[successIndex].net = sucessNet;
      }
    }

    private void UpdateTargets()
    {
      targetsNets = targetsNets.OrderBy(x => x.GetFitness()).ToList();
      for (int i = 0; i < targetCount / 2; i++)
      {
        var successIndex = i + (targetCount / 2);

        var sucessNet = new NeuralNetwork(targetsNets[successIndex]);
        var failedNet = new NeuralNetwork(sucessNet);
        failedNet.Mutate();

        targetsNets[i] = failedNet;
        targetsNets[successIndex] = sucessNet;

        Targets[i].net = failedNet;
        Targets[successIndex].net = sucessNet;
      }

    }

    private void CreateTargets(int count)
    {
      for (int i = 0; i < count; i++)
      {
        if (Targets.Count > 0)
        {
          var oldtarget = Targets[0];
          Targets.Remove(oldtarget);
          canvas.Children.Remove(oldtarget.point);
        }

      }

      for (int i = 0; i < count; i++)
      {

        var newTarget = new Target(targetsNets[i]);

        Targets.Add(newTarget);
        canvas.Children.Add(newTarget.point);

        var newX = random.Next(0, (int)(canvasWidth - newTarget.point.Width));
        var newY = random.Next(0, (int)(canvasHeight - newTarget.point.Height));

        Canvas.SetLeft(newTarget.point, newX);
        Canvas.SetTop(newTarget.point, newY);
      }
    }
  }
}
