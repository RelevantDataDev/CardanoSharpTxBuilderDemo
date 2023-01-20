using CardanoSharp.Wallet.CIPs.CIP2.Models;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Models.Addresses;
using CardanoSharp.Wallet.Models.Transactions;
using CardanoSharp.Wallet.Models;
using CardanoSharp.Wallet.TransactionBuilding;
using System.Text;
using CardanoSharp.Koios.Client;
using Microsoft.Extensions.Configuration;
using CardanoSharp.Wallet.CIPs.CIP2;
using CardanoSharp.Wallet.Extensions.Models.Transactions;
using CardanoSharp.Wallet.Extensions.Models.Transactions.TransactionWitnesses;

namespace CardanoSharpTxBuilderDemo.Shared
{
    public interface ITxBuilderService
    {
        Task<Transaction?> BuildTxPay(TxRequestPay payment);
        Task<Transaction?> BuildTxMint(TxDemoNft nft);
        Task<Transaction?> BuildTxSign(string txCborHex, string witness);
    }

    public class TxBuilderService : ITxBuilderService
    {
        private readonly IAddressClient _addressClient;
        private readonly INetworkClient _networkClient;
        private readonly IEpochClient _epochClient;
        private readonly ITxPolicyManager _policyManager;
        private readonly string _sendPaymentToAddress;

        public TxBuilderService(IConfiguration config, IAddressClient addressClient, INetworkClient networkClient, IEpochClient epochClient, ITxPolicyManager policyManager)
        {
            _addressClient = addressClient;
            _networkClient = networkClient;
            _epochClient = epochClient;
            _policyManager = policyManager;
            _sendPaymentToAddress = "addr_test1qp8x9c0j7zln32fsa7sws3x0s5heu3mz0j08935lkyc6msj8g9w84yjdj2fnrsq2725vvsacw23lay5xqt4wr46fhscqdlrzcz";
        }

        public async Task<Transaction?> BuildTxPay(TxRequestPay payment)
        {
            // Address making the payment
            var senderAddress = new Address(payment.WalletAddress);

            // Address receiving the payment
            var receiverAddress = new Address(_sendPaymentToAddress);

            //1. Get UTxOs
            var utxos = await GetUtxos(senderAddress.ToString());

            ///2. Create the Body
            var transactionBody = TransactionBodyBuilder.Create;

            //set payment outputs
            transactionBody.AddOutput(_sendPaymentToAddress.ToAddress().GetBytes(), (ulong) payment.AmountInLovelace);

            //perform coin selection
            var coinSelection = ((TransactionBodyBuilder)transactionBody).UseRandomImprove(utxos, senderAddress.ToString());

            //add the inputs from coin selection to transaction body builder
            AddInputsFromCoinSelection(coinSelection, transactionBody);

            //if we have change from coin selection, add to outputs
            if (coinSelection.ChangeOutputs is not null && coinSelection.ChangeOutputs.Any())
            {
                AddChangeOutputs(transactionBody, coinSelection.ChangeOutputs, senderAddress.ToString());
            }

            //get protocol parameters and set default fee
            var ppResponse = await _epochClient.GetProtocolParameters();
            var protocolParameters = ppResponse.Content.FirstOrDefault();
            protocolParameters.MinFeeB = 155381;
            protocolParameters.MinFeeA = 44;

            transactionBody.SetFee(protocolParameters.MinFeeB.Value);

            //get network tip and set ttl
            var blockSummaries = (await _networkClient.GetChainTip()).Content;
            var ttl = 2500 + (uint)blockSummaries.First().AbsSlot;
            transactionBody.SetTtl(ttl);

            ///3. Mock Witnesses
            var witnessSet = TransactionWitnessSetBuilder.Create
                .MockVKeyWitness(2);

            ///4. Build Draft TX
            //create transaction builder and add the pieces
            var transaction = TransactionBuilder.Create;
            transaction.SetBody(transactionBody);
            transaction.SetWitnesses(witnessSet);

            //get a draft transaction to calculate fee
            var draft = transaction.Build();
            var fee = draft.CalculateFee(protocolParameters.MinFeeA, protocolParameters.MinFeeB);

            //update fee and change output
            transactionBody.SetFee(fee);
            transactionBody.RemoveFeeFromChange();

            var rawTx = transaction.Build();

            //remove mock witness
            var mockWitnesses = rawTx.TransactionWitnessSet.VKeyWitnesses.Where(x => x.IsMock);

            foreach (var mw in mockWitnesses)
            {
                rawTx.TransactionWitnessSet.VKeyWitnesses.Remove(mw);
            }

            return rawTx;
        }

