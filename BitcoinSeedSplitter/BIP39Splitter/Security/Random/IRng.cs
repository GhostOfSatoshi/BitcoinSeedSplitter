namespace Security.Random
{
    public interface IRng
    {
        void GetBytes(byte[] bytes);
    }
}