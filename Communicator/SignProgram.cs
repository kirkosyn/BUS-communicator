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
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();
            
            rsaPrivateParams = rsaCSP.ExportParameters(true);
            rsaPubParams = rsaCSP.ExportParameters(false);
            
        }

        public RSAParameters GetClientKeys()
        {
            ASCIIEncoding myAscii = new ASCIIEncoding();
            //RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();
            RSAParameters _rsaParams = new RSAParameters();
            
            _rsaParams.Modulus = myAscii.GetBytes(clientModulus);
            _rsaParams.Exponent = myAscii.GetBytes(clientExponent);
        
            //rsaCSP.ImportParameters(_rsaParams);

            return _rsaParams;
        }

        /// <summary>
        /// Podpis hasha wiadomości
        /// </summary>
        public byte[] HashSign(byte[] message)
        {
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();

            rsaCSP.ImportParameters(rsaPrivateParams);

            HashProgram hash = new HashProgram();
            return rsaCSP.SignHash(hash.DoHash(message), CryptoConfig.MapNameToOID("SHA256"));
        }

        /// <summary>
        /// Szyfrowanie wiadomości RSA
        /// </summary>
        /// <param name="rsaParams">parametry RSA (klucz publiczny klienta)</param>
        /// <param name="toEncrypt">wiadomość do szyfrowania</param>
        /// <returns></returns>
        public byte[] EncryptData(RSAParameters rsaParams, byte[] toEncrypt)
        {
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();

            rsaCSP.ImportParameters(rsaParams);
            return rsaCSP.Encrypt(toEncrypt, false);
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
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();
            rsaCSP.ImportParameters(rsaParams);

            bool dataOK = rsaCSP.VerifyData(signedData, CryptoConfig.MapNameToOID("SHA256"), signature);
            HashProgram hash = new HashProgram();

            return rsaCSP.VerifyHash(hash.DoHash(signedData), CryptoConfig.MapNameToOID("SHA256"), signature);
        }

        /// <summary>
        /// Odszyfrowanie wiadomości
        /// </summary>
        /// <param name="encrypted">wiadomość</param>
        public string DecryptData(byte[] encrypted)
        {
            byte[] fromEncrypt;
            string roundTrip;
            ASCIIEncoding myAscii = new ASCIIEncoding();
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();

            rsaCSP.ImportParameters(rsaPrivateParams);
            fromEncrypt = rsaCSP.Decrypt(encrypted, false);
            return roundTrip = myAscii.GetString(fromEncrypt);
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
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
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

        public void SetClientModulus(string data)
        {
            clientModulus = data;
        }

        public void SetClientExponent(string data)
        {
            clientExponent = data;
        }

    }
}
