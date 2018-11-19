using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Communicator
{
    class SignProgram
    {
        /// <summary>
        /// Klucz własny prywatny
        /// </summary>
        private Tuple<BigInteger, BigInteger> keyPrivate;
        /// <summary>
        /// Klucz własny publiczny
        /// </summary>
        public Tuple<BigInteger, BigInteger> keyPublic;
        /// <summary>
        /// Klucz publiczny drugiej strony
        /// </summary>
        public Tuple<BigInteger, BigInteger> clientKeyPublic;
        private byte[] signature;

        /// <summary>
        /// Konstruktor
        /// </summary>
        public SignProgram()
        {
            SetKeys();
            signature = HashSign("Hello");
        }
        /// <summary>
        /// Generowanie liczby pierwszej z pliku
        /// </summary>
        public int GeneratePrimeNumber()
        {
            string fileStream = @"primes.txt", line;
            Random rnd = new Random();
            int number = rnd.Next(0, 9999) * 2;
            line = File.ReadLines(fileStream).Skip(number).Take(1).First();

            return Int32.Parse(line);
        }
        /// <summary>
        /// Znajdowanie liczby względnie pierwszej
        /// </summary>
        /// <param name="a">1. liczba pierwsza</param>
        /// <param name="b">2. liczba pierwsza</param>
        /// <param name="n">iloczyn dwóch liczb pierwszych</param>
        /// <returns></returns>
        public BigInteger FindCoprime(BigInteger a, BigInteger b, BigInteger n)
        {
            BigInteger result = BigInteger.Multiply((a - 1), (b - 1));
            BigInteger coprime = n - 1;
            while (coprime != 0)
            {
                if (BigInteger.GreatestCommonDivisor(coprime, result) == 1)
                    break;
                coprime--;
            }
            return coprime;
        }

        /// <summary>
        /// Odwrotność modulo
        /// </summary>
        /// <param name="a">1. liczba</param>
        /// <param name="b">2. liczba</param>
        /// <returns></returns>
        public BigInteger ModInverse(BigInteger a, BigInteger b)
        {
            BigInteger b0 = b, t, q;
            BigInteger x0 = 0, x1 = 1;
            if (b == 1)
                return 1;
            while (a > 1)
            {
                q = BigInteger.Divide(a, b);
                t = b;
                b = BigInteger.ModPow(a, 1, b);
                a = t;
                t = x0;
                x0 = BigInteger.Subtract(x1, BigInteger.Multiply(q, x0));
                x1 = t;
            }
            if (x1 < 0)
                x1 = BigInteger.Add(x1, b0);

            return x1;
        }

        /// <summary>
        /// Ustawienie kluczy własnych
        /// </summary>
        public void SetKeys()
        {
            int p, q;
            p = GeneratePrimeNumber();
            q = GeneratePrimeNumber();

            BigInteger n = p * q;
            BigInteger e = FindCoprime(p, q, n);

            keyPublic = Tuple.Create(n, e);
            BigInteger d = ModInverse(e, (p - 1) * (q - 1));
            keyPrivate = Tuple.Create(n, d);

        }

        /// <summary>
        /// Podpis hasha wiadomości
        /// </summary>
        public byte[] HashSign(string message)
        {
            RSAParameters _rsaParams = new RSAParameters();
            
            //szyfrowanie następuje własnym prywatnym kluczem
            _rsaParams.Modulus = keyPrivate.Item1.ToByteArray();
            _rsaParams.Exponent = keyPrivate.Item2.ToByteArray();

            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();
            rsaCSP.ImportParameters(_rsaParams);

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
        public void DecryptData(byte[] encrypted)
        {
            byte[] fromEncrypt;
            string roundTrip;
            ASCIIEncoding myAscii = new ASCIIEncoding();
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();

            RSAParameters _rsaParams = new RSAParameters();
            
            //odszyfrowanie następuje kluczem własnym prywatnym
            _rsaParams.Modulus = keyPrivate.Item1.ToByteArray();
            _rsaParams.Exponent = keyPrivate.Item2.ToByteArray();

            rsaCSP.ImportParameters(_rsaParams);
            fromEncrypt = rsaCSP.Decrypt(encrypted, false);
            roundTrip = myAscii.GetString(fromEncrypt);
        }

        /// <summary>
        /// Ustawia klucz publiczny clienta
        /// </summary>
        /// <param name="keys"></param>
        public void SetClientKeyPublic(Tuple<BigInteger, BigInteger> keys)
        {
            clientKeyPublic = Tuple.Create(keys.Item1, keys.Item2);
        }

    }
}
