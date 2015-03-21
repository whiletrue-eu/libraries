namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Severity of a <see cref="ValidationMessage"/>
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary/>
        Info=1,
        /// <summary/>
        Warning=2,
        /// <summary/>
        ImplicitError = 3,
        /// <summary/>
        Error=4,
        /// <summary/>
        None=-1,
    }
}