        public async Task<Transaction?> BuildTxMint(TxDemoNft nft)
        {
            //0. Prep
            var address = new Address(nft.MintWallet.HexToByteArray());
            var scriptPolicy = _policyManager.GetPolicyScript();

            //1. Get UTxOs
            var utxos = await GetUtxos(address.ToString());

            ///2. Create the Body
            var transactionBody = TransactionBodyBuilder.Create;

            //set payment outputs
            transactionBody.AddOutput(_sendPaymentToAddress.ToAddress().GetBytes(), (ulong)(nft.CostInLovelace));

            //set mint
            var policyId = scriptPolicy.Build().GetPolicyId();
            ITokenBundleBuilder tbb = TokenBundleBuilder.Create
                .AddToken(policyId, nft.Title.ToBytes(), 1);
            transactionBody.AddOutput(address.GetBytes(), 2000000, tbb, outputPurpose: OutputPurpose.Mint);
            transactionBody.SetMint(tbb);

            //perform coin selection
            var coinSelection = ((TransactionBodyBuilder)transactionBody).UseRandomImprove(utxos, address.ToString(), tbb);

            //add the inputs from coin selection to transaction body builder
            AddInputsFromCoinSelection(coinSelection, transactionBody);

            //if we have change from coin selection, add to outputs
            if (coinSelection.ChangeOutputs is not null && coinSelection.ChangeOutputs.Any())
            {
                AddChangeOutputs(transactionBody, coinSelection.ChangeOutputs, address.ToString());
            }

            //get protocol parameters and set default fee
            var ppResponse = await _epochClient.GetProtocolParameters();
            var protocolParameters = ppResponse.Content.FirstOrDefault();
            protocolParameters.MinFeeB = 155381;
            protocolParameters.MinFeeA = 44;


            transactionBody.SetFee(protocolParameters.MinFeeB.Value);

            //get network tip and set ttl
            var blockSummaries = (await _networkClient.GetChainTip()).Content;
            var ttl = 2500 + (uint)blockSummaries.First().AbsSlot;
            transactionBody.SetTtl(ttl);

            ///3. Mock Witnesses
            var witnessSet = TransactionWitnessSetBuilder.Create
                .SetScriptAllNativeScript(scriptPolicy)
                .MockVKeyWitness(2);

            //metadata
            nft.PolicyId = policyId.ToStringHex();

            var metadata = GetMetadata(nft);
            var auxData = AuxiliaryDataBuilder.Create.AddMetadata(721, metadata);

            ///4. Build Draft TX
            //create transaction builder and add the pieces
            var transaction = TransactionBuilder.Create;
            transaction.SetBody(transactionBody);
            transaction.SetWitnesses(witnessSet);
            transaction.SetAuxData(auxData);

            //get a draft transaction to calculate fee
            var draft = transaction.Build();
            var fee = draft.CalculateFee(protocolParameters.MinFeeA, protocolParameters.MinFeeB);

            //update fee and change output
            transactionBody.SetFee(fee);
            transactionBody.RemoveFeeFromChange();

            var rawTx = transaction.Build();

            //remove mock witness
            var mockWitnesses = rawTx.TransactionWitnessSet.VKeyWitnesses.Where(x => x.IsMock);
            foreach (var mw in mockWitnesses)
                rawTx.TransactionWitnessSet.VKeyWitnesses.Remove(mw);

            return rawTx;
        }

