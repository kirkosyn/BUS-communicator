using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Communicator
{
    class AESProgram
    {
        private AesCryptoServiceProvider aes;
        private string salt;
        public AESProgram()
        {
            setAesVars();
        }
        public string DoAESEnc(string source)
        {
            string keySource = getAesKeys();
            string IVSource = getAesIV();
            try
            {
                string original = source;
                string text;
                // Create a new instance of the Aes
                // class.  This generates a new key and initialization 
                // vector (IV).

                using (Aes myAes = Aes.Create())
                {
                    // Encrypt the string to an array of bytes.
                    myAes.Key = Convert.FromBase64String(keySource);
                    myAes.IV = Convert.FromBase64String(IVSource);
                    byte[] encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);
                    text = Convert.ToBase64String(encrypted);

                }

                return text;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return "";
            }
        }

        public string DoAESDec(string source)
        {
            string keySource = getAesKeys();
            string IVSource = getAesIV();
            try
            {
                string original = source;
                string roundtrip;
                // Create a new instance of the Aes
                // class.  This generates a new key and initialization 
                // vector (IV).

                using (Aes myAes = Aes.Create())
                {
                    // Encrypt the string to an array of bytes.
                    myAes.Key = Convert.FromBase64String(keySource);
                    myAes.IV = Convert.FromBase64String(IVSource);
                    byte[] encrypted = Convert.FromBase64String(original);
                    roundtrip = DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);

                }

                return roundtrip;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return "";
            }
        }
        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                //System.IO.File.WriteAllText(@"Key.txt", Convert.ToBase64String(Key));
                //System.IO.File.WriteAllText(@"IV.txt", Convert.ToBase64String(IV));
                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                //aesAlg.Padding = PaddingMode.None;

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();

                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                //aesAlg.Padding = PaddingMode.None;
                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        private void setAesVars()
        {
            aes = new AesCryptoServiceProvider();
            aes.GenerateKey();
            aes.GenerateIV();
        }

        public string getAesKeys()
        {
            return Convert.ToBase64String(aes.Key);
        }

        public string getAesIV()
        {
            return Convert.ToBase64String(aes.IV);
        }

        public void setAesKeys(string keys)
        {
            aes.Key = Convert.FromBase64String(keys);
        }

        public void setAesIV(string iv)
        {
            aes.IV = Convert.FromBase64String(iv);
        }

        public void setSalt(string salt)
        {
            this.salt = salt;
        }

        public string getSalt()
        {
            return salt;
        }

        public string removeSalt(string msg)
        {
            return msg.Remove(msg.Length-salt.Length, salt.Length);
        }

        public string addSalt(string msg)
        {
            return String.Concat(msg, salt);
        }
    }
}
