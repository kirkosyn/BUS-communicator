using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Communicator
{
    class Protocol
    {
        /// <summary>
        /// Lista dzielników pierwszych
        /// </summary>
        private List<int> primeFactors;
        /// <summary>
        /// Lista testów
        /// </summary>
        private List<int> tests;
        /// <summary>
        /// Liczba pierwsza
        /// </summary>
        private int primeNumber;
        /// <summary>
        /// Pierwiastek pierwotny
        /// </summary>
        private int primitiveRoot;
        /// <summary>
        /// Tajna liczba całkowita
        /// </summary>
        private int secretNumber;
        /// <summary>
        /// Liczba do wysłania
        /// </summary>
        private BigInteger numberToSend;
        /// <summary>
        /// Tajna współdzielona liczba
        /// </summary>
        private BigInteger s;

        public Tuple<BigInteger,BigInteger> keyPrivate;
        public Tuple<BigInteger, BigInteger> keyPublic;

        /// <summary>
        /// Konstruktor protokołu Diffiego-Hellmana
        /// </summary>
        public Protocol()
        {
            primeNumber = GeneratePrimeNumber();
            FindPrimitiveRoot();
            SetKeys();
        }

        /// <summary>
        /// Konstruktor protokołu Diffiego-Hellmana z parametrami
        /// </summary>
        /// <param name="primeNumber">liczba pierwsza</param>
        /// <param name="primitiveRoot">pierwiastek pierwotny</param>
        public Protocol(int primeNumber, int primitiveRoot)
        {
            this.primeNumber = primeNumber;
            this.primitiveRoot = primitiveRoot;
        }

        /// <summary>
        /// Generowanie tajnej liczby całkowitej
        /// </summary>
        public void GenerateNumber()
        {
            Random rnd = new Random();
            secretNumber = rnd.Next(0, 10000000);
        }

        /// <summary>
        /// Tworzenie liczby do wysłania drugiej osobie
        /// </summary>
        public void CreateNumberToSend()
        {
            numberToSend = new BigInteger();
            numberToSend = BigInteger.ModPow(primitiveRoot, secretNumber, primeNumber);
        }

        /// <summary>
        /// Obliczanie współdzielonej liczby
        /// </summary>
        /// <param name="number">liczba uzyskana od drugiej osoby</param>
        public void CalculateReceivedNumber(BigInteger number)
        {
            s = new BigInteger();
            s = BigInteger.ModPow(number, secretNumber, primeNumber);
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
        /// Szukanie pierwiastka pierwotnego
        /// </summary>
        public void FindPrimitiveRoot()
        {
            int number = primeNumber;
            int s = number - 1;
            PrimeFactors(s);
            GenerateTests(s);

            primitiveRoot = PerformTests(number);
        }

        /// <summary>
        /// Wyznaczanie niepowtarzających się dzielników pierwszych liczby
        /// </summary>
        /// <param name="number">liczba</param>
        private void PrimeFactors(int number)
        {
            List<int> listCopy = new List<int>();
            primeFactors = new List<int>();
            int i = 2;
            while (number != 1)
            {
                if (number % i == 0)
                {
                    listCopy.Add(i);
                    number = number / i;
                }
                else
                    i++;
            }
            primeFactors = listCopy.Distinct().ToList();
        }

        /// <summary>
        /// Generowanie testów do znalezienia pierwiastka pierwotnego
        /// </summary>
        /// <param name="number">liczba</param>
        private void GenerateTests(int number)
        {
            tests = new List<int>();
            primeFactors.ForEach(element => tests.Add(number / element));
        }

        /// <summary>
        /// Wykonanie testów znalezienia pierwiastka pierwotnego modulo liczba
        /// </summary>
        /// <param name="number">liczba do modulo</param>
        /// <returns></returns>
        private int PerformTests(int number)
        {
            //BigInteger valuePrime = new BigInteger();
            //BigInteger valueExponent = new BigInteger();
            //BigInteger valueModulus = new BigInteger(number);
            BigInteger valueEnd = new BigInteger();
            int valuePrime = 2;
            int valueExponent;
            int valueModulus = number;

            Random rnd = new Random();

            bool root = true;

            for (; ; )
            {
                for (int j = 0; j < tests.Count; j++)
                {
                    valuePrime = rnd.Next(2, number - 1);
                    valueExponent = tests[j];
                    valueEnd = BigInteger.ModPow(valuePrime, valueExponent, valueModulus);
                    
                    if (valueEnd == 1)
                    {
                        root = false;
                        break;
                    }
                }
                if (root)
                {
                    break;
                }
                else if (!root)
                {
                    root = true;
                    continue;
                }

            }

            return valuePrime;
        }

        /// <summary>
        /// Zwraca liczbę pierwszą
        /// </summary>
        /// <returns></returns>
        public int GetPrimeNumber()
        {
            return primeNumber;
        }

        /// <summary>
        /// Zwraca pierwiastek pierwotny
        /// </summary>
        /// <returns></returns>
        public int GetPrimitiveRoot()
        {
            return primitiveRoot;
        }

        /// <summary>
        /// Ustawianie liczby pierwszej
        /// </summary>
        /// <param name="primeNumber">wartość liczby</param>
        public void SetPrimeNumber(int primeNumber)
        {
            this.primeNumber = primeNumber;
        }

        /// <summary>
        /// Ustawianie pierwiastka pierwotnego
        /// </summary>
        /// <param name="primitiveRoot">wartość pierwiastka</param>
        public void SetPrimitiveRoot(int primitiveRoot)
        {
            this.primitiveRoot = primitiveRoot;
        }

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
        public void HashSign()
        {
            RSAParameters _rsaParams = new RSAParameters();
            _rsaParams.Modulus = keyPrivate.Item1.ToByteArray();
            _rsaParams.Exponent = keyPrivate.Item2.ToByteArray();

            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();
            rsaCSP.ImportParameters(_rsaParams);

            HashProgram hash = new HashProgram();

            rsaCSP.SignHash(hash.DoHash("Hello"), CryptoConfig.MapNameToOID("SHA256"));
        }

        public byte[] EncryptData(RSAParameters rsaParams, byte[] toEncrypt)
        {
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();

            rsaCSP.ImportParameters(rsaParams);
            return rsaCSP.Encrypt(toEncrypt, false);
        }

        public bool VerifyHash(RSAParameters rsaParams, byte[] signedData, byte[] signature)
        {
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();
            rsaCSP.ImportParameters(rsaParams);

            bool dataOK = rsaCSP.VerifyData(signedData, CryptoConfig.MapNameToOID("SHA256"), signature);
            HashProgram hash = new HashProgram();

            return rsaCSP.VerifyHash(hash.DoHash(signedData), CryptoConfig.MapNameToOID("SHA256"), signature);
        }

        public void DecryptData(byte[] encrypted)
        {
            byte[] fromEncrypt;
            string roundTrip;
            ASCIIEncoding myAscii = new ASCIIEncoding();
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();

            RSAParameters _rsaParams = new RSAParameters();
            _rsaParams.Modulus = keyPrivate.Item1.ToByteArray();
            _rsaParams.Exponent = keyPrivate.Item2.ToByteArray();

            rsaCSP.ImportParameters(_rsaParams);
            fromEncrypt = rsaCSP.Decrypt(encrypted, false);
            roundTrip = myAscii.GetString(fromEncrypt);

            Console.WriteLine("RoundTrip: {0}", roundTrip);
        }
    }
}
