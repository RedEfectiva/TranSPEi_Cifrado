using System.Text;
using TranSPEi_Cifrado.Domain.Interfaces.Services;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
namespace TranSPEi_Cifrado.Application.Common.Services
{
    public class EncryptionService : IEncryptionService
    {
        private const int KeySizeBits = 256;
        private const int BlockSizeBytes = 16; // AES block size (128 bits)
        private const int SaltSizeBytes = 16;
        private const int Pbkdf2Iterations = 100000;
        private const string DefaultKey = "DefaultSecureKey1234567890123456"; // contraseña de 32 caracteres para AES-256
        private const string CipherPrefix = "ENC:";
        private readonly string _key;
        private static readonly Lazy<EncryptionService> _instance = new Lazy<EncryptionService>(() =>
        {
            string pwd = Environment.GetEnvironmentVariable("ENCRYPTION_PASSWORD") ?? DefaultKey;
            return new EncryptionService(pwd);
        }, true);

        // Propiedad para acceder a la instancia Singleton
        public static EncryptionService Instance => _instance.Value;

        private EncryptionService(string pwd)
        {
            if (string.IsNullOrEmpty(pwd))
                throw new ArgumentNullException(nameof(pwd), "La contraseña no puede ser nula o vacía.");
            _key =pwd;
        }

        public string Encrypt(string plainText)
        {
            ValidateInput(plainText, nameof(plainText));

            byte[] salt = GenerateRandomBytes(SaltSizeBytes);//Salt aleatoria
            byte[] iv = GenerateRandomBytes(BlockSizeBytes);// Vector de inicialización aletoria
            byte[] keyBytes = DeriveKey(_key, salt);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = ProcessCipher(plainBytes, keyBytes, iv, true);
            return CipherPrefix + Convert.ToBase64String(CombineBuffers(salt, iv, cipherBytes));
        }

        public string Decrypt(string cipherText)
        {
            ValidateInput(cipherText, nameof(cipherText));

            if (!cipherText.StartsWith(CipherPrefix))
                throw new ArgumentException("Texto cifrado no válido: falta prefijo");

            string base64Text = cipherText.Substring(CipherPrefix.Length);
            byte[] inputBytes;
            try
            {
                inputBytes = Convert.FromBase64String(base64Text);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Formato de texto cifrado no válido");
            }

            if (inputBytes.Length < SaltSizeBytes + BlockSizeBytes)
                throw new ArgumentException("Longitud de texto cifrado no válida");

            byte[] salt = new byte[SaltSizeBytes];
            byte[] iv = new byte[BlockSizeBytes];
            byte[] actualCipher = new byte[inputBytes.Length - SaltSizeBytes - BlockSizeBytes];

            Buffer.BlockCopy(inputBytes, 0, salt, 0, SaltSizeBytes);
            Buffer.BlockCopy(inputBytes, SaltSizeBytes, iv, 0, BlockSizeBytes);
            Buffer.BlockCopy(inputBytes, SaltSizeBytes + BlockSizeBytes, actualCipher, 0, actualCipher.Length);

            byte[] keyBytes = DeriveKey(_key, salt);
            byte[] decryptedBytes = ProcessCipher(actualCipher, keyBytes, iv, false);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public bool IsEncrypted(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.StartsWith(CipherPrefix))
                return false;

            try
            {
                byte[] bytes = Convert.FromBase64String(text.Substring(CipherPrefix.Length));
                return bytes.Length >= SaltSizeBytes + BlockSizeBytes + BlockSizeBytes;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static void ValidateInput(string input, string paramName)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(paramName);
        }

        private static byte[] DeriveKey(string key, byte[] salt)
        {
            var pbkdf2 = new Pkcs5S2ParametersGenerator();
            pbkdf2.Init(Encoding.UTF8.GetBytes(key), salt, Pbkdf2Iterations);
            return ((KeyParameter)pbkdf2.GenerateDerivedParameters("aes", KeySizeBits)).GetKey();
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            var rng = new Org.BouncyCastle.Security.SecureRandom();
            rng.NextBytes(bytes);
            return bytes;
        }

        private static byte[] ProcessCipher(byte[] input, byte[] key, byte[] iv, bool encrypt)
        {
            var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine()), new Pkcs7Padding());
            cipher.Init(encrypt, new ParametersWithIV(new KeyParameter(key), iv));

            byte[] output = new byte[cipher.GetOutputSize(input.Length)];
            int length = cipher.ProcessBytes(input, 0, input.Length, output, 0);
            length += cipher.DoFinal(output, length);

            if (!encrypt)
            {
                Array.Resize(ref output, length);
            }
            return output;
        }

        private static byte[] CombineBuffers(params byte[][] buffers)
        {
            int totalLength = 0;
            foreach (var buffer in buffers)
                totalLength += buffer.Length;

            byte[] result = new byte[totalLength];
            int offset = 0;
            foreach (var buffer in buffers)
            {
                Buffer.BlockCopy(buffer, 0, result, offset, buffer.Length);
                offset += buffer.Length;
            }
            return result;
        }
    }
}
