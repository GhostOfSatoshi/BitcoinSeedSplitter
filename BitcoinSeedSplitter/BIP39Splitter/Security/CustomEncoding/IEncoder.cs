namespace Security.CustomEncoding
{
    public interface IEncoder
    {
        string GetString(byte[] data);

        byte[] GetBytes(string data);
    }
}