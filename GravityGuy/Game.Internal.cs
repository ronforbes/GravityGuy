using GravityGuy.Support.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GravityGuy
{
    internal partial class Game
    {
        GameManager gameManager;
        MainWindow window;

        public Game(GameManager gameManager, MainWindow window)
        {
            this.gameManager = gameManager;
            this.window = window;

            gameManager.OnCoinCaptured += (sender, args) => OnCoinCaptured();
            gameManager.OnPropertyChange += OnPropertyChanged;
        }

        public void UpdateCoinDisplay()
        {
            window.CoinsCaptured.Text = coinDisplay.ToString();
        }

		public void Victory()
        {
			gameManager.Victory();
        }

        public Brush VictoryBrush
        {
            get { return GameRunState.Victory.Background; }
            set { GameRunState.Victory.Background = value; }
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "RunState")
            {
                if (gameManager.RunState == GameRunState.Victory)
                {
                    OnVictory();
                }
            }
        }
    }
}
