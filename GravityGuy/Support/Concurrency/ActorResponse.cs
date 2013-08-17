using System;

namespace GravityGuy.Support.Concurrency
{
    /// <summary>
    /// Represents a void response from the Actor operation.
    /// </summary>
    public class ActorResponse
    {
        /// <summary>
        /// Gets a value indicating whether a fault occured.
        /// </summary>
        public bool IsFault
        {
            get { return this.Fault != null; }
        }

        /// <summary>
        /// Gets or sets the fault associated iwth this response.
        /// </summary>
        public Exception Fault
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents a valued response from the Actor operation.
    /// </summary>
    /// <typeparam name="T">Type associated with response.</typeparam>
    public class ActorResponse<T> : ActorResponse
    {
        /// <summary>
        /// Gets the value returned from the actor operation.
        /// </summary>
        public T Value
        {
            get;
            set;
        }
    }
}
