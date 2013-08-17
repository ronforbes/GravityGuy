using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GravityGuy.Support.Concurrency;

namespace GravityGuy.Support.Game
{
    /// <summary>
    /// Game entity responsible for overall game update logic.
    /// </summary>
    public sealed class GameManager : Actor
    {
        // Run state property information.
        private static readonly PropertyChangedEventArgs RunStateProperty = new PropertyChangedEventArgs("RunState");
        
        // Level property information.
        private static readonly PropertyChangedEventArgs LevelProperty = new PropertyChangedEventArgs("Level");

        // Cached response associated with synchronous completion - this is used to minimize per-frame heap allocations
        private static readonly Task<ActorResponse> CachedResponse = Task.FromResult(new ActorResponse());

        // Cached response associated with TRUE - this is used to minimize per-frame heap allocations
        private static readonly Task<ActorResponse<bool>> CachedResponseTrue = Task.FromResult(new ActorResponse<bool>() { Value = true });

        // Cached response associated with FALSE - tihs is used to minimize per-frame heap allocations
        private static readonly Task<ActorResponse<bool>> CachedResponseFalse = Task.FromResult(new ActorResponse<bool>() { Value = false });

        // Reusable blank actor response.
        private static readonly ActorResponse BlankResponse = new ActorResponse();

        // Players velocity.
        private Vector velocity;

        // Players x-Acceleration
        private double xAcceleration;

        // Signals game completion.
        private TaskCompletionSource<int> completion;

        // Signals the update loop to exit.
        private CancellationTokenSource exit;

        /// <summary>
        /// Event that is fired when the game starts. This callback is only fired on the UI thread.
        /// </summary>
        public event EventHandler OnStart;

        /// <summary>
        /// Event that is fired when it is time to present the updated game. This callback is only fired on the UI thread.
        /// </summary>
        public event EventHandler OnPresent;

        /// <summary>
        /// Event that is fired when a coin is captured
        /// </summary>
        public event EventHandler<Coin> OnCoinCaptured;

        /// <summary>
        /// Initializes a new GameManager instance.
        /// </summary>
        /// <param name="uiTaskScheduler">TaskScheduler associated with the UI thread.</param>
        public GameManager(TaskScheduler uiTaskScheduler)
            : base(uiTaskScheduler)
        {
            this.exit = new CancellationTokenSource();

            this.RunState = GameRunState.NotStarted;
            this.Stage = StageInformation.Load("Stage.txt", uiTaskScheduler);
        }

        /// <summary>
        /// Gets the current Level.
        /// </summary>
        public LevelState Level
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current Run state. (i.e. Paused, Running, GameOver)
        /// </summary>
        public GameRunState RunState
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all stage information.
        /// </summary>
        public StageInformation Stage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the character associated with the player.
        /// </summary>
        public Character Player
        {
            get;
            private set;
        }

