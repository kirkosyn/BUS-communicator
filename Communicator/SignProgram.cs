using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Communicator
{
    class SignProgram
    {
        RSAParameters rsaPubParams;
        RSAParameters rsaPrivateParams;
        public string clientModulus;
        public string clientExponent;
        public Tuple<string, string> ownPubKey;
        readonly RSAEncryptionPadding padding = RSAEncryptionPadding.OaepSHA1;
        readonly RSASignaturePadding spadding = RSASignaturePadding.Pkcs1;

        /// <summary>
        /// Konstruktor
        /// </summary>
        public SignProgram()
        {
            SetKeys();
            ToFileRsa(rsaPrivateParams, rsaPubParams);
            ReadXml(@"publicKeyPath.xml");
        }


        /// <summary>
        /// Ustawienie kluczy własnych
        /// </summary>
        public void SetKeys()
        {
            try
            {
                using (RSA rsa = RSA.Create())
                {
                    rsaPrivateParams = rsa.ExportParameters(true);
                    rsaPubParams = rsa.ExportParameters(false);
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

            }


        }

        /// <summary>
        /// Pobranie kluczy klienta
        /// </summary>
        /// <returns>klucze</returns>
        public RSAParameters GetClientKeys()
        {
            //ASCIIEncoding myAscii = new ASCIIEncoding();
            //RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();
            RSAParameters _rsaParams = new RSAParameters
            {
                Modulus = Encoding.UTF8.GetBytes(clientModulus),
                Exponent = Encoding.UTF8.GetBytes(clientExponent)
            };

            //rsaCSP.ImportParameters(_rsaParams);

            return _rsaParams;
        }

        /// <summary>
        /// Podpis hasha wiadomości
        /// </summary>
        public byte[] HashSign(byte[] message)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(rsaPrivateParams);

                HashProgram hash = new HashProgram();
                return rsa.SignHash(hash.DoHash(message), HashAlgorithmName.SHA1, spadding);
            }
        }

        /// <summary>
        /// Szyfrowanie wiadomości RSA
        /// </summary>
        /// <param name="rsaParams">parametry RSA (klucz publiczny klienta)</param>
        /// <param name="toEncrypt">wiadomość do szyfrowania</param>
        /// <returns></returns>
        public byte[] EncryptData(RSAParameters rsaParams, byte[] toEncrypt)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(rsaParams);
                return rsa.Encrypt(toEncrypt, padding);
            }
        }

        /// <summary>
        /// Sprawdzenie hasha
        /// </summary>
        /// <param name="rsaParams">parametry RSA (klucz publiczny klienta)</param>
        /// <param name="signedData">wiadomość podpisana</param>
        /// <param name="signature">podpis</param>
        /// <returns></returns>
        public bool VerifyHash(RSAParameters rsaParams, byte[] signedData, byte[] signature)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(rsaParams);

                bool dataOK = rsa.VerifyData(signedData, signature, HashAlgorithmName.SHA1, spadding);
                HashProgram hash = new HashProgram();

                return rsa.VerifyHash(hash.DoHash(signedData), signature, HashAlgorithmName.SHA1, spadding);
            }
        }
        //CryptoConfig.MapNameToOID("SHA256")

        /// <summary>
        /// Odszyfrowanie wiadomości
        /// </summary>
        /// <param name="encrypted">wiadomość</param>
        public string DecryptData(byte[] encrypted)
        {
            byte[] fromEncrypt;
            string roundTrip;
            //ASCIIEncoding myAscii = new ASCIIEncoding();

            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(rsaPrivateParams);
                fromEncrypt = rsa.Decrypt(encrypted, padding);
            }


            return roundTrip = Encoding.UTF8.GetString(fromEncrypt);
        }

        /// <summary>
        /// Pobranie publicznych kluczy RSA
        /// </summary>
        public RSAParameters PublicParameters
        {
            get
            {
                return rsaPubParams;
            }
        }

        /// <summary>
        /// Zapis kluczy do pliku xml
        /// </summary>
        /// <param name="rsaPriv">klucz prywatny</param>
        /// <param name="rsaPub">klucz publiczny</param>
        public static void ToFileRsa(RSAParameters rsaPriv, RSAParameters rsaPub)
        {
            //stream to save the keys
            FileStream fs = null;
            StreamWriter sw = null;

            //create RSA provider
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(rsaPriv);
                try
                {
                    //save private key
                    fs = new FileStream("privateKeyPath.xml", FileMode.Create, FileAccess.Write);
                    sw = new StreamWriter(fs);
                    sw.Write(rsa.ToXmlString(true));
                    sw.Flush();
                }
                finally
                {
                    if (sw != null) sw.Close();
                    if (fs != null) fs.Close();
                }

                rsa.ImportParameters(rsaPub);
                try
                {
                    //save public key
                    fs = new FileStream("publicKeyPath.xml", FileMode.Create, FileAccess.Write);
                    sw = new StreamWriter(fs);
                    sw.Write(rsa.ToXmlString(false));
                    sw.Flush();
                }
                finally
                {
                    if (sw != null) sw.Close();
                    if (fs != null) fs.Close();
                }
                rsa.Clear();
            }
        }

        /// <summary>
        /// Odczyt kluczy
        /// </summary>
        /// <param name="filePath">ścieżka do pliku</param>
        public void ReadXml(string filePath)
        {
            string modulus, exponent;
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            foreach (XmlNode node in doc.SelectNodes("RSAKeyValue"))
            {
                modulus = node.SelectSingleNode("Modulus").InnerText;
                exponent = node.SelectSingleNode("Exponent").InnerText;

                ownPubKey = Tuple.Create(modulus, exponent);
            }

        }

        /// <summary>
        /// Ustawienie wartości modulo klienta
        /// </summary>
        /// <param name="data">liczba</param>
        public void SetClientModulus(string data)
        {
            clientModulus = data;
        }

        /// <summary>
        /// Ustawienie wartości wykładnika potęgi klienta
        /// </summary>
        /// <param name="data">liczba</param>
        public void SetClientExponent(string data)
        {
            clientExponent = data;
        }

    }
}
