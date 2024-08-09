using System.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VCore.Standard.Modularity.Interfaces;
using LiveCharts;
using VNeuralNetwork;

namespace MoonLanding_NeuralNetwork
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, IView
  {
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
        }
      }
    }

    #endregion


    public MainWindow()
    {
      DataContext = this;

      InitializeComponent();
     

      Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      if(DataContext is MainWindowViewModel viewModel)
      {
        viewModel.GhostSimulator2.canvas = canvas;
        viewModel.GhostSimulator2.Start();

      }
    }
  }
}
