namespace CardanoSharpTxBuilderDemo.Shared
{
    public class TxRequestPay
    {
        public string WalletAddress { get; set; } = string.Empty;

        public long AmountInLovelace { get; set; }
    }
}
