﻿namespace StyleCopRules
{
    /// <summary>
    /// Represents the rules for StyleCop.
    /// </summary>
    internal enum Rules : sbyte
    {
        /// <summary>
        /// This StyleCop rule makes sure that instance variables are prefixed with an underscore.
        /// </summary>
        InstanceVariablesUnderscorePrefix,

        /// <summary>
        /// This StyleCop rule not be allowed use linq aliases.
        /// </summary>
        DoNotUseLinqAliases,

        /// <summary>
        /// This StyleCop rule makes sure that instance variables are minimum length.
        /// </summary>
        MinimumFieldLength,

        /// <summary>
        /// Enforces the don't use of the built-in types.
        /// </summary>
        DoNotUseBuiltInTypeAliases
    }
}
