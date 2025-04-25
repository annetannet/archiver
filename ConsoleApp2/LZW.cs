namespace ConsoleApp2;

public static class LZW
{
    public static byte[] Encode(byte[] input, int maxDictSize = 4096)
    {
        var output = new List<byte> { 0 };
        int bits = 9, currBit = 0;

        // инициализация дерева
        var root = new TrieNode();
        for (var i = 0; i < 256; i++)
        {
            root.Children[(byte)i] = new TrieNode { Number = i };
        }

        var w = root; // w - это строка с предыдущей итерации
        var nextCode = 256;

        foreach (byte a in input)
        {
            // если wa есть в словаре, то w = wa
            if (w.Children.TryGetValue(a, out var nextNode))
            {
                w = nextNode;
                continue;
            }

            // добавление номера w к output
            WriteBits(output, w.Number, bits, ref currBit);

            // добавление wa в словарь
            if (nextCode < maxDictSize)
            {
                var newNode = new TrieNode { Number = nextCode++ };
                w.Children[a] = newNode;
            }

            // w = a
            w = root.Children[a];

            // увеличение числа бит на номер при необходимости
            if (nextCode >= 1 << bits)
            {
                bits++;
            }
        }

        WriteBits(output, w.Number, bits, ref currBit);

        return output.ToArray();
    }

    private static void WriteBits(List<byte> output, int code, int bits, ref int currBit)
    {
        for (var i = bits - 1; i >= 0; i--)
        {
            var bit = (code >> i) & 1;
            output[^1] |= (byte)(bit << (7 - currBit));
            if (++currBit >= 8)
            {
                output.Add(0);
                currBit = 0;
            }
        }
    }

    public static byte[] Decode(byte[] bytes, int maxDictSize = 4096)
    {
        var input = new Reader(bytes) { BitsToRead = 9 };
        var output = new List<byte>();

        // инициализация словаря
        var dict = new List<List<byte>>();
        for (var i = 0; i < 256; i++)
            dict.Add([(byte)i]);

        var prevCode = input.ReadNumber();
        if (prevCode == -1) return [];
        var w = dict[prevCode]; // w - строка, взятая из словаря на предыдущем шаге
        output.AddRange(w);

        while (true)
        {
            var number = input.ReadNumber();
            if (number == -1) break;

            List<byte> v;
            if (number < dict.Count)
            {
                v = dict[number];
            }
            else if (number == dict.Count)
            {
                // особый случай KwK
                v =
                [
                    ..w,
                    w[0]
                ];
            }
            else
            {
                throw new InvalidDataException("Invalid LZW code");
            }

            output.AddRange(v);

            // добавление новой последовательности w*v[0] в словарь
            if (dict.Count < maxDictSize)
            {
                var newEntry = new List<byte>(w);
                newEntry.Add(v[0]);
                dict.Add(newEntry);
            }

            w = v;

            // увеличение числа бит на номер при необходимости
            if (dict.Count >= (1 << input.BitsToRead) - 1)
                input.BitsToRead++;
        }

        return output.ToArray();
    }
}