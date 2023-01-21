using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Extensions.Models.Transactions;
using CardanoSharpTxBuilderDemo.Server.Services;
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

        [HttpPost("build/pay")]
        public async Task<IActionResult> BuildTransactionPay([FromBody] TxRequestPay request)
        {
            try
            {
                var tx = await _txBuilderService.BuildTxPay(request);

                var txCborBytesHex = tx.Serialize().ToStringHex();

                return Ok(txCborBytesHex);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpPost("build/mint")]
        public async Task<IActionResult> BuildTransactionMint([FromBody] TxRequestMint request)
        {
            try
            {
                var nft = new TxDemoNft
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

                var txCborBytesHex = tx.Serialize().ToStringHex();

                return Ok(txCborBytesHex);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpPost("sign")]
        public async Task<IActionResult> BuildTransactionSignPay([FromBody] TxRequestSign request)
        {
            try
            {
                var tx = await _txBuilderService.BuildTxSign(request.TxCborHex, request.Witness);

                var txCbor = tx.GetCBOR();

                var txCborBytes = txCbor.EncodeToBytes();

                var txCborBytesHex = txCborBytes.ToStringHex();

                return Ok(txCborBytesHex);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }
    }
}
