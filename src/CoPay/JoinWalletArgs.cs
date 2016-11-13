using System;

namespace CoPay
{
    //              var args = {
    //  walletId: walletId,
    //  name: encCopayerName,
    //  xPubKey: xPubKey,
    //  requestPubKey: requestPubKey,
    //  customData: encCustomData,
    //};

    public class JoinWalletArgs
    {
        public Guid walletId { get; set; }

        public String name { get; set; }

        public String xPubKey { get; set; }

        public String requestPubKey { get; set; }

        public String customData { get; set; }

        public String copayerSignature { get; set; }
    }
}
