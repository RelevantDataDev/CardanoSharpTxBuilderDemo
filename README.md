# CardanoSharpTxBuilderDemo
Demo app for building transactions using CardanoSharp Wallet &amp; Blazor Components.

This is a basic minimal Blazor Server hosted web assembly app that uses the CardanoSharp Blazor Components. 
The idea is to build out all the various ways you might want to use the CardanoSharp Wallet & Blazor Components, but in a way that does the bare minimum implementation, so this demo app can serve as a reference for getting started building other apps that use the CardanoSharp Wallet & Blazor Components.

- The main guts of the app live in the Index.razor file of the client project, the TxBuilder class of the Shared project, and the TxBuilderEndpoint of the Server project.

- So far have "User pays ADA" and "User mints NFT"

- Preprod is used.

- A test wallet I created for the demo has its wallet address hard-coded as the payee.

- A test wallet has it's mnemonic seed phrase stored in clear-text in the policy file for convenience - this should NEVER be the way its setup in a production environment :) 
