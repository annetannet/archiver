namespace ConsoleApp2;

public class Reader
{
    private readonly byte[] data;
    private int position;
    private int bitPosition;
    public int BitsToRead { get; set; }

    public Reader(byte[] data) => this.data = data;

    public int ReadNumber()
    {
        int result = 0;
        for (int i = 0; i < BitsToRead; i++)
        {
            if (position >= data.Length) return -1;
            int bit = (data[position] >> (7 - bitPosition)) & 1;
            result = (result << 1) | bit;
            if (++bitPosition >= 8)
            {
                bitPosition = 0;
                position++;
            }
        }

        return result;
    }
}