namespace StyleCopRules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.StyleCop;
    using Microsoft.StyleCop.CSharp;

    /// <summary>
    /// This StyleCop Rule makes sure that instance variables are prefixed with an underscore.
    /// </summary>
    [SourceAnalyzer(typeof(CsParser))]
    public sealed class InstanceVariablesUnderscorePrefix : SourceAnalyzer
    {
        /// <summary>
        /// Analyzes the document.
        /// </summary>
        /// <param name="document">The document.</param>
        public override void AnalyzeDocument(CodeDocument document)
        {
            CsDocument csdocument = (CsDocument)document;
            if (csdocument.RootElement != null && !csdocument.RootElement.Generated)
            {
                csdocument.WalkDocument(new CodeWalkerElementVisitor<Object>(this.VisitElement), null, null);
            }
        }

        /// <summary>
        /// Visits the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="parentElement">The parent element.</param>
        /// <param name="context">The context.</param>
        /// <returns>True if elemnt was visited.</returns>
        private Boolean VisitElement(CsElement element, CsElement parentElement, Object context)
        {
            // Flag a violation if the instance variables are not prefixed with an underscore.
            if (!element.Generated && element.ElementType == ElementType.Field && element.ActualAccess != AccessModifierType.Public 
                && element.ActualAccess != AccessModifierType.Internal && element.Declaration.Name.ToCharArray()[0] != '_')
            {
                this.AddViolation(element, "InstanceVariablesUnderscorePrefix");
            }
            return true;
        }
    }
}
