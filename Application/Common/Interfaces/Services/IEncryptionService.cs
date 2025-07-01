using System.Text;

namespace TranSPEi_Cifrado.Domain.Interfaces.Services;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    bool IsEncrypted(string text);


}
