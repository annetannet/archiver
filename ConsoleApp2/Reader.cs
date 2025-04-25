namespace ConsoleApp2;

public class Reader
{
    private Stream stream;
    private int bitsRemainderInCurrentByte;
    private int currentByte;

    public Reader(byte[] input)
    {
        stream = new MemoryStream(input);
    }

    public int BitsToRead { get; set; }

    public int ReadNumber()
    {
        var num = 0;
        for (var i = 0; i < BitsToRead && num != -1; i++)
        {
            if (bitsRemainderInCurrentByte == 0)
            {
                currentByte = stream.ReadByte();
                bitsRemainderInCurrentByte = 8;
            }

            if (currentByte == -1)
            {
                num = -1;
            }
            else
            {
                num <<= 1;
                num |= (currentByte >> (bitsRemainderInCurrentByte - 1)) & 0x1;
                bitsRemainderInCurrentByte--;
            }
        }

        return num;
    }
}