        public async Task<Transaction?> BuildTxSign(string txCborHex, string witness)
        {
            var transaction = txCborHex.HexToByteArray().DeserializeTransaction();
            var vKeyWitnesses = witness.HexToByteArray().DeserializeTransactionWitnessSet();

            if(transaction.TransactionWitnessSet == null)
            {
                transaction.TransactionWitnessSet = new TransactionWitnessSet();
            }

            foreach (var vkeyWitness in vKeyWitnesses.VKeyWitnesses)
            {
                transaction.TransactionWitnessSet.VKeyWitnesses.Add(vkeyWitness);
            }

            transaction.TransactionWitnessSet.VKeyWitnesses.Add(new VKeyWitness()
            {
                VKey = _policyManager.GetPublicKey(),
                SKey = _policyManager.GetPrivateKey()
            });

            return await Task.FromResult(transaction);
        }

        private async Task<List<Utxo>> GetUtxos(string address)
        {
            try
            {
                var addressBulkRequest = new AddressBulkRequest { Addresses = new List<string> { address } };
                var addressResponse = await _addressClient.GetAddressInformation(addressBulkRequest);
                var addressInfo = addressResponse.Content;
                var utxos = new List<Utxo>();

                foreach (var ai in addressInfo.SelectMany(x => x.UtxoSets))
                {
                    if (ai is null) continue;
                    var utxo = new Utxo()
                    {
                        TxIndex = ai.TxIndex,
                        TxHash = ai.TxHash,
                        Balance = new Balance()
                        {
                            Lovelaces = ulong.Parse(ai.Value)
                        }
                    };

                    var assetList = new List<Asset>();
                    foreach (var aa in ai.AssetList)
                    {
                        assetList.Add(new Asset()
                        {
                            Name = aa.AssetName,
                            PolicyId = aa.PolicyId,
                            Quantity = long.Parse(aa.Quantity)
                        });
                    }

                    utxo.Balance.Assets = assetList;
                    utxos.Add(utxo);
                }

                return utxos;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private void AddInputsFromCoinSelection(CoinSelection coinSelection, ITransactionBodyBuilder transactionBody)
        {
            foreach (var i in coinSelection.Inputs)
            {
                transactionBody.AddInput(i.TransactionId, i.TransactionIndex);
            }
        }

        private void AddChangeOutputs(ITransactionBodyBuilder ttb, List<TransactionOutput> outputs, string address)
        {
            foreach (var output in outputs)
            {
                ITokenBundleBuilder? assetList = null;

                if (output.Value.MultiAsset is not null)
                {
                    assetList = TokenBundleBuilder.Create;
                    foreach (var ma in output.Value.MultiAsset)
                    {
                        foreach (var na in ma.Value.Token)
                        {
                            assetList.AddToken(ma.Key, na.Key, na.Value);
                        }
                    }
                }

                ttb.AddOutput(new Address(address), output.Value.Coin, assetList, outputPurpose: OutputPurpose.Change);
            }
        }

        private Dictionary<string, object> GetMetadata(TxDemoNft nft)
        {
            var file = new
            {
                name = $"{nft.AssetName} Icon",
                mediaType = "image/png",
                src = nft.ImageUrl
            };

            var fileElement = new List<object>() { file };

            var assetElement = new Dictionary<string, object>()
            {
                {
                    Encoding.ASCII.GetBytes($"{nft.AssetName} {nft.Title}").ToStringHex(),
                    new
                    {
                        name = nft.AssetName,
                        image = nft.ImageUrl,
                        mediaType = "image/png",
                        files = fileElement,
                        serialNum = $"{nft.Title}-{nft.MintDateUTC.Ticks}",
                        rarity = nft.Title
                    }
                }
            };

            var policyElement = new Dictionary<string, object>()
            {
                {
                    nft.PolicyId, assetElement
                }
            };

            // return new Dictionary<string, object>()
            // {
            //     {
            //         "721", policyElement
            //     }
            // };

            return policyElement;
        }
    }
}
