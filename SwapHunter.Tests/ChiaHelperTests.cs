using Xunit;

namespace SwapHunter.Tests;

public class ChiaHelperTests : SwapHunterTestBase
{
    public ChiaHelperTests(SwapHunterFixture fixture) : base(fixture)
    {
    }

    [Theory]
    [InlineData(0.000000000001,1)] // Positive smallest mojo
    [InlineData(0.0000000001,100)] // Positive dust
    [InlineData(0.001,1000000000)] // Positive dust
    [InlineData(0.1,100000000000)] // Positive small amount
    [InlineData(1.555555,1555555000000)] // 
    [InlineData(100000,100000000000000000)] // Positive super large amount
    [InlineData(-0.000000000001,-1)] // Negative smallest mojo
    [InlineData(-0.0000000001,-100)] // Negative dust
    [InlineData(-0.001,-1000000000)] // Negative dust
    [InlineData(-0.1,-100000000000)] // Negative small amount
    [InlineData(-1.555555,-1555555000000)] // 
    [InlineData(-100000,-100000000000000000)] //Negative super large amount
    
    public void converting_chia_mojos_returns_correct_value(double xch_amount, double mojos)
    {
        // arrange

        // act
        var xch_mojos = ChiaHelper.ConvertToMojos(xch_amount);
        
        // assert
        Assert.Equal(mojos, xch_mojos);
    }
}