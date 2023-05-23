using SwapHunter.Client;
using Xunit;

namespace SwapHunter.Tests;

public class TibetClientTests : SwapHunterTestBase
{
    //Get Quote
    //Post offer
    //Get Token Pairs
    public TibetClientTests(SwapHunterFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task calling_quote_endpoint_returns_ok()
    {
        // arrange
        var dbx_txch_pair = "3f2b9ddb0d90b81f43d86870d31b505df5dd377f4215928a436bc51d9d613294";

        // act
        var quote = await TibetClient.GetQuote(dbx_txch_pair, "1000");
        
        // assert
        Assert.NotNull(quote);
    }
    
    [Fact]
    public async Task calling_tokens_endpoint_returns_ok()
    {
        // arrange
        
        // act
        var pairs = await TibetClient.GetTokenPairs();
        
        // assert
        Assert.NotNull(pairs);
    }
}