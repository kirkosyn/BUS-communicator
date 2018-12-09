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
        public HashProgram()
        {

        }
        public byte[] DoHash(string source)
        {
            using (SHA1 sha256Hash = SHA1.Create())
            {
                byte[] hash = GetHash(sha256Hash, source);
                return hash;
            }
        }

        public byte[] DoHash(byte[] source)
        {
            using (SHA1 sha256Hash = SHA1.Create())
            {
                HashAlgorithm hashAlgorithm = sha256Hash;
                byte[] hash = hashAlgorithm.ComputeHash(source);
                return hash;
            }
        }

        private byte[] GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            return data;
        }


    }
}
