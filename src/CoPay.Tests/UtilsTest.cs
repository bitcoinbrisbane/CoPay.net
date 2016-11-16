using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoPay.Tests
{
    [TestClass]
    public class UtilsTest
    {
        [TestMethod]
        public void Should_Get_CoPay_Hash()
        {
            String actual = CoPay.Utils.getCopayerHash("lucas", "", "");
        }

        [TestMethod]
        public void Should_Sign_Message()
        {
            //const String keyAsHex = "09458c090a69a38368975fb68115df2f4b0ab7d1bc463fc60c67aa1730641d6c";

            var actual = Utils.signMessage("hola", "09458c090a69a38368975fb68115df2f4b0ab7d1bc463fc60c67aa1730641d6c");
            //should.exist(sig);
            Assert.AreEqual("3045022100f2e3369dd4813d4d42aa2ed74b5cf8e364a8fa13d43ec541e4bc29525e0564c302205b37a7d1ca73f684f91256806cdad4b320b4ed3000bee2e388bcec106e0280e0", actual);
        }

        [TestMethod]
        public void Should_Hash_Message()
        {
            String actual = Utils.hashMessage("hola");
            Assert.AreEqual("4102b8a140ec642feaa1c645345f714bc7132d4fd2f7f6202db8db305a96172f", actual);
            
            //res.toString('hex').should.equal('4102b8a140ec642feaa1c645345f714bc7132d4fd2f7f6202db8db305a96172f');
        }
    }
}
