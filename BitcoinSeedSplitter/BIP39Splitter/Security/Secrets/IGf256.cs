namespace Security.Secrets
{
    public interface IGf256
    {
        byte[] Generate(int degree, byte input);

        byte Evaluate(byte[] polynomial, byte part);

        byte Interpolate(byte[][] points);
    }
}