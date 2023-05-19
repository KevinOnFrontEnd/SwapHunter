# swaphunter
a PoC console application for snooping new xch tokens listed on TibetSwap.io

With the chia ecosystem ramping up now that there is a AMM (TibetSwap), there will be an influx in tokens that are listed on the exchange. This console application is intended to help users find new tokens available to trade as early as possible. 

# Setup instructions
- Install Chia gui client https://www.chia.net/downloads/
- Install dotnet core SDK/RunTime 7.0 https://dotnet.microsoft.com/en-us/download/dotnet/7.0
- Before running the application - chia must be running in order to communicate with chia wallet RPC to generate offers.

This application requires access to the wallet certificate & key in order to use the chia wallet RPC to generate offer files. The following application settings (appsettings.json) need to be filled in before this application will function.

```json
"Wallet_key_path": "~/.chia/mainnet/config/ssl/wallet/private_wallet.key OR c:\Users\<YOURNAME>\chia\mainnet\config\ssl\wallet\private_wallet.key",
"Wallet_cert_path": "~/.chia/mainnet/config/ssl/wallet/private_wallet.crt  OR c:\Users\<YOURNAME>\chia\mainnet\config\ssl\wallet\private_wallet.crt"
 ```

# Contributing
Please feel free to contribute.

# TODO #
- Logic that posts/gets offer data from TibetSwap api
- Generate token offer using chia wallet make_offer (WIP using chia wallet RPC as of 19/05/2023)

# Disclaimer
I take no responsibility for the use or misuse of any products, services, or information provided. It is the sole responsibility of the user to exercise their judgment and discretion when utilizing these resources. I do not accept liability for any damages, losses, or adverse consequences resulting from the use, interpretation, or reliance upon the materials provided. Users are encouraged to seek professional advice and exercise caution when applying the information or engaging in any activities related this program.
