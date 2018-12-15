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
        /// <summary>
        /// Prywatne klucze własne
        /// </summary>
        RSAParameters rsaPrivateParams;

        /// <summary>
        /// Modulo klienta - do klucza prywatnego
        /// </summary>
        public string clientModulus;
        /// <summary>
        /// Exponent klienta - do klucza prywatnego
        /// </summary>
        public string clientExponent;
        /// <summary>
        /// P klienta - do klucza prywatnego
        /// </summary>
        public string clientP;
        /// <summary>
        /// Q klienta - do klucza prywatnego
        /// </summary>
        public string clientQ;
        /// <summary>
        /// DP klienta - do klucza prywatnego
        /// </summary>
        public string clientDP;
        /// <summary>
        /// DQ klienta - do klucza prywatnego
        /// </summary>
        public string clientDQ;
        /// <summary>
        /// InverseQ klienta - do klucza prywatnego
        /// </summary>
        public string clientInverseQ;
        /// <summary>
        /// D klienta - do klucza prywatnego
        /// </summary>
        public string clientD;

        /// <summary>
        /// Własne klucze publiczne - do przesyłu
        /// </summary>
        public Tuple<string, string> ownPubKey;
        /// <summary>
        /// Własne klucze prywatne - do przesyłu
        /// </summary>
        public List<string> ownPrivKey;

        /// <summary>
        /// Padding do enkrypcji
        /// </summary>
        readonly RSAEncryptionPadding padding = RSAEncryptionPadding.Pkcs1;
        /// <summary>
        /// Padding do podpisu
        /// </summary>
        readonly RSASignaturePadding spadding = RSASignaturePadding.Pkcs1;

        /// <summary>
        /// Konstruktor
        /// </summary>
        public SignProgram()
        {
            SetKeys();
            ToFileRsa(rsaPrivateParams, PublicParameters);
            ReadPublicXml(@"publicKeyPath.xml");
            ReadPrivateXml(@"privateKeyPath.xml");
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
                    PublicParameters = rsa.ExportParameters(false);
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

            }


        }

        /// <summary>
        /// Pobranie kluczy publicznych klienta
        /// </summary>
        /// <returns>klucze</returns>
        public RSAParameters GetClientPublicKeys()
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
        /// Pobranie kluczy prywatnych klienta
        /// </summary>
        /// <returns>klucze</returns>
        public RSAParameters GetClientPrivateKeys()
        {
            //ASCIIEncoding myAscii = new ASCIIEncoding();
            //RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();
            RSAParameters _rsaParams = new RSAParameters
            {
                Modulus = Encoding.UTF8.GetBytes(clientModulus),
                Exponent = Encoding.UTF8.GetBytes(clientExponent),
                Q = Encoding.UTF8.GetBytes(clientQ),
                P = Encoding.UTF8.GetBytes(clientP),
                DP = Encoding.UTF8.GetBytes(clientDP),
                DQ = Encoding.UTF8.GetBytes(clientDQ),
                InverseQ = Encoding.UTF8.GetBytes(clientInverseQ),
                D = Encoding.UTF8.GetBytes(clientD)
            };

            //rsaCSP.ImportParameters(_rsaParams);

            return _rsaParams;
        }

        /// <summary>
        /// Podpis hasha wiadomości
        /// <param name="message">wiadomość do zahashowania</param>
        /// <returns>podpisany hash</returns>
        /// </summary>
        // prywatne klucze własne
        public byte[] HashSign(byte[] message)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(rsaPrivateParams);

                HashProgram hash = new HashProgram();

                return rsa.SignHash(hash.DoHash(message), HashAlgorithmName.SHA256, spadding);
            }
        }

        /// <summary>
        /// Szyfrowanie wiadomości RSA
        /// </summary>
        /// <param name="rsaParams">parametry RSA (klucz publiczny klienta)</param>
        /// <param name="toEncrypt">wiadomość do szyfrowania</param>
        /// <returns>zaszyfrowany tekst</returns>
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
        /// <returns>czy hash jest poprawny</returns>
        public bool VerifyHash(RSAParameters rsaParams, byte[] signedData, byte[] signature)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(rsaParams);

                bool dataOK = rsa.VerifyData(signedData, signature, HashAlgorithmName.SHA256, spadding);
                HashProgram hash = new HashProgram();

                return rsa.VerifyHash(hash.DoHash(signedData), signature, HashAlgorithmName.SHA256, spadding);
            }
        }
        //CryptoConfig.MapNameToOID("SHA256")

        /// <summary>
        /// Odszyfrowanie wiadomości
        /// </summary>
        /// <param name="encrypted">wiadomość</param>
        /// <returns>odszyfrowany tekst</returns>
        public string DecryptData(byte[] encrypted) //klucz prywatny klienta
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
        public RSAParameters PublicParameters { get; private set; }

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
        /// Odczyt publicznych kluczy
        /// </summary>
        /// <param name="filePath">ścieżka do pliku</param>
        public void ReadPublicXml(string filePath)
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
        /// Odczyt prywatnych kluczy
        /// </summary>
        /// <param name="filePath">ścieżka do pliku</param>
        public void ReadPrivateXml(string filePath)
        {
            string modulus, exponent, p, q, dp, dq, inverseq, d;
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            ownPrivKey = new List<string>();

            foreach (XmlNode node in doc.SelectNodes("RSAKeyValue"))
            {
                modulus = node.SelectSingleNode("Modulus").InnerText;
                exponent = node.SelectSingleNode("Exponent").InnerText;
                p = node.SelectSingleNode("P").InnerText;
                q = node.SelectSingleNode("Q").InnerText;
                dp = node.SelectSingleNode("DP").InnerText;
                dq = node.SelectSingleNode("DQ").InnerText;
                inverseq = node.SelectSingleNode("InverseQ").InnerText;
                d = node.SelectSingleNode("D").InnerText;

                ownPrivKey.Add(modulus); //0
                ownPrivKey.Add(exponent); //1
                ownPrivKey.Add(p); //2
                ownPrivKey.Add(q); //3
                ownPrivKey.Add(dp); //4
                ownPrivKey.Add(dq); //5
                ownPrivKey.Add(inverseq); //6
                ownPrivKey.Add(d); //7
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

        /// <summary>
        /// Ustawienie wartości D klienta
        /// </summary>
        /// <param name="data">liczba</param>
        public void SetClientD(string data)
        {
            clientD = data;
        }

        /// <summary>
        /// Ustawienie wartości Q klienta
        /// </summary>
        /// <param name="data">liczba</param>
        public void SetClientQ(string data)
        {
            clientQ = data;
        }

        /// <summary>
        /// Ustawienie wartości InverseQ klienta
        /// </summary>
        /// <param name="data">liczba</param>
        public void SetClientInverseQ(string data)
        {
            clientInverseQ = data;
        }

        /// <summary>
        /// Ustawienie wartości P klienta
        /// </summary>
        /// <param name="data">liczba</param>
        public void SetClientP(string data)
        {
            clientP = data;
        }

        /// <summary>
        /// Ustawienie wartości DQ klienta
        /// </summary>
        /// <param name="data">liczba</param>
        public void SetClientDQ(string data)
        {
            clientDQ = data;
        }

        /// <summary>
        /// Ustawienie wartości DP klienta
        /// </summary>
        /// <param name="data">liczba</param>
        public void SetClientDP(string data)
        {
            clientDP = data;
        }

    }
}
