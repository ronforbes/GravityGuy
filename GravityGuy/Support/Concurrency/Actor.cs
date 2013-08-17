using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace GravityGuy.Support.Concurrency
{
    /// <summary>
    /// Represents a potentially concurrently actionable game entity. Actor actions
    /// are serialized to preserve the internal consistency of the entities state.
    /// </summary>
    public abstract class Actor
    {
        // Scheduler used to serialize Task execution.
        private readonly ConcurrentExclusiveSchedulerPair concurrencyController;

        // Scheduler associated with the UI thread. Property change notifications are reported back on this thread.
        private readonly TaskScheduler uiThreadScheduler;

        /// <summary>
        /// Initializes a new Actor instance.
        /// </summary>
        /// <param name="uiThreadScheduler">Scheduler associated with the UI thread.</param>
        protected Actor(TaskScheduler uiThreadScheduler)
        {
            if (null == uiThreadScheduler)
            {
                throw new ArgumentNullException("uiThreadScheduler");
            }

            this.concurrencyController = new ConcurrentExclusiveSchedulerPair();
            this.uiThreadScheduler = uiThreadScheduler;
        }

        /// <summary>
        /// An event that is triggered when a property on the actor is changed. All event call-backs
        /// are dispatched on the UI thread.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> OnPropertyChange;

        /// <summary>
        /// Gets the scheduler associated with the UI thread.
        /// </summary>
        protected TaskScheduler TaskScheduler
        {
            get { return this.uiThreadScheduler; }
        }

        /// <summary>
        /// Schedules an game entity operation. All game entity operations are serialized.
        /// </summary>
        /// <typeparam name="TResponse">Type associated with operation response.</typeparam>
        /// <param name="operation">Operation to run.</param>
        /// <returns>Response associated with operation.</returns>
        protected Task<TResponse> ScheduleOperation<TResponse>(Func<Task<TResponse>> operation)
            where TResponse : ActorResponse, new()
        {
            return Task.Factory.StartNew<Task<TResponse>>(operation, CancellationToken.None, TaskCreationOptions.None, this.concurrencyController.ExclusiveScheduler)
                .Unwrap()
                .ContinueWith<TResponse>((antecedent) =>
                {
                    if (antecedent.IsFaulted || antecedent.IsCanceled)
                    {
                        return new TResponse()
                        {
                            Fault = antecedent.Exception.InnerException
                        };
                    }

                    return antecedent.Result;
                }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Schedules an game entity operation. All game entity operations are serialized.
        /// </summary>
        /// <typeparam name="TResponse">Type associated with operation response.</typeparam>
        /// <typeparam name="TMessage">Type associated with operation input.</typeparam>
        /// <param name="operation">Operation to run.</param>
        /// <param name="message">Operation input value.</param>
        /// <returns>Response associated with operation.</returns>
        protected Task<TResponse> ScheduleOperation<TResponse, TMessage>(Func<TMessage, Task<TResponse>> operation, TMessage message)
            where TResponse : ActorResponse, new()
        {
            return this.ScheduleOperation(() => operation(message));
        }

        /// <summary>
        /// Extracts a result from the completed operation.
        /// </summary>
        /// <typeparam name="TResult">Type associated with operation result.</typeparam>
        /// <param name="antecedent">Task associated with completed operation.</param>
        /// <returns>Underlying result supplied by the operation response.</returns>
        protected TResult ExtractResult<TResult>(Task<ActorResponse<TResult>> antecedent)
        {
            if (antecedent.IsFaulted)
            {
                throw antecedent.Exception.InnerException;
            }

            return antecedent.Result.Value;
        }

        /// <summary>
        /// Extracts the result from the completed operation.
        /// </summary>
        /// <param name="antecedent">Task associated with completed operation.</param>
        protected void ExtractResult(Task<ActorResponse> antecedent)
        {
            if (antecedent.IsFaulted)
            {
                throw antecedent.Exception.InnerException;
            }
        }

        /// <summary>
        /// Dispatches a callback on the UI thread.
        /// </summary>
        /// <param name="handler">Callback to dispatch on the UI thread.</param>
        protected void Disptach(EventHandler handler)
        {
            if (null != handler)
            {
                Task.Factory.StartNew(() => handler(this, EventArgs.Empty), CancellationToken.None, TaskCreationOptions.None, this.uiThreadScheduler);
            }
        }

        /// <summary>
        /// Notifies subscribers that a game entity property has changed. This notification
        /// occurs on the UI thread.
        /// </summary>
        /// <param name="property">Describes the property that changed.</param>
        protected void PropertyChanged(PropertyChangedEventArgs property)
        {
            if (null != this.OnPropertyChange)
            {
                Task.Factory.StartNew(() => this.OnPropertyChange(this, property), CancellationToken.None, TaskCreationOptions.None, this.uiThreadScheduler);
            }
        }
    }
}