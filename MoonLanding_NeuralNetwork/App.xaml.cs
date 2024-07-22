using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VCore.WPF;
using VCore.WPF.Views.SplashScreen;

namespace MoonLanding_NeuralNetwork
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  /// 


  public class NeuralNetworkWPFApp : VApplication<MainWindow,MainWindowViewModel, SplashScreenView>
  { 

  }

  public partial class App : NeuralNetworkWPFApp
  {
  }
}
