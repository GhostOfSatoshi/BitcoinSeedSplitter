using System.Collections.Generic;

namespace Security.Secrets
{
    public interface IShamirSecretShare
    {
        IEnumerable<byte[]> Split(byte[] secret, int parts, int minimum);

        byte[] Join(IEnumerable<byte[]> parts);
    }
}