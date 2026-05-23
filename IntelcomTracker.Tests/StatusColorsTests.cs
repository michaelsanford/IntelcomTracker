using IntelcomTracker.Ui;
using Xunit;

namespace IntelcomTracker.Tests;

public class StatusColorsTests
{
    [Theory]
    [InlineData(0,   "grey")]
    [InlineData(105, "blue")]
    [InlineData(108, "cyan1")]
    [InlineData(300, "yellow")]
    [InlineData(400, "bright_green")]
    [InlineData(499, "bright_green")]  // boundary: last code in 400-499 band
    [InlineData(500, "red")]
    [InlineData(999, "red")]
    [InlineData(1,   "white")]         // unknown code below 105
    [InlineData(200, "white")]         // unknown code between known values
    public void GetColor_KnownStatusCode_ReturnsExpectedColor(int code, string expected)
        => Assert.Equal(expected, StatusColors.GetColor(code));

    [Fact]
    public void Colorize_WrapsTextInMarkupTags()
    {
        var result = StatusColors.Colorize("On our way!", 300);
        Assert.StartsWith("[yellow]", result);
        Assert.EndsWith("[/]", result);
        Assert.Contains("On our way!", result);
    }

    [Fact]
    public void Colorize_EscapesBrackets()
    {
        // Markup-sensitive characters in text must be escaped so they don't break rendering
        var result = StatusColors.Colorize("Status [update]", 0);
        Assert.Contains("[[update]]", result);
    }
}
