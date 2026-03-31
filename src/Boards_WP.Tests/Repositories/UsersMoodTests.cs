namespace Boards_WP.Tests.Repositories;

public class UsersMoodTests
{
    [Theory]
    [InlineData(24)]
    [InlineData(40)]
    [InlineData(7)]
    public void GetDefaultDistribution_AlwaysSumsTo10000(int categoryCount)
    {
        var repo = new UsersMoodRepository("Server=dummy;Database=dummy;");

        var distribution = repo.GetDefaultDistribution(categoryCount);

        int totalSum = distribution.Values.Sum();
        Assert.Equal(10000, totalSum);
        Assert.Equal(categoryCount, distribution.Count);
        int min = distribution.Values.Min();
        int max = distribution.Values.Max();
        Assert.True(max - min <= 1, "The distribution is not uniform!");
    }
}
