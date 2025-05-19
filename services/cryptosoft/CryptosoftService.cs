using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public static class CryptosoftService
    {
        const string CryptoSoftPath = "CryptoSoft.dll";
        private const string Name = "CryptoSoft.Program";

        private static void ValidateInputPath(string inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                throw new ArgumentException("Input path cannot be null or empty", nameof(inputPath));
            }

            if (!File.Exists(inputPath) && !Directory.Exists(inputPath))
            {
                throw new FileNotFoundException($"The specified path does not exist: {inputPath}");
            }
        }

        private static void ValidateCryptoSoftDll()
        {
            if (!File.Exists(CryptoSoftPath))
            {
                throw new FileNotFoundException($"CryptoSoft.dll not found at: {Path.GetFullPath(CryptoSoftPath)}");
            }
        }

        public static async Task<int> Encrypt(string inputPath)
        {
            try
            {
                // Validate inputs
                ValidateInputPath(inputPath);
                ValidateCryptoSoftDll();

                // Load the assembly
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(CryptoSoftPath);
                }
                catch (BadImageFormatException)
                {
                    throw new InvalidOperationException("CryptoSoft.dll is not a valid .NET assembly");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to load CryptoSoft.dll: {ex.Message}");
                }
                
                // Create the arguments array
                string[] args = new string[] { "encrypt", inputPath };
                
                // Get the Program type
                Type? programType = assembly.GetType(Name);
                if (programType == null)
                {
                    throw new InvalidOperationException($"Could not find type '{Name}' in CryptoSoft.dll");
                }
                
                // Get the Main method
                MethodInfo? mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
                if (mainMethod == null)
                {
                    throw new InvalidOperationException("Could not find Main method in CryptoSoft.Program");
                }
                
                // Invoke the Main method
                var result = await (Task<int>)mainMethod.Invoke(null, new object[] { args })!;
                
                return result;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Invalid argument: {ex.Message}");
                return -2;
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine($"File not found: {ex.Message}");
                return -3;
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine($"Operation failed: {ex.Message}");
                return -4;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error while using CryptoSoft: {ex.Message}");
                return -1;
            }
        }
        
        public static async Task<int> Decrypt(string inputPath)
        {
            try
            {
                // Validate inputs
                ValidateInputPath(inputPath);
                ValidateCryptoSoftDll();

                // Load the assembly
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(CryptoSoftPath);
                }
                catch (BadImageFormatException)
                {
                    throw new InvalidOperationException("CryptoSoft.dll is not a valid .NET assembly");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to load CryptoSoft.dll: {ex.Message}");
                }
                
                // Create the arguments array
                string[] args = new string[] { "decrypt", inputPath };
                
                // Get the Program type
                Type? programType = assembly.GetType(Name);
                if (programType == null)
                {
                    throw new InvalidOperationException($"Could not find type '{Name}' in CryptoSoft.dll");
                }
                
                // Get the Main method
                MethodInfo? mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
                if (mainMethod == null)
                {
                    throw new InvalidOperationException("Could not find Main method in CryptoSoft.Program");
                }
                
                // Invoke the Main method
                var result = await (Task<int>)mainMethod.Invoke(null, new object[] { args })!;
                
                return result;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Invalid argument: {ex.Message}");
                return -2;
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine($"File not found: {ex.Message}");
                return -3;
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine($"Operation failed: {ex.Message}");
                return -4;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error while using CryptoSoft: {ex.Message}");
                return -1;
            }
        }
    }
}