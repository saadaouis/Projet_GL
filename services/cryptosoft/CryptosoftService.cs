// <copyright file="CryptosoftService.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using CryptoSoftLib;

namespace CryptoSoftService
{
    /// <summary>
    /// Service for encrypting and decrypting files using CryptoSoftLib.
    /// </summary>
    public class CryptosoftService
    {
        private readonly CryptoService cryptoService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptosoftService"/> class.
        /// </summary>
        public CryptosoftService()
        {
            this.cryptoService = new CryptoService();
        }

        /// <summary>
        /// Encrypts a file.
        /// </summary>
        /// <param name="path">The path to the file to encrypt.</param>
        /// <returns>The encrypted file path.</returns>
        public async Task<string> Encrypt(string path)
        {
            int result = await this.cryptoService.EncryptFileAsync(path);
            switch (result)
            {
                case 0:
                    return "File encrypted successfully";
                case -1:
                    return "Invalid Argument Count";
                case -2:
                    return "Invalid Operation";
                case -3:
                    return "File not found";
                default:
                    return "File encryption failed";
            }
        }

        /// <summary>
        /// Decrypts a file.
        /// </summary>
        /// <param name="path">The path to the file to decrypt.</param>
        /// <returns>The decrypted file path.</returns>
        public async Task<string> Decrypt(string path)
        {
            int result = await this.cryptoService.DecryptFileAsync(path);
            switch (result)
            {
                case 0:
                    return "File decrypted successfully";
                case -1:
                    return "Invalid Argument Count";
                case -2:
                    return "Invalid Operation";
                case -3:
                    return "File not found";
                default:
                    return "File decryption failed";
            }
        }
    }
}
