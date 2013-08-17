using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GravityGuy.Support.Geometry;

namespace GravityGuy.Support.Game
{
    public struct LevelInformation
    {
        public static readonly LevelInformation None = new LevelInformation() { Start = double.PositiveInfinity };

        public int Number;
        public double MaxXSpeed;
        public double Start;

        public static implicit operator LevelState(LevelInformation information)
        {
            return new LevelState() { Number = information.Number, MaxXSpeed = information.MaxXSpeed };
        }
    }

    public struct StageInformation
    {
        public IReadOnlyList<Platform> Platforms;
        public IReadOnlyList<Coin> Coins;
        public IReadOnlyList<LevelInformation> Levels;
        public Rect Viewport;
        public Rect Size;
        public Point PlayerStart;

        public void ResetCoins()
        {
            this.Coins = this.Coins.Select(coin => Coin.Reset(coin)).ToList();
        }

        public static StageInformation Load(string path, TaskScheduler uiTaskScheduler)
        {
            StageInformation result = new StageInformation();
            List<Platform> platforms = new List<Platform>();
            List<Coin> coins = new List<Coin>();
            List<LevelInformation> levels = new List<LevelInformation>();

            var stageLines = File.ReadAllLines(path);

            result.Coins = coins;
            result.Platforms = platforms;
            result.Levels = levels;

            levels.Add(new LevelInformation() { MaxXSpeed = 4, Number = 1, Start = 0 });

            foreach (var parameter in stageLines[0].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parameterInfo = parameter.Split(new char[] {'='}, 2);

                if (parameterInfo.Length != 2)
                {
                    throw new ArgumentException("Stage file format is invalid.");
                }

                var name = parameterInfo[0];
                var value = parameterInfo[1];

                if (string.Equals(name, "Width", StringComparison.OrdinalIgnoreCase))
                {
                    result.Size = new Rect(0, 0, int.Parse(value), result.Size.Height);
                }
                else if (string.Equals(name, "Height", StringComparison.OrdinalIgnoreCase))
                {
                    result.Size = new Rect(0, 0, result.Size.Width, int.Parse(value));
                    result.Viewport = new Rect(0, 0, result.Viewport.Width, int.Parse(value));
                }
                else if (string.Equals(name, "View", StringComparison.OrdinalIgnoreCase))
                {
                    result.Viewport = new Rect(0, 0, int.Parse(value), result.Viewport.Height);
                }
                else if (string.Equals(name, "Levels", StringComparison.OrdinalIgnoreCase))
                {
                    var last = levels.Last();

                    foreach (var level in value.Split(','))
                    {
                        int plane = int.Parse(level);

                        if (last.Start >= plane)
                        {
                            throw new ArgumentException("Stage file format is invalid - non-increasing level spotted.");
                        }

                        levels.Add(last = new LevelInformation() { Number = last.Number + 1, MaxXSpeed = last.MaxXSpeed * 1.2, Start = plane });
                    }
                }
                else
                {
                    throw new ArgumentException("Stage file format is invalid - unknown parameter specified: " + name);
                }
            }

            if (result.Size.Width < 1 || result.Size.Height < 1 || result.Viewport.Width < 1)
            {
                throw new ArgumentException("Stage file format is invalid - width, height, and/or view parameters are missing.");
            }

            if (stageLines.Length > result.Size.Height + 1)
            {
                throw new ArgumentException("Stage file format is invalid - there are too many lines specified.");
            }

            var width = (int)result.Size.Width;
            var height = (int)result.Size.Height;

            for (int c = 1; c < stageLines.Length; c++)
            {
                int y = (height - c);
                var line = stageLines[c];
                int start = -1;

                line = line.Substring(0, Math.Min(line.Length, width));

                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] == '=')
                    {
                        if (start == -1)
                        {
                            start = i;
                        }
                    }
                    else
                    {
                        if (start != -1)
                        {
                            platforms.Add(CreatePlatform(start, i, y, result.Size));
                            start = -1;
                        }

                        if (line[i] == 'C')
                        {
                            coins.Add(new Coin(new Circle(new Point(i + 0.5, y + 0.5), 0.5), uiTaskScheduler));
                        }
                        else if (line[i] == 'P')
                        {
                            result.PlayerStart = new Point(i, y);
                        }
                    }
                }

                if (start != -1)
                {
                    platforms.Add(CreatePlatform(start, line.Length, y, result.Size));
                }
            }

            return result;
        }

        private static Platform CreatePlatform(int start, int stop, int y, Rect size)
        {
            PlatformOrientation orientation = PlatformOrientation.Inner;

            if (y == 0)
            {
                orientation = PlatformOrientation.OuterBottom;
            }
            else if (y == size.Height - 1)
            {
                orientation = PlatformOrientation.OuterTop;
            }

            return new Platform(orientation, new Quadrilateral(new Point(start, y), new Vector(stop - start, 1)));
        }
    }
}