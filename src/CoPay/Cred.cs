using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoPay
{
    public class Cred
    {
        public String network { get; set; }

        /// <summary>
        /// HD Priv key
        /// </summary>
        public String xPrivKey { get; set; }

        public string copayerId { get; set; }

        public String requestPrivKey { get; set; }

        //var requestDerivation = xPrivKey.derive(Constants.PATHS.REQUEST_KEY);
        //this.requestPrivKey = requestDerivation.privateKey.toString();
    }
}
