using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoPay
{
    public class Utils
    {
        public static String encryptMessage(String message, String password)
        {
            //default AES 128
            return Encrypt(message, password);
        }

        //args.name, args.xPubKey, args.requestPubKey
        public static String getCopayerHash(String name, String xPubKey, String requestPubKey)
        {
            //return [name, xPubKey, requestPubKey].join('|');
            return String.Format("{0}|{1}|{2}", name, xPubKey, requestPubKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="privKey">Assume hex</param>
        /// <returns></returns>
        public static String signMessage(String text, String privKey)
        {
            //Assume private key is in hex
            Byte[] keyAsBytes = NBitcoin.DataEncoders.Encoders.Hex.DecodeData(privKey);
            Key key = new Key(keyAsBytes);

            //var priv = new PrivateKey(privKey);
            BitcoinSecret priv = new BitcoinSecret(key, Network.Main);
            
            //var hash = Utils.hashMessage(text);
            String hash = Utils.hashMessage(text);

            //return crypto.ECDSA.sign(hash, priv, 'little').toString();
            String base64 = priv.PrivateKey.SignMessage(hash);
            Byte[] base64Bytes = NBitcoin.DataEncoders.Encoders.Base64.DecodeData(base64);

            return NBitcoin.DataEncoders.Encoders.Hex.EncodeData(base64Bytes);
        }

        public static string signRequest(String method, String url, CreateWallet.Request args, string key)
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

        public static String hashMessage(String test)
        {
            //  $.checkArgument(text);
            //var buf = new Buffer(text);
            //var ret = crypto.Hash.sha256sha256(buf);
            //ret = new Bitcore.encoding.BufferReader(ret).readReverse();
            //return ret;

            //Assume ascii?
            Byte[] data = NBitcoin.DataEncoders.Encoders.ASCII.DecodeData(test);
            Byte[] doubleHash = NBitcoin.Crypto.Hashes.SHA256(NBitcoin.Crypto.Hashes.SHA256(data));

            //Reverse bytes
            Array.Reverse(doubleHash);

            //Return as double sha 256
            return NBitcoin.DataEncoders.Encoders.Hex.EncodeData(doubleHash);
        }

        public static string Encrypt(string clearText, string key)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new
                    Rfc2898DeriveBytes(EncryptionKey, new byte[]
                    { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
