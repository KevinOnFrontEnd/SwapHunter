using SwapHunter.Client;
using SwapHunter.Tests.Core;
using Xunit;

namespace SwapHunter.Tests.TibetClientTests;

public class TibetClientTests : SwapHunterTestBase
{
    public TibetClientTests(SwapHunterFixture baseTest):base(baseTest)
    {
    }

    [Fact]
    public async Task calling_router_endpoint_returns_ok()
    {
        // arrange

        // act
        var router = await TibetClient.GetRouter();
        
        // assert
        Assert.NotNull(router);
        Assert.Equal("testnet10",router.Network);
        Assert.Equal("d037e35cc7269df10e45d0d152d2ff53d26f318adf5ea20578e5cfb80b5b2a71",router.Launcher_Id);
        Assert.Equal("2223a2a9c037ae5844f7fbf2ecfa16740e31bd2d3ffe2f4a76006e266947776f",router.Current_Id);
    }
    
    [Fact]
    public async Task calling_quote_endpoint_returns_ok()
    {
        // arrange
        var dbx_txch_pair = "3f2b9ddb0d90b81f43d86870d31b505df5dd377f4215928a436bc51d9d613294";

        // act
        var quote = await TibetClient.GetQuote(dbx_txch_pair, 1000);
        
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
    
    [Fact]
    public async Task calling_gettoken_endpoint_returns_ok()
    {
        // arrange
        var tokens = await TibetClient.GetTokenPairs();
        var dbx = tokens.FirstOrDefault(x => x.short_name == "TDBX");
        
        // act
        var token = await TibetClient.GetToken(dbx.Asset_id);
        
        // assert
        Assert.NotNull(token);
        Assert.Equal(dbx.Asset_id, token.Asset_id);
        Assert.Equal(dbx.pair_id, token.pair_id);
        Assert.Equal(dbx.name,token.name);
        Assert.Equal(dbx.short_name,token.short_name);
        Assert.Equal(dbx.verified,token.verified);
    }
}