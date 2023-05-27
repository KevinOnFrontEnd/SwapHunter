using SwapHunter.Client.Chia;
using SwapHunter.Client.TibetSwap;

namespace SwapHunter;

public class OfferService : IOfferService
{
    private ITibetClient _tibetClient { get; set; }
    private IChiaRpcClient _chiaRpcClient { get; set; }
    
    public OfferService(ITibetClient tibetClient, IChiaRpcClient chiaClient)
    {
        _tibetClient = tibetClient;
        _chiaRpcClient = chiaClient;
    }
}