namespace GravityGuy.Support.Game
{
    /// <summary>
    /// Stateful enumeration associated with character animation state.
    /// </summary>
    public struct CharacterAnimationState
    {
        /// <summary>
        /// Gets animation state assoicated with a falling down character.
        /// </summary>
        public static CharacterAnimationState FallingDown
        { 
            get
            { 
                return new CharacterAnimationState() { SequenceIndex = 0 }; 
            } 
        }

        /// <summary>
        /// Gets animation state associated with a falling up character.
        /// </summary>
        public static CharacterAnimationState FallingUp
        {
            get
            {
                return new CharacterAnimationState() { SequenceIndex = 1 };
            }
        }

        /// <summary>
        /// Gets animation state associated with a grounded down character.
        /// </summary>
        public static CharacterAnimationState Down
        {
            get
            {
                return new CharacterAnimationState() { SequenceIndex = 2 };
            }
        }

        /// <summary>
        /// Gets animation state associated with a grounded up character.
        /// </summary>
        public static CharacterAnimationState Up
        {
            get
            {
                return new CharacterAnimationState() { SequenceIndex = 3 };
            }
        }

        /// <summary>
        /// Gets the key-frame animation sequence index.
        /// </summary>
        public int SequenceIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the current animation key-frame.
        /// </summary>
        public double Frame
        {
            get;
            set; 
        }

        /// <summary>
        /// Advances the frame by the specified amount.
        /// </summary>
        /// <param name="original">Original frame animation.</param>
        /// <param name="frame">Frame amount.</param>
        /// <returns>The updated animation state.</returns>
        public static CharacterAnimationState operator +(CharacterAnimationState original, double frame)
        {
            return new CharacterAnimationState()
            {
                SequenceIndex = original.SequenceIndex,
                Frame = original.Frame + frame
            };
        }
    }
}