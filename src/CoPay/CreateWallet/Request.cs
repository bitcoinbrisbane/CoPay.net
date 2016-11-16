using System;

namespace CoPay.CreateWallet
{
    public class Request
    {
        public String name { get; set; }

        public Int16 m { get; set; }

        public Int16 n { get; set; }

        public String pubKey { get; set; }

        public String network { get; set; }
    }
}
