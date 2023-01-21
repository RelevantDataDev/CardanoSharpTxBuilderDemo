namespace CardanoSharpTxBuilderDemo.Shared
{
    public class MetaDataInfo
    {
        public string name { get; set; } = string.Empty;
        public string image { get; set; } = string.Empty;
        public string mediaType { get; set; } = "image/png";
        public string serialNum { get; set; } = string.Empty;
        public string rarity { get; set; } = string.Empty;
        public object[] files { get; set; } = Array.Empty<object>();
    }
}
