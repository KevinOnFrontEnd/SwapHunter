# SwapHunter
A Proof of concept console application for sniping new $xch tokens listed on TibetSwap.io v2

With the chia ecosystem ramping up now that there is a AMM (TibetSwap), there will be an influx in tokens that are listed on the exchange. This console application is intended to help users find new tokens available to trade as early as possible. 

This program will do the following when configured
TBD

# Setup instructions
**It is highly recommended that you run this project on chia testnet before running it on mainnet!**

- Install the latest Chia  https://www.chia.net/downloads/
- Install dotnet core SDK/RunTime 7.0 https://dotnet.microsoft.com/en-us/download/dotnet/7.0
- Before running the application - chia must be running in order to communicate with chia wallet RPC to generate offers.
- Review appsettings.json & create secrets for each project.
- If intending to run on chia testnet for testing make sure you run: 

```
chia configure -t true
```

This application requires access to the wallet certificate & key in order to use the chia wallet RPC to generate offer files. The following application settings (appsettings.json) need to be **filled in before this application will function**.

Initialize dotnet secrets for both SwapHunter.Tests & SwapHunter Projects, these will override the values stored in appsettings.json.

```
dotnet user-secrets init --id SwapHunterTests
dotnet user-secrets init --id SwapHunter
```

put the correct path to your wallet ssl key/cert.

```json
{
  "ChiaRpc": {
    "WalletRpcEndpoint": "https://127.0.0.1:9256",
    "Wallet_key_path": "/Users/kev/.chia/mainnet/config/ssl/wallet/private_wallet.key",
    "Wallet_cert_path": "/Users/kev/.chia/mainnet/config/ssl/wallet/private_wallet.crt"
  }
}
 ```

# Contributing
Please feel free to contribute some TXCH. The tests in this project are integration tests that create real offers using a test wallet on the testnet. A few tests can easily tie up all the TXCH in the wallet.

```json
txch1mcgpakts9fqxfp3k45t89vpweue0ucn4mc34g2wpv6e8fk63p6vq7a60et
```

# Credit
- Loading keys/cert method was taken directly from https://github.com/dkackman/chia-dotnet/tree/main
- Thanks to Yakuhito over at TibetSwap for helping me understand the Offers when they were failing.

# Disclaimer
I take no responsibility for the use or misuse of any products, services, or information provided. It is the sole responsibility of the user to exercise their judgment and discretion when utilizing these resources. I do not accept liability for any damages, losses, or adverse consequences resulting from the use, interpretation, or reliance upon the materials provided. Users are encouraged to seek professional advice and exercise caution when applying the information or engaging in any activities related this program.

# Build & Test Status
![Build](https://github.com/kevinonfrontend/swaphunter/actions/workflows/build_and_test.yml/badge.svg)

