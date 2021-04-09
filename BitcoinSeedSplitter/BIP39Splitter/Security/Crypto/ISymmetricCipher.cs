namespace Security.Crypto
{
    public interface ISymmetricCipher
    {
        byte[] Encrypt(byte[] secret, byte[] key, byte[] iv, byte[] salt);

        byte[] Decrypt(byte[] cipherText, byte[] key, byte[] iv, byte[] salt);
    }
}