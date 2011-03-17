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

                this.CheckClassMemberRulesForElements(csDocument.RootElement, null, null);

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

        #region UseThisPrefix

        /// <summary>
        /// Checks the items within the given element.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <param name="parentClass">The class that the element belongs to.</param>
        /// <param name="members">The collection of members of the parent class.</param>
        /// <returns>Returns false if the analyzer should quit.</returns>
        private Boolean CheckClassMemberRulesForElements(CsElement element, ClassBase parentClass, Dictionary<String, List<CsElement>> members)
        {
            // Check whether processing has been cancelled by the user.
            if (this.Cancel)
            {
                return false;
            }

            foreach (CsElement child in element.ChildElements)
            {
                if (!child.Generated)
                {
                    if (child.ElementType == ElementType.Method
                        /* || child.ElementType == ElementType.Constructor 
                         || child.ElementType == ElementType.Destructor */
                        || child.ElementType == ElementType.Accessor)
                    {
                        // If the parent class is null, then this element is sitting outside of a class.
                        // This is illegal in C# so the code will not compile, but we still attempt to
                        // parse it. In this case there is no use of this prefixes since there is no class.
                        if (parentClass != null)
                        {
                            this.CheckClassMemberRulesForStatements(child.ChildStatements, child, parentClass, members);
                        }
                    }
                    else
                    {
                        if (child.ElementType == ElementType.Class || child.ElementType == ElementType.Struct)
                        {
                            ClassBase elementContainer = child as ClassBase;

                            this.CheckClassMemberRulesForElements(child, elementContainer, members);
                        }
                        else if (!this.CheckClassMemberRulesForElements(child, parentClass, members))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Parses the given statement list.
        /// </summary>
        /// <param name="statements">The list of statements to parse.</param>
        /// <param name="parentElement">The element that contains the statements.</param>
        /// <param name="parentClass">The class that the element belongs to.</param>
        /// <param name="members">The collection of members of the parent class.</param>
        private void CheckClassMemberRulesForStatements(
            ICollection<Statement> statements,
            CsElement parentElement,
            ClassBase parentClass,
            Dictionary<String, List<CsElement>> members)
        {
            // Loop through each of the statements.
            foreach (Statement statement in statements)
            {
                if (statement.ChildStatements.Count > 0)
                {
                    // Parse the sub-statements.
                    this.CheckClassMemberRulesForStatements(statement.ChildStatements, parentElement, parentClass, members);
                }

                // Parse the expressions in the statement.
                this.CheckClassMemberRulesForExpressions(statement.ChildExpressions, null, parentElement, parentClass, members);
            }
        }

        /// <summary>
        /// Parses the list of expressions.
        /// </summary>
        /// <param name="expressions">The list of expressions.</param>
        /// <param name="parentExpression">The parent expression, if there is one.</param>
        /// <param name="parentElement">The element that contains the expressions.</param>
        /// <param name="parentClass">The class that the element belongs to.</param>
        /// <param name="members">The collection of members of the parent class.</param>
        private void CheckClassMemberRulesForExpressions(
            ICollection<Expression> expressions,
            Expression parentExpression,
            CsElement parentElement,
            ClassBase parentClass,
            Dictionary<String, List<CsElement>> members)
        {
            // Loop through each of the expressions in the list.
            foreach (Expression expression in expressions)
            {
                // If the expression is a variable declarator expression, we don't 
                // want to match against the identifier tokens.
                if (expression.ExpressionType == ExpressionType.VariableDeclarator)
                {
                    VariableDeclaratorExpression declarator = expression as VariableDeclaratorExpression;
                    if (declarator.Initializer != null)
                    {
                        this.CheckClassMemberRulesForExpression(declarator.Initializer, parentExpression, parentElement, parentClass, members);
                    }
                }
                else
                {
                    this.CheckClassMemberRulesForExpression(expression, parentExpression, parentElement, parentClass, members);
                }
            }
        }

        /// <summary>
        /// Gets the non-whitespace token that appears before the given token.
        /// </summary>
        /// <param name="tokenNode">The token node.</param>
        /// <param name="tokenList">The list that contains the token.</param>
        /// <returns>Returns the previous token.</returns>
        private CsToken GetPreviousToken(Node<CsToken> tokenNode, MasterList<CsToken> tokenList)
        {
            foreach (CsToken token in tokenList.ReverseIterator(tokenNode))
            {
                if (token.CsTokenType != CsTokenType.EndOfLine
                    && token.CsTokenType != CsTokenType.WhiteSpace)
                {
                    return token;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the given token is preceded by a member access symbol.
        /// </summary>
        /// <param name="literalTokenNode">The token to check.</param>
        /// <param name="masterList">The list containing the token.</param>
        /// <returns>Returns true if the token is preceded by a member access symbol.</returns>
        private Boolean IsLiteralTokenPrecededByMemberAccessSymbol(Node<CsToken> literalTokenNode, MasterList<CsToken> masterList)
        {
            // Get the previous non-whitespace token.
            CsToken previousToken = this.GetPreviousToken(literalTokenNode.Previous, masterList);
            if (previousToken == null)
            {
                return false;
            }

            if (previousToken.CsTokenType == CsTokenType.OperatorSymbol)
            {
                OperatorSymbol symbol = (OperatorSymbol)previousToken;

                if (symbol.SymbolType == OperatorType.MemberAccess
                    || symbol.SymbolType == OperatorType.Pointer
                    || symbol.SymbolType == OperatorType.QualifiedAlias)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a matching local variable is contained in the given variable list.
        /// </summary>
        /// <param name="variables">The variable list.</param>
        /// <param name="word">The variable name to check.</param>
        /// <param name="item">The token containing the variable name.</param>
        /// <returns>Returns true if there is a matching local variable.</returns>
        private Boolean ContainsVariable(VariableCollection variables, String word, CsToken item)
        {
            Variable variable = variables[word];
            if (variable != null)
            {
                // Make sure the variable appears before the word.
                if (variable.Location.LineNumber < item.LineNumber)
                {
                    return true;
                }
                else if (variable.Location.LineNumber == item.LineNumber)
                {
                    if (variable.Location.StartPoint.IndexOnLine < item.Location.StartPoint.IndexOnLine)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the given word is the name of a local variable.
        /// </summary>
        /// <param name="word">The name to check.</param>
        /// <param name="item">The token containing the word.</param>
        /// <param name="parent">The code unit that the word appears in.</param>
        /// <returns>True if the word is the name of a local variable, false if not.</returns>
        private Boolean IsLocalMember(String word, CsToken item, ICodeUnit parent)
        {
            while (parent != null)
            {
                // Check to see if the name matches a local variable.
                if (this.ContainsVariable(parent.Variables, word, item))
                {
                    return true;
                }

                // If the parent is an element, do not look any higher up the stack than this.
                if (parent.CodePartType == CodePartType.Element)
                {
                    break;
                }

                // Check to see whether the variable is defined within the parent.
                parent = parent.Parent as ICodeUnit;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the given expression is the left-hand-side literal in any of the assignment expressions
        /// within an object initialize expression.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>Returns true if the expression is the left-hand-side literal in any of the assignment expressions 
        /// within an object initializer expression.</returns>
        /// <remarks>This method checks for the following situation:
        /// class MyClass
        /// {
        ///     public Boolean Member { get { return true; } }
        ///     public void SomeMethod()
        ///     {
        ///         MyObjectType someObject = new MyObjectType { Member = this.Member }; 
        ///     }
        /// }
        /// In this case, StyleCop will raise a violation since it looks like the Member token should be prefixed by 'this.', however,
        /// it is actually referring to a property on the MyObjectType type.
        /// </remarks>
        private Boolean IsObjectInitializerLeftHandSideExpression(Expression expression)
        {
            // The expression should be a literal expression if it represents the keyword being checked.
            if (expression.ExpressionType == ExpressionType.Literal)
            {
                // The literal should be a child-expression of an assignment expression.
                AssignmentExpression assignmentExpression = expression.Parent as AssignmentExpression;
                if (assignmentExpression != null)
                {
                    // The left-hand-side of the assignment expression should be the literal expression.
                    if (assignmentExpression.LeftHandSide == expression)
                    {
                        // The assignment expression should be the child of an object initializer expression.
                        ObjectInitializerExpression objectInitializeExpression = assignmentExpression.Parent as ObjectInitializerExpression;
                        if (objectInitializeExpression != null)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Matches the given word with members of the given class.
        /// </summary>
        /// <param name="word">The word to check.</param>
        /// <param name="members">The collection of members of the parent class.</param>
        /// <param name="interfaces">True if interface implementations should be included.</param>
        /// <returns>Returns the class members that matches against the given name.</returns>
        private ICollection<CsElement> MatchClassMember(String word, Dictionary<String, List<CsElement>> members, Boolean interfaces)
        {
            List<CsElement> matchesFound = null;

            // Look through all the children of this class to see if the word matches
            // against any item in the class.
            List<CsElement> matches = null;
            if (members.TryGetValue(word, out matches))
            {
                foreach (CsElement match in matches)
                {
                    // Check if there is a match.
                    if (match.ElementType == ElementType.Field
                       || match.Declaration.Name == word
                       || (interfaces && match.Declaration.Name.EndsWith("." + word, StringComparison.Ordinal)))
                    {
                        if (matchesFound == null)
                        {
                            matchesFound = new List<CsElement>();
                        }

                        matchesFound.Add(match);
                    }
                }
            }

            return matchesFound;
        }

        /// <summary>
        /// Finds the given class member in the given class.
        /// </summary>
        /// <param name="word">The word to check.</param>
        /// <param name="parentClass">The class the word appears in.</param>
        /// <param name="members">The collection of members of the parent class.</param>
        /// <param name="interfaces">True if interface implementations should be included.</param>
        /// <returns>Returns the class members that match against the given name.</returns>
        private ICollection<CsElement> FindClassMember(String word, ClassBase parentClass, Dictionary<String, List<CsElement>> members, Boolean interfaces)
        {
            // If the word is the same as the class name, then this is a constructor and we
            // don't want to match against it.
            if (word != parentClass.Declaration.Name)
            {
                ICollection<CsElement> matches = this.MatchClassMember(word, members, interfaces);
                if (matches != null && matches.Count > 0)
                {
                    return matches;
                }
            }

            return null;
        }

        /// <summary>
        /// Parses the given expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parentExpression">The parent expression, if there is one.</param>
        /// <param name="parentElement">The element that contains the expressions.</param>
        /// <param name="parentClass">The class that the element belongs to.</param>
        /// <param name="members">The collection of members of the parent class.</param>
        private void CheckClassMemberRulesForExpression(
            Expression expression,
            Expression parentExpression,
            CsElement parentElement,
            ClassBase parentClass,
            Dictionary<String, List<CsElement>> members)
        {
            if (expression.ExpressionType == ExpressionType.Literal)
            {
                LiteralExpression literalExpression = (LiteralExpression)expression;

                // Check to see whether this literal is preceded by a member access symbol. If not
                // then we want to check whether this is a reference to one of our class members.
                if (!this.IsLiteralTokenPrecededByMemberAccessSymbol(literalExpression.TokenNode, expression.Tokens.MasterList))
                {
                    // Process the literal.
                    this.CheckClassMemberRulesForLiteralToken(
                        literalExpression.TokenNode,
                        expression,
                        parentExpression,
                        parentElement,
                        parentClass,
                        members);
                }
            }
            else
            {
                if (expression.ExpressionType == ExpressionType.Assignment
                    && parentExpression != null
                    && parentExpression.ExpressionType == ExpressionType.CollectionInitializer)
                {
                    this.CheckClassMemberRulesForExpression(((AssignmentExpression)expression).RightHandSide, expression, parentElement, parentClass, members);
                }
                else if (expression.ChildExpressions.Count > 0)
                {
                    // Check each child expression within this expression.
                    this.CheckClassMemberRulesForExpressions(
                        expression.ChildExpressions,
                        expression,
                        parentElement,
                        parentClass,
                        members);
                }

                // Check if this is an anonymous method expression, which contains a child statement list.
                if (expression.ExpressionType == ExpressionType.AnonymousMethod)
                {
                    // Check the statements under this anonymous method.
                    this.CheckClassMemberRulesForStatements(
                        expression.ChildStatements,
                        parentElement,
                        parentClass,
                        members);
                }
                else if (expression.ExpressionType == ExpressionType.MethodInvocation)
                {
                    // Check each of the arguments passed into the method call.
                    MethodInvocationExpression methodInvocation = (MethodInvocationExpression)expression;
                    foreach (Argument argument in methodInvocation.Arguments)
                    {
                        // Check each expression within this child expression.
                        this.CheckClassMemberRulesForExpression(
                            argument.Expression,
                            null,
                            parentElement,
                            parentClass,
                            members);
                    }
                }
            }
        }

        /// <summary>
        /// Parses the given literal token.
        /// </summary>
        /// <param name="tokenNode">The literal token node.</param>
        /// <param name="expression">The expression that contains the token.</param>
        /// <param name="parentExpression">The parent of the expression that contains the token.</param>
        /// <param name="parentElement">The element that contains the expression.</param>
        /// <param name="parentClass">The class that the element belongs to.</param>
        /// <param name="members">The collection of members of the parent class.</param>
        private void CheckClassMemberRulesForLiteralToken(
            Node<CsToken> tokenNode,
            Expression expression,
            Expression parentExpression,
            CsElement parentElement,
            ClassBase parentClass,
            Dictionary<String, List<CsElement>> members)
        {
            // Skip types. We only care about named members.
            if (!(tokenNode.Value is TypeToken))
            {
                // If the name starts with a dot, ignore it.
                if (!tokenNode.Value.Text.StartsWith(".", StringComparison.Ordinal))
                {
                    if (tokenNode.Value.Text != "this")
                    {
                        // Check whether this word should really start with this.
                        this.CheckWordUsageAgainstClassMemberRules(
                            tokenNode.Value.Text,
                            tokenNode.Value,
                            tokenNode.Value.LineNumber,
                            expression,
                            parentElement,
                            parentClass,
                            members);
                    }
                }
            }
        }

        /// <summary>
        /// Checks a word to see if it should start with this. or base.
        /// </summary>
        /// <param name="word">The word text to check.</param>
        /// <param name="item">The word being checked.</param>
        /// <param name="line">The line that the word appears on.</param>
        /// <param name="expression">The expression the word appears within.</param>
        /// <param name="parentElement">The element that contains the word.</param>
        /// <param name="parentClass">The parent class that this element belongs to.</param>
        /// <param name="members">The collection of members of the parent class.</param>
        private void CheckWordUsageAgainstClassMemberRules(
            String word,
            CsToken item,
            Int32 line,
            Expression expression,
            CsElement parentElement,
            ClassBase parentClass,
            Dictionary<String, List<CsElement>> members)
        {
            // If there is a local variable with the same name, or if the item we're checking is within the left-hand side
            // of an object initializer expression, then ignore it.
            if (!this.IsLocalMember(word, item, expression) && !this.IsObjectInitializerLeftHandSideExpression(expression))
            {
                // Determine if this is a member of our class.
                CsElement foundMember = null;
                ICollection<CsElement> classMembers = this.FindClassMember(word, parentClass, members, false);
                if (classMembers != null)
                {
                    if (classMembers != null)
                    {
                        foreach (CsElement classMember in classMembers)
                        {
                            if (classMember.Declaration.ContainsModifier(CsTokenType.Static)
                                || (classMember.ElementType == ElementType.Field && ((Field)classMember).Const))
                            {
                                // There is a member with a matching name that is static or is a const field. In this case, 
                                // ignore the issue and quit.
                                foundMember = null;
                                break;
                            }
                            else if (classMember.ElementType != ElementType.Class
                                && classMember.ElementType != ElementType.Struct
                                && classMember.ElementType != ElementType.Delegate
                                && classMember.ElementType != ElementType.Enum)
                            {
                                // Found a matching member.
                                if (foundMember == null)
                                {
                                    foundMember = classMember;
                                }
                            }
                        }
                    }

                    if (foundMember != null)
                    {
                        if (foundMember.ElementType == ElementType.Property)
                        {
                            // If the property's name and type are the same, then this is not a violation.
                            // In this case, the type is being accessed, not the property.
                            Property property = (Property)foundMember;
                            if (property.ReturnType.Text != property.Declaration.Name)
                            {
                                this.AddViolation(parentElement, line, Rules.UseThisPrefix, word);
                            }
                        }
                        else
                        {
                            this.AddViolation(parentElement, line, Rules.UseThisPrefix, word);
                        }
                    }
                }
            }
        }

        #endregion UseThisPrefix
    }
}
