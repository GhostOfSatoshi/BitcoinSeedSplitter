using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Security.Crypto
{
    public class SymmetricCipher : ISymmetricCipher
    {
        public byte[] Encrypt(byte[] secret, byte[] key, byte[] iv, byte[] salt)
        {
            var cipherParameters = new AeadParameters(new KeyParameter(key), 128, iv, salt);
            var cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(true, cipherParameters);

            var cipherText = new byte[cipher.GetOutputSize(secret.Length)];

            var len = cipher.ProcessBytes(secret, 0, secret.Length, cipherText, 0);
            cipher.DoFinal(cipherText, len);

            return cipherText;
        }

        public byte[] Decrypt(byte[] cipherText, byte[] key, byte[] iv, byte[] salt)
        {
            var cipherParameters = new AeadParameters(new KeyParameter(key), 128, iv, salt);
            var cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(false, cipherParameters);

            var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];

            var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
            cipher.DoFinal(plainText, len);

            return plainText;
        }
    }
}