using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Communicator
{
    class HashProgram
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        public HashProgram()
        {

        }
        /// <summary>
        /// Stworzenie hasha
        /// </summary>
        /// <param name="source">tekst</param>
        /// <returns></returns>
        public byte[] DoHash(string source)
        {
            using (SHA256 SHA256Hash = SHA256.Create())
            {
                
                byte[] hash = GetHash(SHA256Hash, source);
                return hash;
            }
        }

        /// <summary>
        /// Stworzenie hasha
        /// </summary>
        /// <param name="source">tablica bitowa</param>
        /// <returns></returns>
        public byte[] DoHash(byte[] source)
        {
            using (SHA256 SHA256Hash = SHA256.Create())
            {
                HashAlgorithm hashAlgorithm = SHA256Hash;
                byte[] hash = hashAlgorithm.ComputeHash(source);
                return hash;
            }
        }

        /// <summary>
        /// Pobranie funkcji skrótu
        /// </summary>
        /// <param name="hashAlgorithm">Algorytm hasha</param>
        /// <param name="input">tekst</param>
        /// <returns></returns>
        private byte[] GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            return data;
        }


    }
}
