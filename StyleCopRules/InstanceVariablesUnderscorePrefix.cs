namespace StyleCopRules
{
    using System;
    using Microsoft.StyleCop;
    using Microsoft.StyleCop.CSharp;

    /// <summary>
    /// This StyleCop Rule makes sure that instance variables are prefixed with an underscore.
    /// </summary>
    [SourceAnalyzer(typeof(CsParser), "StyleCopRulesSettings.xml")]
    public sealed class InstanceVariablesUnderscorePrefix : SourceAnalyzer
    {
        /// <summary>
        /// Temp ssdf.
        /// </summary>
        private Int16 temp;

        /// <summary>
        /// Analyzes the document.
        /// </summary>
        /// <param name="document">The document.</param>
        public override void AnalyzeDocument(CodeDocument document)
        {            
            CsDocument csDocument = (CsDocument)document;
            if (csDocument.RootElement != null && !csDocument.RootElement.Generated)
            {
                this.ProcessElement(csDocument.RootElement);
            }
        }

        /// <summary>
        /// Processes the C# element.
        /// </summary>
        /// <param name="element">The C# element.</param>
        /// <returns>
        /// <c>True</c> if element was processed; otherwise, <c>false</c>.
        /// </returns>
        private Boolean ProcessElement(CsElement element)
        {
            if (this.Cancel)
            {
                return false;
            }

            // Flag a violation if the instance variables are not prefixed with an underscore.
            String elementName = element.Declaration.Name;
            if (element.ElementType == ElementType.Field
                && !element.Generated
                && this.IsPrivateAccess(element.AccessModifier)
                && (!elementName.StartsWith("_", StringComparison.Ordinal)
                    || (elementName.Length > 2 && elementName.Substring(1, 1).ToLower() != elementName.Substring(1, 1))))
            {
                this.AddViolation(element, "InstanceVariablesUnderscorePrefix");
            }

            foreach (CsElement child in element.ChildElements)
            {
                if (!this.ProcessElement(child))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether is private access.
        /// </summary>
        /// <param name="type">The access modifier type.</param>
        /// <returns>
        /// <c>True</c> if is private access; otherwise, <c>false</c>.
        /// </returns>
        private Boolean IsPrivateAccess(AccessModifierType type)
        {
            return (type == AccessModifierType.Private || type == AccessModifierType.Protected || type == AccessModifierType.ProtectedInternal);
        }
    }
}
