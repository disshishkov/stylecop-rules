namespace StyleCopRules
{
    using System;
    using Microsoft.StyleCop;
    using Microsoft.StyleCop.CSharp;
    using System.Collections.Generic;

    /// <summary>
    /// This StyleCop Rule makes sure that instance variables are prefixed with an underscore.
    /// </summary>
    [SourceAnalyzer(typeof(CsParser))]
    public sealed class RusWizardsRules : SourceAnalyzer
    {
        /// <summary>
        /// Analyzes the document.
        /// </summary>
        /// <param name="document">The document.</param>
        public override void AnalyzeDocument(CodeDocument document)
        {            
            CsDocument csDocument = (CsDocument)document;
            if (csDocument.RootElement != null && !csDocument.RootElement.Generated)
            {
                csDocument.WalkDocument<Object>(new CodeWalkerElementVisitor<Object>(this.ProcessElement), null, new CodeWalkerExpressionVisitor<Object>(this.ProcessExpression), null);
                //this.ProcessElement(csDocument.RootElement);                
            }
        }

        private Boolean ProcessExpression(Expression expression, Expression parentExpression, Statement parentStatement, CsElement parentElement, Object context)
        {
            //if (!parentElement.Generated && this.IsLocalElementType(parentElement.ElementType) 
            //    /*&& parentStatement.StatementType == StatementType.Expression*/
            //    && ((expression.ExpressionType == ExpressionType.Literal && parentStatement.StatementType == StatementType.Return)
            //        || (expression.ExpressionType == ExpressionType.Literal && parentStatement.StatementType == StatementType.Yield))
            //    && !parentElement.Declaration.ContainsModifier(new CsTokenType[] { CsTokenType.Static, CsTokenType.Const }))
            //{                
            //    //if (expression.ExpressionType == ExpressionType.MemberAccess || expression.ExpressionType == ExpressionType.MethodInvocation)
            //    if (parentExpression == null || parentExpression.ExpressionType != ExpressionType.MemberAccess)
            //    {
            //        for (Node<CsToken> node = parentElement.Tokens.First; node != null; node = node.Next)
            //        {
            //            if (node.Value.Text != "this")
            //            {
            //                this.AddViolation(parentElement, Rules.UseClassNameOrThis, parentElement.Declaration.Name);
            //            }
            //        }

            //        /*foreach (CsToken token in expression.Tokens)
            //        {
            //            if (token.CsTokenType != CsTokenType.This)
            //            {
            //                this.AddViolation(parentElement, Rules.UseClassNameOrThis, parentElement.Declaration.Name);
            //            }
            //        }*/
            //    }
            //}
            this.CheckDoNotUseLinqAliases(parentElement, expression);
            //this.CheckUseClassNameOrThis(parentElement, expression);

            return true;
        }

        /// <summary>
        /// Processes the C# element.
        /// </summary>
        /// <param name="element">The C# element.</param>
        /// <param name="parentElement">The parent element.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        ///  <c>True</c> if element was processed; otherwise, <c>false</c>.
        /// </returns>
        private Boolean ProcessElement(CsElement element, CsElement parentElement, Object context)
        {
            if (this.Cancel)
            {
                return false;
            }                        
            
            this.CheckInstanceVariablesUnderscorePrefix(element);
            this.CheckMinimumFieldLength(element);

            foreach (CsElement child in element.ChildElements)
            {
                if (!this.ProcessElement(child, parentElement, context))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks the rule: instance variables should has underscore prefix.
        /// </summary>
        /// <param name="element">The C# element.</param>
        private void CheckInstanceVariablesUnderscorePrefix(CsElement element)
        {
            String elementName = element.Declaration.Name;
            if (!element.Generated
                && element.ElementType == ElementType.Field
                && this.IsPrivateAccess(element.AccessModifier)
                && (!elementName.StartsWith("_", StringComparison.Ordinal)
                    || (elementName.Length >= 2 && elementName.Substring(1, 1).ToLower() != elementName.Substring(1, 1))))
            {
                this.AddViolation(element, Rules.InstanceVariablesUnderscorePrefix, elementName);
            }
        }

        /// <summary>
        /// Determines whether is private access.
        /// </summary>
        /// <param name="type"> The access modifier type. See <see cref="AccessModifierType"/> for help.</param>
        /// <returns>
        /// <c>True</c> if is private access; otherwise, <c>false</c>.
        /// </returns>
        private Boolean IsPrivateAccess(AccessModifierType type)
        {
            return (type == AccessModifierType.Private || type == AccessModifierType.Protected || type == AccessModifierType.ProtectedInternal);
        }

        /// <summary>
        /// Checks the rule: use class name or "this".
        /// </summary>
        /// <param name="element">The C# element.</param>
        /// <param name="expression">The expression.</param>
        private void CheckUseClassNameOrThis(CsElement element, Expression expression)
        {
            if (!element.Generated 
                && (expression.ExpressionType == ExpressionType.MemberAccess || expression.ExpressionType == ExpressionType.MethodInvocation) 
                && expression.Tokens.First.Value.CsTokenType != CsTokenType.This)
            {
                this.AddViolation(element, Rules.UseClassNameOrThis, element.Declaration.Name);
            }
            //if (!element.Generated && this.IsLocalElementType(element.ElementType)
            //    && !element.Declaration.ContainsModifier(new CsTokenType[] { CsTokenType.Static, CsTokenType.Const }))
            //{
            //    //Expression expression = element.FindParentExpression();
            //    //if (expression != null && expression.ExpressionType == ExpressionType.MemberAccess || expression.ExpressionType == ExpressionType.MethodInvocation)
            //    //{
            //        if (element.Declaration.Tokens.First.Value.Text != "this")
            //        {
            //            this.AddViolation(element, Rules.UseClassNameOrThis, element.Declaration.Name);
            //        }
            //        /*for (Node<CsToken> node = element.Tokens.First; node != null; node = node.Next)
            //        {
            //            if (node.Value.Text != "this")
            //            {
            //                this.AddViolation(element, Rules.UseClassNameOrThis, element.Declaration.Name);
            //            }
            //        }*/
            //    //}
            //}

            //if (!element.Generated)
            //{
            //    Expression expression = element.FindParentExpression();
            //    if (expression != null && expression.ExpressionType == ExpressionType.MemberAccess || expression.ExpressionType == ExpressionType.MethodInvocation)
            //    {
            //        foreach (CsToken token in expression.Tokens)
            //        {
            //            if (token.CsTokenType != CsTokenType.This)
            //            {
            //                this.AddViolation(element, Rules.UseClassNameOrThis, element.Declaration.Name);
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Determines whether is local element type of specified element.
        /// </summary>
        /// <param name="type">The element type. See <see cref="ElementType"/> for help.</param>
        /// <returns>
        /// <c>True</c> if is local element type; otherwise, <c>false</c>.
        /// </returns>
        private Boolean IsLocalElementType(ElementType type)
        {
            return (type == ElementType.Delegate || type == ElementType.Event || type == ElementType.Field || type == ElementType.Method || type == ElementType.Property);
        }

        /// <summary>
        /// Checks the rule: Don't use linq aliases.
        /// </summary>
        /// <param name="element">The C# element.</param>
        /// <param name="expression">The expression.</param>
        private void CheckDoNotUseLinqAliases(CsElement element, Expression expression)
        {
            if (!element.Generated)
            {
                List<CsTokenType> linqAliases = new List<CsTokenType>()
                {
                    CsTokenType.Select, CsTokenType.From, CsTokenType.Ascending, 
                    CsTokenType.Descending, CsTokenType.In, CsTokenType.Into, CsTokenType.Join,
                    CsTokenType.Let, CsTokenType.OrderBy, CsTokenType.Where
                };

                foreach (CsToken token in expression.Tokens)
                {
                    if (linqAliases.Contains(token.CsTokenType))
                    {
                        this.AddViolation(element, Rules.DoNotUseLinqAliases);
                    }
                }
            }
        }
        
        /// <summary>
        /// Checks the rule: minimum fiels length should be 3 symbols.
        /// </summary>
        /// <param name="element">The C# element.</param>
        private void CheckMinimumFieldLength(CsElement element)
        {
            const Int32 minAllowedLength = 2;
            
            if (!element.Generated 
                && this.IsSupportedForMinLengthElementType(element.ElementType) 
                && element.Declaration.Name.Length < minAllowedLength)
            {
                this.AddViolation(element, Rules.MinimumFieldLength, element.Declaration.Name);
            }
        }

        /// <summary>
        /// Determines whether is supported for minimum length for name.
        /// </summary>
        /// <param name="type">The element type. See <see cref="ElementType"/> for help.</param>
        /// <returns>
        /// <c>True</c> if is supported; otherwise, <c>false</c>.
        /// </returns>
        private Boolean IsSupportedForMinLengthElementType(ElementType type)
        {
            return (type == ElementType.Class || type == ElementType.Delegate || type == ElementType.Enum 
                || type == ElementType.Event || type == ElementType.Field || type == ElementType.Interface || type == ElementType.Method
                || type == ElementType.Namespace || type == ElementType.Property || type == ElementType.Struct);
        }
    }
}
