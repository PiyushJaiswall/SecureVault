using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SecureVault.Services
{
    public class CryptoService
    {
        private readonly byte[] _key;
        private const int KeySize = 256;
        private const int BlockSize = 128;

        public CryptoService(string password)
        {
            _key = GenerateKeyFromPassword(password);
        }

        private byte[] GenerateKeyFromPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // A more robust salt would be better, but this is sufficient for this example
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "SecureVault_Salt_2025"));
            }
        }

        public void DecryptFileToStream(string inputFilePath, Stream outputStream)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Key = _key;

                using (var inputStream = new FileStream(inputFilePath, FileMode.Open))
                {
                    var iv = new byte[16];
                    inputStream.Read(iv, 0, iv.Length);
                    aes.IV = iv;

                    using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        cryptoStream.CopyTo(outputStream);
                    }
                }
            }
        }

        public void ChangeMasterKey(string path, CryptoService newCryptoService)
        {
            // Decrypt the validation text with the old key
            var encryptedData = File.ReadAllBytes(path);
            var decryptedData = DecryptData(encryptedData);

            // Re-encrypt the validation text with the new key and overwrite the file
            var newEncryptedData = newCryptoService.EncryptData(decryptedData);
            File.WriteAllBytes(path, newEncryptedData);
        }

        // --- NEW METHOD ---
        // Creates an encrypted master key file to verify the password later
        public void CreateMasterKeyFile(string path)
        {
            var testData = Encoding.UTF8.GetBytes("SECUREVAULT_MASTER_KEY_VALIDATION");
            var encryptedData = EncryptData(testData);
            File.WriteAllBytes(path, encryptedData);
        }

        // --- NEW METHOD ---
        // Tries to decrypt the master key file to verify the password is correct
        public bool VerifyMasterKeyFile(string path)
        {
            if (!File.Exists(path)) return false;

            try
            {
                var encryptedData = File.ReadAllBytes(path);
                var decryptedData = DecryptData(encryptedData);
                var validationText = Encoding.UTF8.GetString(decryptedData);
                return validationText == "SECUREVAULT_MASTER_KEY_VALIDATION";
            }
            catch (CryptographicException)
            {
                // This exception occurs if the password (and thus the key) is wrong
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void EncryptFile(string inputFilePath, string outputFilePath)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Key = _key;
                aes.GenerateIV();

                using (var outputStream = new FileStream(outputFilePath, FileMode.Create))
                {
                    outputStream.Write(aes.IV, 0, aes.IV.Length);

                    using (var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (var inputStream = new FileStream(inputFilePath, FileMode.Open))
                    {
                        inputStream.CopyTo(cryptoStream);
                    }
                }
            }
        }

        public void DecryptFile(string inputFilePath, string outputFilePath)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Key = _key;

                using (var inputStream = new FileStream(inputFilePath, FileMode.Open))
                {
                    var iv = new byte[16];
                    inputStream.Read(iv, 0, iv.Length);
                    aes.IV = iv;

                    using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var outputStream = new FileStream(outputFilePath, FileMode.Create))
                    {
                        cryptoStream.CopyTo(outputStream);
                    }
                }
            }
        }

        public byte[] EncryptData(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Key = _key;
                aes.GenerateIV();

                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(aes.IV, 0, aes.IV.Length);

                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        public byte[] DecryptData(byte[] encryptedData)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Key = _key;

                using (var memoryStream = new MemoryStream(encryptedData))
                {
                    var iv = new byte[16];
                    memoryStream.Read(iv, 0, iv.Length);
                    aes.IV = iv;

                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var outputStream = new MemoryStream())
                    {
                        cryptoStream.CopyTo(outputStream);
                        return outputStream.ToArray();
                    }
                }
            }
        }
    }
}
