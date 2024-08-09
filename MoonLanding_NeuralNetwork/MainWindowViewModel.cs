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
using System.IO;
using TradingBroker.MachineLearning;
using System.Diagnostics;
using NeuralNetwork_WPF.Ghosts;
using NeuralNetwork_WPF.Rocket;

namespace MoonLanding_NeuralNetwork
{
  public class MainWindowViewModel : BaseMainWindowViewModel
  {
    
    public MainWindowViewModel(IViewModelsFactory viewModelsFactory) : base(viewModelsFactory)
    {
      GhostSimulator = new GhostSimulator(viewModelsFactory);
      GhostSimulator2 = new GhostSimulator2(viewModelsFactory);
      RocketSimulator = new RocketSimulator(viewModelsFactory);
    }


    public GhostSimulator GhostSimulator { get; set; }

    public GhostSimulator2 GhostSimulator2 { get; set; }

    public RocketSimulator RocketSimulator { get; set; }
  }
}
