namespace CardanoSharpTxBuilderDemo.Shared
{
    public class TxDemoNft
    {
        public string Title { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string PolicyId { get; set; } = string.Empty;
        public string MintWallet { get; set; } = string.Empty;
        public DateTime MintDateUTC { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public long CostInLovelace { get; set; }
    }
}
