namespace Security.CustomEncoding
{
    public static class Encoder
    {
        public static IEncoder Base16 { get; }

        static Encoder()
        {
            Base16 = new Base16Encoder();
        }
    }
}