        /// <summary>
        /// Starts a new game. This operation is a no-op if the game is already running.
        /// </summary>
        /// <returns>The inner task can be used to track the completion of the game.</returns>
        public Task<Task> Start()
        {
            return this.ScheduleOperation(this.DoStart)
                .ContinueWith<Task>(this.ExtractResult, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Attempts pause the game. A game may only be paused if it currently in the Running state.
        /// </summary>
        /// <returns>A task whose result will be true if the game was successfully paused; otherwise, false.</returns>
        public Task<bool> Pause()
        {
            return this.ScheduleOperation(this.DoPause)
                .ContinueWith<bool>(this.ExtractResult, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Attempts to resume the game. A game may only be resumed if it currently in the Paused state.
        /// </summary>
        /// <returns>A task whose result will be true if the game was successfully resumed; otherwise, false.</returns>
        public Task<bool> Resume()
        {
            return this.ScheduleOperation(this.DoResume)
                .ContinueWith<bool>(this.ExtractResult, TaskContinuationOptions.ExecuteSynchronously);
        }

        public Task Victory()
        {
            return this.ScheduleOperation(this.DoVictory)
                .ContinueWith(this.ExtractResult, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Game update loop. The update loop executes in the background, it is a fire and forget
        /// async operation that kicks off when the Game starts and terminates after Game-over.
        /// </summary>
        private async void UpdateLoop()
        {
            Stopwatch gameClock = Stopwatch.StartNew();
            double t0;

            t0 = gameClock.Elapsed.TotalSeconds;

            while (exit.IsCancellationRequested == false)
            {
                double dt = gameClock.Elapsed.TotalSeconds - t0;

                await this.ScheduleOperation(this.DoUpdate, Math.Min(0.03, dt));

                if (this.RunState == GameRunState.GameOver)
                {
                    break;
                }

                t0 += dt;

                this.Disptach(this.OnPresent);

                await Task.Delay(16);
            }
        }

        #region Actor Support Code

        /// <summary>
        /// Performs an individual frame update.
        /// </summary>
        /// <param name="dt">Time change.</param>
        /// <returns>A task used to track the completion of this operation.</returns>
        private Task<ActorResponse> DoUpdate(double dt)
        {
            if (this.RunState != GameRunState.Running)
            {
                return CachedResponse;
            }

            List<Task> pendingOperations = new List<Task>(2);
            Vector dp, dg;
            Point p0;
            bool grounded = false;

            this.velocity.X = Math.Min(this.Level.MaxXSpeed, this.velocity.X + this.xAcceleration * dt);
            this.velocity.Y += (this.Player.Gravity.Acceleration * dt).Y; 
            
            p0 = this.Player.Position.Corner;
            dp = this.velocity * dt;
            dg = this.Player.Position.Diagonal;

            foreach (var platform in this.Stage.Platforms)
            {
                var platformRect = platform.Position.Region;
                var intersection = Rect.Intersect(new Rect(p0 + dp, dg), platformRect);

                if (intersection.IsEmpty == false && intersection.Height > 0 && intersection.Width > 0)
                {
                    // Player must intersect from BOTTOM when gravity is up (note TOP and BOTTOM are inverted due to coordinate system inversion along Y-axis)
                    if (this.Player.Gravity.Direction == GravityDirection.Up && intersection.Top == platformRect.Top)
                    {
                        if (intersection.Height < intersection.Width)
                        {
                            dp.Y -= Math.Min(intersection.Height, Math.Abs(dp.Y));
                            grounded = true;
                            this.velocity.Y = 0;

                            intersection = Rect.Intersect(new Rect(p0 + dp, dg), platformRect);
                        }
                    }
                    // Player must intersect from TOP when gravit is down (note TOP and BOTTOM are inverted due to coordinate system inversion along Y-axis)
                    else if (this.Player.Gravity.Direction == GravityDirection.Down && intersection.Bottom == platformRect.Bottom)
                    {
                        if (intersection.Height < intersection.Width)
                        {
                            dp.Y += Math.Min(intersection.Height, Math.Abs(dp.Y));
                            grounded = true;
                            this.velocity.Y = 0;

                            intersection = Rect.Intersect(new Rect(p0 + dp, dg), platformRect);
                        }
                    }
                }

                if (intersection.IsEmpty == false && intersection.Height > 0 && intersection.Width > 0)
                {
                    // Cannot intersect from the right side player only moves left
                    if (intersection.Right != platformRect.Right)
                    {
                        dp.X -= Math.Min(intersection.Width, Math.Abs(dp.X));
                    }
                }
            }

            var playerRect = new Rect(p0 + dp, dg);

            foreach (var coin in this.Stage.Coins)
            {
                if (coin.Availability.CanCapture && playerRect.IntersectsWith(coin.Position.CollisionRegion))
                {
                    pendingOperations.Add(coin.Capture());
                    Dispatch<Coin>(OnCoinCaptured, coin);
                }
            }

            var stage = this.Stage;
            var p1 = p0 + dp;

            stage.Viewport.X = Math.Max(p1.X - 6, Math.Min(p1.X - 3, stage.Viewport.X + dp.X * 0.75));

            this.Stage = stage;

            pendingOperations.Add(this.Player.AdvancePosition(Character.PositionChange.Create(dp, grounded)));

            if (this.Stage.Levels.Count > this.Level.Number && p1.X > this.Stage.Levels[this.Level.Number].Start)
            {
                var next = this.Stage.Levels[this.Level.Number];

                this.Level = new LevelState() { Number = next.Number, MaxXSpeed = next.MaxXSpeed };

                this.PropertyChanged(LevelProperty);
            }

            var stageIntersection = Rect.Intersect(stage.Size, Rect.Offset(this.Player.Position.Region, dp));

            if (stageIntersection.IsEmpty)
            {
                pendingOperations.Add(this.DoGameOver());
            }

            Player.AdvanceAnimation(dp.X * 10);

            return Task.WhenAll(pendingOperations.ToArray())
                .ContinueWith(antecendet => BlankResponse, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Exclusively attempts to start the game.
        /// </summary>
        /// <returns>A task whose response can be used to track the completion of the game.</returns>
        private Task<ActorResponse<Task>> DoStart()
        {
            if (this.RunState == GameRunState.NotStarted || this.RunState == GameRunState.GameOver)
            {
                this.RunState = GameRunState.Running;

                this.Level = this.Stage.Levels.First();
                this.Player = new Character(this.Stage.PlayerStart, new Vector(1, 1.5), this.TaskScheduler);

                var stage = this.Stage;
                stage.ResetCoins();

                this.Stage = stage;

                this.xAcceleration = 5;
                this.velocity = new Vector(this.Level.MaxXSpeed, 0);

                this.completion = new TaskCompletionSource<int>();

                this.UpdateLoop();

                this.PropertyChanged(RunStateProperty);

                this.Disptach(this.OnStart);

                return Task.FromResult(new ActorResponse<Task>() { Value = this.completion.Task });
            }

            return Task.FromResult(new ActorResponse<Task>() { Value = this.completion.Task });
        }

        /// <summary>
        /// Exclusively attempts to pause the game.
        /// </summary>
        /// <returns>A task whose result indicates whether the game was paused..</returns>
        private Task<ActorResponse<bool>> DoPause()
        {
            if (this.RunState == GameRunState.Running)
            {
                this.RunState = GameRunState.Paused;

                this.PropertyChanged(RunStateProperty);

                return CachedResponseTrue;
            }

            return CachedResponseFalse;
        }

        /// <summary>
        /// Exclusively attempts to resume the game.
        /// </summary>
        /// <returns>A task whose result indicates whether the game was resumed.</returns>
        private Task<ActorResponse<bool>> DoResume()
        {
            if (this.RunState == GameRunState.Paused)
            {
                this.RunState = GameRunState.Running;

                this.PropertyChanged(RunStateProperty);

                return CachedResponseTrue;
            }

            return CachedResponseFalse;
        }

        /// <summary>
        /// Exclusively attempts to move the Game into the GameOver state.
        /// </summary>
        /// <returns>A task used to track the completion of this operation.</returns>
        private Task<ActorResponse> DoGameOver()
        {
            if (this.RunState != GameRunState.GameOver)
            {
                this.RunState = GameRunState.GameOver;

                this.PropertyChanged(RunStateProperty);
            }

            return CachedResponse;
        }

        /// <summary>
        /// Exclusively attempts to move the Game into the Victory state.
        /// </summary>
        /// <returns></returns>
        private Task<ActorResponse> DoVictory()
        {
            if (this.RunState != GameRunState.Victory)
            {
                this.RunState = GameRunState.Victory;

                this.PropertyChanged(RunStateProperty);
            }

            return CachedResponse;
        }

        #endregion
    }
}
