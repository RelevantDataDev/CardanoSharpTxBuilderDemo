# CardanoSharpTxBuilderDemo
Demo app for building transactions using CardanoSharp Wallet &amp; Blazor Components.

This is a basic minimal Blazor Server hosted web assembly app that uses the CardanoSharp Blazor Components. 
The idea is to build out all the various ways you might want to use the CardanoSharp Wallet & Blazor Components, but in a way that does the bare minimum implementation, so this demo app can serve as a reference for getting started building other apps that use the CardanoSharp Wallet & Blazor Components.

- The main guts of the app live in the Index.razor file of the client project, the TxBuilder class of the Shared project, and the TxBuilderEndpoint of the Server project.

- So far just implementing the first attempt at a minting transaction that is based upon the NftSaleDemo app created by Kyle from CardanoSharp.

-- Step #1: Run the app
-- Step #2: Install the Eternl browser extension and set it to the PreProd network.
-- Step #3: Restore a test wallet if you have one or create one and fund it from the Cardano test faucet.
-- Step #4: Connect to the Eternl wallet in the app.
-- Step #5: Set a breakpoint in Index.razor where the wallet signing call occurs: (var userSignTxResult = await WalletSignTx(txResponse.TxCborHex, true);)
-- Step #6: Click the "Build Transaction: Nft Mint" button.

 - Currently everything goes great until the wallet opens up for signing. In Eternl the transaction preview is missing, and in Nami there is an error message from the wallet: "Inputs do not conform to this spec or are otherwise invalid.
