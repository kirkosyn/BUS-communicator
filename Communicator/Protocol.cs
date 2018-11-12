using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
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

        /// <summary>
        /// Konstruktor protokołu Diffiego-Hellmana
        /// </summary>
        public Protocol()
        {
            GeneratePrimeNumber();
            FindPrimitiveRoot();
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
        public void GeneratePrimeNumber()
        {
            string fileStream = @"primes.txt", line;
            Random rnd = new Random();
            int number = rnd.Next(0, 10000) * 2;
            line = File.ReadLines(fileStream).Skip(number).Take(1).First();

            primeNumber = Int32.Parse(line);
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
    }
}
