using CardanoSharp.Wallet;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Models.Derivations;
using CardanoSharp.Wallet.Models.Keys;
using CardanoSharp.Wallet.TransactionBuilding;
using CardanoSharp.Wallet.Utilities;
using Microsoft.Extensions.Configuration;

namespace CardanoSharpTxBuilderDemo.Shared
{
    public interface ITxPolicyManager
    {
        IScriptAllBuilder GetPolicyScript();
        PrivateKey GetPrivateKey();
        PublicKey GetPublicKey();
    }

    public class TxPolicyManager : ITxPolicyManager
    {
        private readonly PrivateKey _privateKey;
        private readonly PublicKey _publicKey;

        public TxPolicyManager(IConfiguration config)
        {
            var mnemonic = new MnemonicService()
                .Restore("absorb host frequent rubber step wrap either term there embrace tag rack agree silent genre essence above pistol spring parent resemble yard infant frame");
            IIndexNodeDerivation paymentNode1 = mnemonic.GetMasterNode()
                .Derive(PurposeType.PolicyKeys)
                .Derive()
                .Derive(0)
                .Derive(RoleType.ExternalChain)
                .Derive(0);
            paymentNode1.SetPublicKey();
            _privateKey = paymentNode1.PrivateKey;
            _publicKey = paymentNode1.PublicKey;
        }

        public IScriptAllBuilder GetPolicyScript() =>
            ScriptAllBuilder.Create
                .SetScript(NativeScriptBuilder.Create.SetKeyHash(
                    HashUtility.Blake2b224(_publicKey.Key)));

        public PrivateKey GetPrivateKey() => _privateKey;
        public PublicKey GetPublicKey() => _publicKey;
    }
}
