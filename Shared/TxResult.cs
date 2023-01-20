namespace CardanoSharpTxBuilderDemo.Shared
{
    public enum TxKind
    {
        None,
        Error,
        Payment,
        Mint
    }

    public class TxResult
    {
        public string Message { get; set; } = string.Empty;

        public TxKind Kind { get; set; }
    }
}
