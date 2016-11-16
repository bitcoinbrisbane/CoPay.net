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
    public class Client
    {
        private readonly string BWS_INSTANCE_URL = "https://bws.bitpay.com/bws/api";

        public Client(String baseUrl = "https://bws.bitpay.com/bws/api")
        {
            BWS_INSTANCE_URL = baseUrl;
        }

        public async Task<string> createWallet(String walletName, String copayerName, Int16 m, Int16 n, opts opts)
        {
            Cred cred = new Cred()
            {
                copayerId = copayerName,
                network = "testnet",
                requestPrivKey = "tprv8dxkXXLevuHXR3tLvBkaDLyCnQxsQQVafnDMEQNds8r8tjSPfNTGD5ShtpP8QeTdtCoWGmrMC5gs9j7ap8ATdSsAD2KCv87BGdzPWwmdJt2",
                xPrivKey = "cNaQCDwmmh4dS9LzCgVtyy1e1xjCJ21GUDHe9K98nzb689JvinGV"
            };

            CreateWallet.Request request = new CreateWallet.Request()
            {
                m = m,
                n = n,
                name = walletName,
                pubKey = "02fcba7ecf41bc7e1be4ee122d9d22e3333671eb0a3a87b5cdf099d59874e1940f",
                network = "testnet"
            };

            String url = BWS_INSTANCE_URL + "/v2/wallets/";
            String reqSignature = Utils.signRequest("GET", url, request, cred.xPrivKey);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-identity", cred.copayerId);
            client.DefaultRequestHeaders.Add("x-signature", reqSignature);

            String json = JsonConvert.SerializeObject(request);
            StringContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            using (HttpResponseMessage responseMessage = await client.PostAsync(url, requestContent))
            {
                if (responseMessage.IsSuccessStatusCode)
                {
                    String responseContent = await responseMessage.Content.ReadAsStringAsync();

                    CreateWallet.Response response = JsonConvert.DeserializeObject<CreateWallet.Response>(responseContent);
                    String share = buildSecret(response.walletId, cred.xPrivKey, "testnet");
                    return share;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public static void doJoinWallet(Guid walletId, String walletPrivKey, String xPubKey, String requestPubKey, String copayerName)
        {
            //var xPrivKey = new Bitcore.HDPrivateKey.fromString(this.xPrivKey);

            //// this extra derivation is not to share a non hardened xPubKey to the server.
            //var addressDerivation = xPrivKey.derive(this.getBaseAddressDerivationPath());
            //this.xPubKey = (new Bitcore.HDPublicKey(addressDerivation)).toString();

            //var requestDerivation = xPrivKey.derive(Constants.PATHS.REQUEST_KEY);
            //this.requestPrivKey = requestDerivation.privateKey.toString();

            //var pubKey = requestDerivation.publicKey;
            //this.requestPubKey = pubKey.toString();


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

            String personalEncryptingKey = "";
            String sharedEncryptingKey = "";

            var encCustomData = Utils.encryptMessage(json, personalEncryptingKey);
            var encCopayerName = Utils.encryptMessage(copayerName, sharedEncryptingKey);

            JoinWalletArgs args = new JoinWalletArgs()
            {
                walletId = walletId,
                name = encCopayerName,
                xPubKey = xPubKey,
                requestPubKey = requestPubKey,
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
    }
}
