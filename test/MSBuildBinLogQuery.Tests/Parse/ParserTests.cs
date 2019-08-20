using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Build.Logging.Query.Ast;
using Microsoft.Build.Logging.Query.Parse;
using Xunit;

namespace Microsoft.Build.Logging.Query.Tests.Parse
{
    public class ParserTests
    {
        public static IEnumerable<object[]> GenerateDataForTestParsedAst()
        {
            yield return new object[]
            {
                "/message",
                new MessageNode(LogNodeType.Direct)
            };

            yield return new object[]
            {
                "/warning",
                new WarningNode(LogNodeType.Direct)
            };

            yield return new object[]
            {
                "/error",
                new ErrorNode(LogNodeType.Direct)
            };

            yield return new object[]
            {
                "/task",
                new TaskNode(null)
            };

            yield return new object[]
            {
                "/target",
                new TargetNode(null)
            };

            yield return new object[]
            {
                "/project",
                new ProjectNode((AstNode)null)
            };

            yield return new object[]
            {
                "/task/message",
                new TaskNode(new MessageNode(LogNodeType.Direct))
            };

            yield return new object[]
            {
                "/target/warning",
                new TargetNode(new WarningNode(LogNodeType.Direct))
            };

            yield return new object[]
            {
                "/project/error",
                new ProjectNode(new ErrorNode(LogNodeType.Direct))
            };

            yield return new object[]
            {
                "/target/task/message",
                new TargetNode(new TaskNode(new MessageNode(LogNodeType.Direct)))
            };

            yield return new object[]
            {
                "/project/task/warning",
                new ProjectNode(new TaskNode(new WarningNode(LogNodeType.Direct)))
            };

            yield return new object[]
            {
                "/project/target/task/error",
                new ProjectNode(new TargetNode(new TaskNode(new ErrorNode(LogNodeType.Direct))))
            };

            yield return new object[]
            {
                "//message",
                new MessageNode(LogNodeType.All)
            };

            yield return new object[]
            {
                "/project//warning",
                new ProjectNode(new MessageNode(LogNodeType.All))
            };

            yield return new object[]
            {
                "/target/task//error",
                new TargetNode(new TaskNode(new MessageNode(LogNodeType.All)))
            };

            yield return new object[]
            {
                "/project/task//message",
                new ProjectNode(new TaskNode(new MessageNode(LogNodeType.All)))
            };

            yield return new object[]
            {
                "/Task[]",
                new TaskNode(null)
            };

            yield return new object[]
            {
                "/Task[ID=341]/Message",
                new TaskNode(
                    new MessageNode(LogNodeType.Direct),
                    new List<ConstraintNode> { new IdNode(341) })
            };

            yield return new object[]
            {
                "/Project/Task[id=1, Id=2, ID=3]//Warning",
                new ProjectNode(new TaskNode(
                    new MessageNode(LogNodeType.All),
                    new List<ConstraintNode> { new IdNode(1), new IdNode(2), new IdNode(3) }))
            };

            yield return new object[]
            {
                "/Target[]",
                new TargetNode(null)
            };

            yield return new object[]
            {
                "/Target[Id=153]",
                new TargetNode(
                    null,
                    new List<ConstraintNode> { new IdNode(153) })
            };

            yield return new object[]
            {
                "/Project/Target[Id=980321]//Error",
                new ProjectNode(
                    new TargetNode(
                        new ErrorNode(LogNodeType.All),
                        new List<ConstraintNode> { new IdNode(980321) }))
            };

            yield return new object[]
            {
                "/Target[Id=9]/Task[ID=81]/Warning",
                new TargetNode(
                    new TaskNode(
                        new WarningNode(LogNodeType.Direct),
                        new List<ConstraintNode> { new IdNode(81) }),
                    new List<ConstraintNode> { new IdNode(9) })
            };

            yield return new object[]
            {
                "/Project[]",
                new ProjectNode(null as TargetNode)
            };

            yield return new object[]
            {
                "/Project[Id=536]",
                new ProjectNode(
                    null as TargetNode,
                    new List<ConstraintNode> { new IdNode(536) })
            };

            yield return new object[]
            {
                "/Project[ID=448]/Error",
                new ProjectNode(
                    new MessageNode(LogNodeType.Direct),
                    new List<ConstraintNode> { new IdNode(448) })
            };

            yield return new object[]
            {
                "/Project[Id=121]/Task[Id=421]",
                new ProjectNode(
                    new TargetNode(
                        new TaskNode(
                            null,
                            new List<ConstraintNode> { new IdNode(421) })),
                    new List<ConstraintNode> { new IdNode(121) })
            };

            yield return new object[]
            {
                "/Project[Id=1]/Target[Id=2]/Task[Id=3]//Message",
                new ProjectNode(
                    new TargetNode(
                        new TaskNode(
                            new MessageNode(LogNodeType.All),
                            new List<ConstraintNode> { new IdNode(3) }),
                        new List<ConstraintNode> { new IdNode(2) }),
                    new List<ConstraintNode> { new IdNode(1) })
            };
        }

        [Theory]
        [MemberData(nameof(GenerateDataForTestParsedAst))]
        public void TestParsedAst(string expression, AstNode expectedAst)
        {
            var actualAst = Parser.Parse(expression);
            actualAst.Should().Be(expectedAst);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/")]
        [InlineData("//")]
        [InlineData("message")]
        [InlineData("project/message")]
        [InlineData("/warning/")]
        [InlineData("/message/project")]
        [InlineData("/task/task")]
        [InlineData("/project/target/target/task/error")]
        [InlineData("/project//target")]
        [InlineData("//message/target")]
        [InlineData("/project/target/task//warning/task")]
        [InlineData("/message/message")]
        [InlineData("/warning//error")]
        [InlineData("//error/message")]
        [InlineData("//warning//message")]
        [InlineData("/Task[ID=\"123\"]")]
        [InlineData("/Task[ID==123]")]
        [InlineData("/Task[ID]")]
        [InlineData("/Task[ID=123")]
        [InlineData("/Target[Id=\"999\"]/Task")]
        [InlineData("/Target[Id,Id=123]")]
        [InlineData("/Project[[Id=1]]")]
        public void TestParsedAstException(string expression)
        {
            Action action = () =>
            {
                var _ = Parser.Parse(expression);
            };

            action.Should().Throw<ParseException>(expression);
        }
    }
}