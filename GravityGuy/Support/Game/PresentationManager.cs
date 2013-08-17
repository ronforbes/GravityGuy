using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
            this.assets.CharacterSource = new BitmapSource[30];

            int spriteWidth = 42;
            int spriteHeight = 51;
            
            System.Drawing.Rectangle croppedSource = new System.Drawing.Rectangle(0, 0, spriteWidth, spriteHeight);
            Bitmap source = System.Drawing.Image.FromFile("Sprites\\Character.png") as Bitmap;

            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    croppedSource.X = col * spriteWidth;
                    croppedSource.Y = row * spriteHeight;
                    Bitmap target = new System.Drawing.Bitmap((int)croppedSource.Width, (int)croppedSource.Height);
                    Graphics.FromImage(target).DrawImage(source, new System.Drawing.Rectangle(0, 0, target.Width, target.Height), croppedSource, GraphicsUnit.Pixel);
                    BitmapSource frame = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(target.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(target.Width, target.Height));
                    int index = row * 6 + col;
                    this.assets.CharacterSource[index] = frame;
                }
            }
        }

        public void Reset()
        {
            this.gameCanvas.Children.Clear();

            this.entityShapes = new GameEntityShapes()
            {
                Player = new System.Windows.Shapes.Rectangle()
                {
                    Width = this.gameManager.Player.Position.Diagonal.X,
                    Height = this.gameManager.Player.Position.Diagonal.Y,
                    Fill = System.Windows.Media.Brushes.Green
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

            this.entityShapes.Platforms = new List<System.Windows.Shapes.Rectangle>();
            this.entityShapes.CoinShapes = new Dictionary<Coin, Ellipse>();

            foreach (var coin in gameManager.Stage.Coins)
            {
                var radius = coin.Position.Radius;

                var shape = new Ellipse()
                {
                    Fill = System.Windows.Media.Brushes.Gold,
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
                var rectangle = new System.Windows.Shapes.Rectangle();
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

            this.entityShapes.Player.Fill = new ImageBrush(this.assets.CharacterSource[(int)this.gameManager.Player.Animation.Frame % 30]);
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
            public System.Windows.Shapes.Rectangle Player;
            public TranslateTransform PlayerTranslation;

            public TranslateTransform ViewportPan;
            public ScaleTransform ModelViewScale;
            
            public List<System.Windows.Shapes.Rectangle> Platforms;
            public Dictionary<Coin, Ellipse> CoinShapes;
        }

        private class Assets
        {
            public BitmapImage PlatformBottomSource;
            public BitmapImage PlatformTopSource;
            public BitmapImage PlatformInnerSource;
            public BitmapSource[] CharacterSource;
        }
    }
}