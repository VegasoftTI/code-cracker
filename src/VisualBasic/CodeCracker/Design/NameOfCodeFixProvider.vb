﻿Imports System.Collections.Immutable
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Formatting
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Design
    <ExportCodeFixProvider("CodeCrackerNameOfCodeFixProvider", LanguageNames.VisualBasic), Composition.Shared>
    Public Class NameOfCodeFixProvider
        Inherits CodeFixProvider

        Public Overrides NotOverridable ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String) = ImmutableArray.Create(NameOfAnalyzer.Id)

        Public Overrides Function GetFixAllProvider() As FixAllProvider
            Return WellKnownFixAllProviders.BatchFixer
        End Function

        Public Overrides Async Function RegisterCodeFixesAsync(context As CodeFixContext) As Task
            Dim root = Await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(False)
            Dim diagnostic = context.Diagnostics.First
            Dim diagnosticspan = diagnostic.Location.SourceSpan
            Dim stringLiteral = root.FindToken(diagnosticspan.Start).Parent.AncestorsAndSelf.OfType(Of LiteralExpressionSyntax).FirstOrDefault
            If stringLiteral IsNot Nothing Then
                context.RegisterCodeFix(CodeAction.Create("use NameOf()", Function(c) MakeNameOfAsync(context.Document, stringLiteral, c)), diagnostic)
            End If
        End Function

        Private Async Function MakeNameOfAsync(document As Document, stringLiteral As LiteralExpressionSyntax, cancellationToken As CancellationToken) As Task(Of Document)
            Dim newNameof = SyntaxFactory.ParseExpression(String.Format("NameOf({0})", stringLiteral.Token.ValueText)).
            WithLeadingTrivia(stringLiteral.GetLeadingTrivia).
            WithTrailingTrivia(stringLiteral.GetTrailingTrivia).
            WithAdditionalAnnotations(Formatter.Annotation)
            Dim root = Await document.GetSyntaxRootAsync(cancellationToken)
            Dim newRoot = root.ReplaceNode(stringLiteral, newNameof)
            Return document.WithSyntaxRoot(newRoot)
        End Function
    End Class
End Namespace