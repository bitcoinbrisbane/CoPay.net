using System;

namespace CoPay
{
    public class opts
    {
        public customData customData { get; set; }

        public opts()
        {
            this.customData = new customData();
        }
    }
}
