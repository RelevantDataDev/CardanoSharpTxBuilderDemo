﻿using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Extensions.Models.Transactions;
using CardanoSharpTxBuilderDemo.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CardanoSharpTxBuilderDemo.Server.Controllers
{
    [ApiController]
    [Route("tx")]
    public class TxBuilderEndpoint : ControllerBase
    {
        private readonly ITxBuilderService _txBuilderService;

        // GET
        public TxBuilderEndpoint(ITxBuilderService txBuilderService)
        {
            _txBuilderService = txBuilderService;
        }

        [HttpPost("build")]
        public async Task<IActionResult> BuildTransaction([FromBody] BuildTxRequest request)
        {
            try
            {
                var nft = new TxBuilderNft
                {
                    Title = request.Label,
                    CostInLovelace = 10000000,
                    ImageUrl = "ipfs://QmNUM16BNZvFzXhFxjaKvijswAb7ybVbswzP9DuWA8KuVT",
                    MintDateUTC = DateTime.UtcNow,
                    AssetName = "CardanoSharp TxBuilder - Demo",
                    PolicyId = "",
                    MintWallet = request.WalletAddress
                };

                var tx = await _txBuilderService.BuildTxMint(nft);

                var txCbor = tx.GetCBOR();

                var txCborBytes = txCbor.EncodeToBytes();

                var txCborBytesHex = txCborBytes.ToStringHex();

                return Ok(new BuildTxResponse
                {
                    TxCborHex = txCborBytesHex
                });
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }
    }
}
