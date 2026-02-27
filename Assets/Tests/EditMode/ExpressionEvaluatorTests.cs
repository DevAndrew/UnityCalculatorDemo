using DevAndrew.Calculator.Core.Logic;
using NUnit.Framework;

public class ExpressionEvaluatorTests
{
    [TestCase("54+21", 75)]
    [TestCase("45+00", 45)]
    [TestCase("123", 123)]
    [TestCase("1+2+3", 6)]
    public void TryEvaluate_ReturnsTrue_ForValidExpressions(string expression, long expected)
    {
        var isValid = ExpressionEvaluator.TryEvaluate(expression, out var sum);

        Assert.IsTrue(isValid);
        Assert.AreEqual(expected, sum);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("+1")]
    [TestCase("1+")]
    [TestCase("1++2")]
    [TestCase("5+6-")]
    [TestCase("98.12+48.1")]
    [TestCase("45+-88")]
    [TestCase("1 + 2")]
    public void TryEvaluate_ReturnsFalse_ForInvalidExpressions(string expression)
    {
        var isValid = ExpressionEvaluator.TryEvaluate(expression, out _);

        Assert.IsFalse(isValid);
    }

    [Test]
    public void TryEvaluate_ReturnsFalse_OnOverflow()
    {
        const string expression = "9223372036854775807+1";

        var isValid = ExpressionEvaluator.TryEvaluate(expression, out _);

        Assert.IsFalse(isValid);
    }
}
