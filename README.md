# CardanoSharpTxBuilderDemo
Demo app for building transactions using CardanoSharp Wallet &amp; Blazor Components.

This is a basic minimal Blazor Server hosted web assembly app that uses the CardanoSharp Blazor Components. 
The idea is to build out all the various ways you might want to use the CardanoSharp Wallet & Blazor Components, but in a way that does the bare minimum implementation, so this demo app can serve as a reference for getting started building other apps that use the CardanoSharp Wallet & Blazor Components.

- The main guts of the app live in the Index.razor file of the client project, the TxBuilder class of the Shared project, and the TxBuilderEndpoint of the Server project.

- So far just implementing the first attempt at a minting transaction that is based upon the NftSaleDemo app created by Kyle from CardanoSharp.

 - Currently everything goes great until Line 52 of Index.razor - there, the wallet opens up for signing, but  the transaction preview is missing and/or there is an error message from the wallet: "Inputs do not conform to this spec or are otherwise invalid."
