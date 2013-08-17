﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GravityGuy.Support.Game
{
    public class PresentationManager
    {
        private readonly GameManager gameManager;
        private readonly Canvas gameCanvas;

        private GameEntityShapes entityShapes;
        private Assets assets;

        public PresentationManager(Canvas gameCanvas, GameManager gameManager)
        {
            if (null == gameManager)
            {
                throw new ArgumentNullException("gameManager");
            }

            this.gameCanvas = gameCanvas;
            this.gameManager = gameManager;
        }

        public void LoadAssets()
        {
            if (null != this.assets)
            {
                return;
            }

            this.assets = new Assets();
            this.assets.PlatformBottomSource = new BitmapImage(new Uri("Sprites\\Platform-Bottom.png", UriKind.Relative));
            this.assets.PlatformTopSource = new BitmapImage(new Uri("Sprites\\Platform-Top.png", UriKind.Relative));
            this.assets.PlatformInnerSource = new BitmapImage(new Uri("Sprites\\Platform-Inner.png", UriKind.Relative));
        }

        public void Reset()
        {
            this.gameCanvas.Children.Clear();

            this.entityShapes = new GameEntityShapes()
            {
                Player = new Rectangle()
                {
                    Width = this.gameManager.Player.Position.Diagonal.X,
                    Height = this.gameManager.Player.Position.Diagonal.Y,
                    Fill = Brushes.Green
                },

                PlayerTranslation = new TranslateTransform(0, 0),
                ViewportPan = new TranslateTransform(-this.gameManager.Stage.Viewport.Left, -this.gameManager.Stage.Viewport.Top),
                ModelViewScale = new ScaleTransform(1, 1)
            };

            var playerTransforms = new TransformGroup();

            playerTransforms.Children.Add(this.entityShapes.PlayerTranslation);
            playerTransforms.Children.Add(this.entityShapes.ViewportPan);
            playerTransforms.Children.Add(this.entityShapes.ModelViewScale);

            this.entityShapes.Player.RenderTransform = playerTransforms;

            this.gameCanvas.Children.Add(this.entityShapes.Player);

            this.entityShapes.Platforms = new List<Rectangle>();
            this.entityShapes.CoinShapes = new Dictionary<Coin, Ellipse>();

            foreach (var coin in gameManager.Stage.Coins)
            {
                var radius = coin.Position.Radius;

                var shape = new Ellipse()
                {
                    Fill = Brushes.Gold,
                    Width = radius * 2,
                    Height = radius * 2,
                };

                var transforms = new TransformGroup();

                transforms.Children.Add(new TranslateTransform(coin.Position.Center.X - radius, this.gameManager.Stage.Viewport.Height - coin.Position.Center.Y - radius));
                transforms.Children.Add(this.entityShapes.ViewportPan);
                transforms.Children.Add(this.entityShapes.ModelViewScale);

                shape.RenderTransform = transforms;

                this.gameCanvas.Children.Add(shape);
                this.entityShapes.CoinShapes[coin] = shape;

                coin.OnPropertyChange += this.CoinPropertyChange;
            }

            foreach (var platform in gameManager.Stage.Platforms)
            {
                var rectangle = new Rectangle();
                var platformTransforms = new TransformGroup();
                var dimension = platform.Position.Diagonal;
                var corner = platform.Position.Corner;

                rectangle.Width = dimension.X;
                rectangle.Height = dimension.Y;

                platformTransforms.Children.Add(new TranslateTransform(corner.X, this.gameManager.Stage.Viewport.Height - dimension.Y - corner.Y));
                platformTransforms.Children.Add(this.entityShapes.ViewportPan);
                platformTransforms.Children.Add(this.entityShapes.ModelViewScale);

                rectangle.RenderTransform = platformTransforms;

                if (platform.Orientation == PlatformOrientation.OuterBottom)
                {
                    rectangle.Fill = new ImageBrush(this.assets.PlatformBottomSource)
                    {
                        Viewport = new Rect(0, 0, 1 / platform.Position.Diagonal.X, 1),
                        TileMode = TileMode.Tile
                    };
                }
                else if (platform.Orientation == PlatformOrientation.OuterTop)
                {
                    rectangle.Fill = new ImageBrush(this.assets.PlatformTopSource)
                    {
                        Viewport = new Rect(0, 0, 1 / platform.Position.Diagonal.X, 1),
                        TileMode = TileMode.Tile
                    };
                }
                else
                {
                    rectangle.Fill = new ImageBrush(this.assets.PlatformInnerSource)
                    {
                        Viewport = new Rect(0, 0, 1 / platform.Position.Diagonal.X, 1),
                        TileMode = TileMode.Tile
                    };
                }

                this.gameCanvas.Children.Add(rectangle);
                this.entityShapes.Platforms.Add(rectangle);
            }
        }

        public void Update()
        {
            double viewWidth = this.gameCanvas.RenderSize.Width;
            double viewHeight = this.gameCanvas.RenderSize.Height;
            double mdlWidth = this.gameManager.Stage.Viewport.Width;
            double mdlHeight = this.gameManager.Stage.Viewport.Height;
            double mdlViewLeft = this.gameManager.Stage.Viewport.Left;
            double mdlViewBottom = this.gameManager.Stage.Viewport.Top;

            double scaleX = viewWidth / mdlWidth;
            double scaleY = viewHeight / mdlHeight;

            if (null != this.entityShapes)
            {
                var player = this.gameManager.Player;
                var stage = this.gameManager.Stage;

                this.entityShapes.ModelViewScale.ScaleX = scaleX;
                this.entityShapes.ModelViewScale.ScaleY = scaleY;
                
                this.entityShapes.ViewportPan.X = -stage.Viewport.X;
                this.entityShapes.ViewportPan.Y = -stage.Viewport.Y;

                this.entityShapes.PlayerTranslation.X = player.Position.Corner.X;
                this.entityShapes.PlayerTranslation.Y = (mdlHeight - player.Position.Diagonal.Y) - (player.Position.Corner.Y);
            }
        }

        private void CoinPropertyChange(object sender, PropertyChangedEventArgs args)
        {
            var coin = (Coin)sender;

            if ("Availability" == args.PropertyName)
            {
                Ellipse shape;

                if (this.entityShapes.CoinShapes.TryGetValue(coin, out shape))
                {
                    shape.Visibility = Visibility.Hidden;
                }
            }
        }

        private class GameEntityShapes
        {
            public Rectangle Player;
            public TranslateTransform PlayerTranslation;

            public TranslateTransform ViewportPan;
            public ScaleTransform ModelViewScale;
            
            public List<Rectangle> Platforms;
            public Dictionary<Coin, Ellipse> CoinShapes;
        }

        private class Assets
        {
            public BitmapImage PlatformBottomSource;
            public BitmapImage PlatformTopSource;
            public BitmapImage PlatformInnerSource;
        }
    }
}