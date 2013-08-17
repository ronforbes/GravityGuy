using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GravityGuy.Support.Game;

namespace GravityGuy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameManager gameManager;
        private PresentationManager presentationManager;
        private Game game;

        public MainWindow()
        {
            InitializeComponent();

            this.gameManager = new GameManager(TaskScheduler.FromCurrentSynchronizationContext());
            
            this.presentationManager = new PresentationManager(this.GameCanvas, this.gameManager);

            this.gameManager.OnStart += (o, e) => this.presentationManager.Reset();
            this.gameManager.OnPresent += (o, e) => this.presentationManager.Update();

            this.game = new Game(gameManager, this);
        }

        private void GameCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.gameManager.Player.GravityFlip();
        }

        private void GameCanvas_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.gameManager.Start();
            }
            else if (e.Key == Key.P)
            {
                this.gameManager.Pause();
            }
            else if (e.Key == Key.R)
            {
                this.gameManager.Resume();
            }
        }

        private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            this.GameCanvas.Background = this.gameManager.RunState.Background;

            this.presentationManager.LoadAssets();
        }
    }
}