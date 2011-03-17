namespace StyleCopRules
{
    using System;
    using Microsoft.StyleCop;
    using Microsoft.StyleCop.CSharp;
    using System.Collections.Generic;

    /// <summary>
    /// Represents of Rus Wizards custom StyleCop rules.
    /// </summary>
    [SourceAnalyzer(typeof(CsParser))]
    public sealed class RusWizardsRules : SourceAnalyzer
    {
        #region Private Static Constants

        /// <summary>
        /// The built-in type aliases for C#.
        /// </summary>
        private readonly String[][] _builtInTypes = new String[][]
        {
            new String[] { "Boolean", "System.Boolean", "bool" },
            new String[] { "Object", "System.Object", "object" },
            new String[] { "String", "System.String", "string" },
            new String[] { "Int16", "System.Int16", "short" },
            new String[] { "UInt16", "System.UInt16", "ushort" },
            new String[] { "Int32", "System.Int32", "int" },
            new String[] { "UInt32", "System.UInt32", "uint" },
            new String[] { "Int64", "System.Int64", "long" },
            new String[] { "UInt64", "System.UInt64", "ulong" },
            new String[] { "Double", "System.Double", "double" },
            new String[] { "Single", "System.Single", "float" },
            new String[] { "Byte", "System.Byte", "byte" },
            new String[] { "SByte", "System.SByte", "sbyte" },
            new String[] { "Char", "System.Char", "char" },
            new String[] { "Decimal", "System.Decimal", "decimal" }
        };

        #endregion Private Static Constants

        /// <summary>
        /// Analyzes the document.
        /// </summary>
        /// <param name="document">The document.</param>
        public override void AnalyzeDocument(CodeDocument document)
        {
            CsDocument csDocument = (CsDocument)document;
            if (csDocument.RootElement != null && !csDocument.RootElement.Generated)
            {
                csDocument.WalkDocument<Object>(
                    new CodeWalkerElementVisitor<Object>(this.ProcessElement),
                    null,
                    new CodeWalkerExpressionVisitor<Object>(this.ProcessExpression),
                    null);                

                this.IterateTokenList(csDocument);
            }
        }

        #region DoNotUseLinqAliases

        /// <summary>
        /// Processes the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parentExpression">The parent expression.</param>
        /// <param name="parentStatement">The parent statement.</param>
        /// <param name="parentElement">The parent element.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// <c>True</c> if element was processed; otherwise, <c>false</c>.
        /// </returns>
        private Boolean ProcessExpression(Expression expression, Expression parentExpression, Statement parentStatement, CsElement parentElement, Object context)
        {
            this.CheckDoNotUseLinqAliases(parentElement, expression);

            return true;
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
                        this.AddViolation(element, token.LineNumber, Rules.DoNotUseLinqAliases);
                    }
                }
            }
        }

        #endregion DoNotUseLinqAliases

        #region InstanceVariablesUnderscorePrefix & Minimum length

        /// <summary>
        /// Processes the C# element.
        /// </summary>
        /// <param name="element">The C# element.</param>
        /// <param name="parentElement">The parent element.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// <c>True</c> if element was processed; otherwise, <c>false</c>.
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

        #endregion InstanceVariablesUnderscorePrefix & Minimum length

        #region DoNotUseBuiltInTypeAliases

        /// <summary>
        /// Checks the built-in types and empty strings within a document.
        /// </summary>
        /// <param name="document">The document containing the tokens.</param>
        private void IterateTokenList(CsDocument document)
        {
            for (Node<CsToken> tokenNode = document.Tokens.First; tokenNode != null; tokenNode = tokenNode.Next)
            {
                CsToken token = tokenNode.Value;

                if (token.CsTokenClass == CsTokenClass.Type || token.CsTokenClass == CsTokenClass.GenericType)
                {
                    // Check that the type is using the built-in types, if applicable.
                    this.CheckBuiltInType(tokenNode, document);
                }
            }
        }

        /// <summary>
        /// Checks a type to determine whether it should use one of the built-in types.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="document">The parent document.</param>
        private void CheckBuiltInType(Node<CsToken> type, CsDocument document)
        {
            TypeToken typeToken = (TypeToken)type.Value;

            if (typeToken.CsTokenClass != CsTokenClass.GenericType)
            {
                for (Int32 i = 0; i < this._builtInTypes.Length; i++)
                {
                    String[] builtInType = this._builtInTypes[i];

                    if (CsTokenList.MatchTokens(typeToken.ChildTokens.First, builtInType[2]))
                    {
                        Boolean isEnumType = false;

                        // If the previous token is an equals sign, then this is a using alias directive.                         
                        // For example: using SomeAlias = System.String;
                        Boolean isUsingAliasDirective = false;
                        for (Node<CsToken> previous = type.Previous; previous != null; previous = previous.Previous)
                        {
                            if (previous.Value.CsTokenType != CsTokenType.EndOfLine
                                && previous.Value.CsTokenType != CsTokenType.MultiLineComment
                                && previous.Value.CsTokenType != CsTokenType.SingleLineComment
                                && previous.Value.CsTokenType != CsTokenType.WhiteSpace)
                            {
                                if (previous.Value.Text == "=")
                                {
                                    isUsingAliasDirective = true;
                                }

                                // Check on the enum.
                                if (previous.Value.Text == ":")
                                {
                                    isEnumType = true;
                                }

                                break;
                            }
                        }

                        if (!isUsingAliasDirective && !isEnumType)
                        {
                            this.AddViolation(
                                typeToken.FindParentElement(),
                                typeToken.LineNumber,
                                Rules.DoNotUseBuiltInTypeAliases,
                                builtInType[0],
                                builtInType[1],
                                builtInType[2]);
                        }

                        break;
                    }
                }
            }

            for (Node<CsToken> childToken = typeToken.ChildTokens.First; childToken != null; childToken = childToken.Next)
            {
                if (childToken.Value.CsTokenClass == CsTokenClass.Type
                    || childToken.Value.CsTokenClass == CsTokenClass.GenericType)
                {
                    this.CheckBuiltInType(childToken, document);
                }
            }
        }

        #endregion DoNotUseBuiltInTypeAliases        
    }
}