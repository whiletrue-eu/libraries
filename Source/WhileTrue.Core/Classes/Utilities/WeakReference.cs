using System;

namespace WhileTrue.Classes.Utilities
{
#if !NET45
    /// <summary>
    /// Implements a typesafe WeakReference
    /// </summary>
    public class WeakReference<ReferenceType> : WeakReference where ReferenceType:class
    {
        /// <summary/>
        public WeakReference(ReferenceType target, bool trackResurrection)
            : base(target, trackResurrection)
        {
        }

        /// <summary/>
        public WeakReference(ReferenceType target)
            : base(target)
        {
        }

        /// <summary/>
        [Obsolete("Not supported from .Net 4.5 on. Use TryGetTarget instead",false)]
        public new ReferenceType Target
        {
            get { return (ReferenceType) base.Target; }
        }

        /// <summary/>
        [Obsolete("Not supported from .Net 4.5 on. Use TryGetTarget instead", false)]
        public new bool IsAlive
        {
            get
            {
                return base.IsAlive;
            }
        }

        public bool TryGetTarget(out ReferenceType value)
        {
            ReferenceType Target = this.Target;
            bool CouldGetTarget = this.IsAlive;
            value = CouldGetTarget ? Target : null;
            return CouldGetTarget;
        }
    }
#endif
}