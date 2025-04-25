namespace ConsoleApp2;

public static class LZW
{
    public static byte[] Encode(byte[] data, int dictSize = 4096)
    {
        var result = new List<byte> { 0 };
        int bitsToEncode = 8, currBit = 0;

        var root = CompressionTrie.InitRoot();
        var currentNode = root;
        var code = 256;

        foreach (var @byte in data)
        {
            if (currentNode.Next.TryGetValue(@byte, out var nextNode))
            {
                currentNode = nextNode;
                continue;
            }

            if (code < dictSize)
                currentNode.Next[@byte] = CompressionTrie.InitTree(code++);
            if (code >= 1 << bitsToEncode)
                bitsToEncode++;
            for (var i = bitsToEncode - 1; i >= 0; i--)
            {
                var bit = ((currentNode.Code & 1 << i) == 0 ? 0 : 1);
                result[^1] |= (byte)(bit << (7 - currBit));
                currBit += 1;
                if (currBit >= 8)
                {
                    result.Add(0);
                    currBit %= 8;
                }
            }

            currentNode = root.Next[@byte];
        }

        if (code >= 1 << bitsToEncode)
            bitsToEncode++;
        for (var i = bitsToEncode - 1; i >= 0; i--)
        {
            var bit = (currentNode.Code & 1 << i) == 0 ? 0 : 1;
            result[^1] |= (byte)(bit << (7 - currBit));
            currBit += 1;
            if (currBit >= 8)
            {
                result.Add(0);
                currBit %= 8;
            }
        }

        return result.ToArray();
    }

    public static byte[] Decode(byte[] compressedData, int dictSize = 4096)
    {
        var reader = new Reader(compressedData)
        {
            BitsToRead = 9
        };

        var result = new List<byte>();

        var root = DecompressionTrie.InitRoot();
        var dict = new List<DecompressionTrie>(256);
        var code = 256;

        foreach (var (@byte, node) in root.Next)
            dict.Add(node);

        var currentNode = root;
        var number = reader.ReadNumber();
        while (number != -1)
        {
            if (number >= dict.Count)
            {
                var node = currentNode;
                while (node!.Parent != root)
                    node = node.Parent;
                var extensionNode = DecompressionTrie.InitTree(currentNode, node.Byte);
                currentNode.Next[node.Byte] = extensionNode;
                while (dict.Count <= number)
                    dict.Add(null!);
                dict[number] = extensionNode;
            }

            var str = new List<byte>();
            var pointer = dict[number];
            while (pointer != root)
            {
                str.Add(pointer!.Byte);
                pointer = pointer.Parent;
            }

            str.Reverse();
            result.AddRange(str);

            if (currentNode != root && code < dictSize)
            {
                var nextNode = DecompressionTrie.InitTree(currentNode, str[0]);
                currentNode.Next[str[0]] = nextNode;
                while (dict.Count <= code)
                    dict.Add(null!);
                dict[code] = nextNode;
                code++;
            }

            currentNode = dict[number];

            // code + 2, так как нужно учитывать задержку между чтением кода и добавлением его в словарь
            if (code + 2 >= 1 << reader.BitsToRead)
                reader.BitsToRead++;

            number = reader.ReadNumber();
        }

        return result.ToArray();
    }
}