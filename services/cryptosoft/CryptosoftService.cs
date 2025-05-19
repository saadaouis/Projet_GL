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

        public static async Task<int> Encrypt(string inputPath)
        {
            try
            {
                // Load the assembly
                var assembly = Assembly.LoadFrom(CryptoSoftPath);
                
                // Create the arguments array
                string[] args = new string[] { "encrypt", inputPath };
                
                // Get the Program type
                Type programType = assembly.GetType(Name)!;
                
                // Get the Main method
                MethodInfo mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static)!;
                
                // Invoke the Main method
                var result = await (Task<int>)mainMethod.Invoke(null, new object[] { args })!;
                
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while using CryptoSoft: {ex.Message}");
                return -1;
            }
        }
        
        public static async Task<int> Decrypt(string inputPath)
        {
            try
            {
                // Load the assembly
                var assembly = Assembly.LoadFrom(CryptoSoftPath);
                
                // Create the arguments array
                string[] args = new string[] { "decrypt", inputPath };
                
                // Get the Program type
                Type programType = assembly.GetType("CryptoSoft.Program")!;
                
                // Get the Main method
                MethodInfo mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static)!;
                
                // Invoke the Main method
                var result = await (Task<int>)mainMethod.Invoke(null, new object[] { args })!;
                
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while using CryptoSoft: {ex.Message}");
                return -1;
            }
        }
    }
}