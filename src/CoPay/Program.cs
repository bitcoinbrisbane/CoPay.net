using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CoPay
{
    class Program
    {
        static string personalEncryptingKey;
        static string sharedEncryptingKey;

        //testnet
        //livenet
        static void Main(string[] args)
        {
            CreateWalletRequest request = new CreateWalletRequest()
            {
                m = 1,
                n = 2,
                name = "test",
                pubKey = "02fcba7ecf41bc7e1be4ee122d9d22e3333671eb0a3a87b5cdf099d59874e1940f",
                network = "testnet"
            };

            Cred cred = new Cred()
            {
                copayerId = "gpib",
                network = "testnet",
                requestPrivKey = "tprv8dxkXXLevuHXR3tLvBkaDLyCnQxsQQVafnDMEQNds8r8tjSPfNTGD5ShtpP8QeTdtCoWGmrMC5gs9j7ap8ATdSsAD2KCv87BGdzPWwmdJt2",
                xPrivKey = "cNaQCDwmmh4dS9LzCgVtyy1e1xjCJ21GUDHe9K98nzb689JvinGV"
            };

            NBitcoin.ExtKey masterKey = new ExtKey();
            Console.WriteLine("Master key : " + masterKey.ToString(Network.Main));
            for (int i = 0; i < 5; i++)
            {
                ExtKey key = masterKey.Derive((uint)i);
                Console.WriteLine("Key " + i + " : " + key.ToString(Network.Main));
            }


            String url = "https://bws.bitpay.com/bws/api/v2/wallets/";
            String reqSignature = signRequest("GET", url, request, cred.xPrivKey);


            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-identity", cred.copayerId);
            client.DefaultRequestHeaders.Add("x-signature", reqSignature);


            String json = JsonConvert.SerializeObject(request);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            String share = "";

            using (HttpResponseMessage responseMessage = client.PostAsync(url, requestContent).Result)
            {
                if (responseMessage.IsSuccessStatusCode)
                {
                    String responseContent = responseMessage.Content.ReadAsStringAsync().Result;

                    CreateWalletResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateWalletResponse>(responseContent);
                    share = buildSecret(response.walletId, cred.xPrivKey, "testnet");
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            Console.Write(share);

        }

        private static void doJoinWallet(Guid walletId, String walletPrivKey, String xPubKey, String requestPubKey, String copayerName)
        {

            //String walletPrivKey = "";

            opts Opts = new opts();
            Opts.customData.walletPrivKey = walletPrivKey;

            String json = JsonConvert.SerializeObject(Opts.customData);


            //API.prototype._doJoinWallet = function(walletId, walletPrivKey, xPubKey, requestPubKey, copayerName, opts, cb) {
            //$.shouldBeFunction(cb);
            //              var self = this;

            //              opts = opts || { };

            //              // Adds encrypted walletPrivateKey to CustomData
            //              opts.customData = opts.customData || { };
            //              opts.customData.walletPrivKey = walletPrivKey.toString();
            var encCustomData = Utils.encryptMessage(json, personalEncryptingKey);
            var encCopayerName = Utils.encryptMessage(copayerName, sharedEncryptingKey);

            JoinWalletArgs args = new JoinWalletArgs() {
              walletId = walletId,
              name = encCopayerName,
              xPubKey = xPubKey,
              requestPubKey =requestPubKey,
              customData = encCustomData
            };

        //          if (opts.dryRun) args.dryRun = true;

        //          if (_.isBoolean(opts.supportBIP44AndP2PKH))
        //              args.supportBIP44AndP2PKH = opts.supportBIP44AndP2PKH;

            var hash = Utils.getCopayerHash(args.name, args.xPubKey, args.requestPubKey);
            args.copayerSignature = Utils.signMessage(hash, walletPrivKey);

            var url = String.Format("/v2/wallets/{0}/copayers", walletId);

  //          this._doPostRequest(url, args, function(err, body) {
  //              if (err) return cb(err);
  //              self._processWallet(body.wallet);
  //              return cb(null, body.wallet);
  //          });
  //      };
    }

        private static string buildSecret(Guid walletId, String walletPrivKey, string network)
        {
            string widHx = walletId.ToString("N");
            
            string widBase58 = Encoders.Base58.EncodeData(Encoders.Hex.DecodeData(widHx));

            return widBase58 + walletPrivKey + "L";
        }

        private static string signRequest(String method, String url, CreateWalletRequest args, string key)
        {
            //var sig = Utils.signMessage('hola', '09458c090a69a38368975fb68115df2f4b0ab7d1bc463fc60c67aa1730641d6c');
            //should.exist(sig);
            //sig.should.equal('3045022100f2e3369dd4813d4d42aa2ed74b5cf8e364a8fa13d43ec541e4bc29525e0564c302205b37a7d1ca73f684f91256806cdad4b320b4ed3000bee2e388bcec106e0280e0');
            String json = JsonConvert.SerializeObject(args);
            String message = String.Format("{0}|{1}|{2}", method.ToLower(), url, json);
            //var message = [method.toLowerCase(), url, JSON.stringify(args)].join('|');

            NBitcoin.BitcoinSecret s = new BitcoinSecret(key);
            return s.PrivateKey.SignMessage(message);
        }
    }

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

    public class CreateWalletRequest
    {
        public String name { get; set; }

        public Int16 m { get; set; }

        public Int16 n { get; set; }

        public String pubKey { get; set; }

        public String network { get; set; }
    }

    public class CreateWalletResponse
    {
        public Guid walletId { get; set; }
    }
}

  //  helpers.createAndJoinWallet = function(clients, m, n, opts, cb)
  //  {
  //      if (_.isFunction(opts))
  //      {
  //          cb = opts;
  //          opts = null;
  //      }

  //      opts = opts || { };

  //      clients[0].seedFromRandomWithMnemonic({
  //          network: 'testnet'
  //      });
  //      clients[0].createWallet('mywallet', 'creator', m, n, {
  //          network: 'testnet',
  //  singleAddress: !!opts.singleAddress,
  //}, function(err, secret) {
  //          should.not.exist(err);

  //          if (n > 1)
  //          {
  //              should.exist(secret);
  //          }
  //      }
