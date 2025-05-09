﻿namespace ConsoleApp2;

public static class Program
{
    public static void Main()
    {
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        var consoleAppDir = currentDir.Parent!.Parent!.Parent;
        var filePath = Path.Combine(consoleAppDir!.FullName, "1.txt");

        var data = File.ReadAllBytes(filePath);

        var encoded = LZW.Encode(data);
        var decoded = LZW.Decode(encoded);

        for (var i = 0; i < data.Length; i++)
        {
            if (decoded[i] != data[i])
                Console.WriteLine($"ERROR: {decoded[i]} != {data[i]}");
        }

        // Получилось почти в 2 раза меньше, чем исходный текст при размере словаря 4096

        Console.WriteLine($"Длина исходного текста: {data.Length:n0} bytes."); // 575 010 bytes
        Console.WriteLine($"Результат сжатия: {encoded.Length:n0} bytes.\n"); // 298 471 bytes

        TestDictSizes(data);
    }

    private static void TestDictSizes(byte[] data)
    {
        var reversedData = data.Reverse().ToArray();
        var _2K = (int)Math.Pow(2, 11);
        var _64K = (int)Math.Pow(2, 16);

        Console.WriteLine("Размер словаря | Коэффициент сжатия слева направо | Коэффициент сжатия справа налево |");

        for (var i = _2K; i <= _64K; i *= 2)
        {
            var encoded = LZW.Encode(data, i);
            var encodedReversed = LZW.Encode(reversedData, i);
            Console.WriteLine(
                $"{i,14} | {(float)data.Length / encoded.Length,32} | {(float)reversedData.Length / encodedReversed.Length,32} |");
        }

        Console.WriteLine("\nВывод: увеличение размера словаря улучшает сжатие, но с убывающей отдачей." +
                          "\nБóльший словарь позволяет запомнить больше последовательностей, в том числе более длинные повторяющиеся последовательности." +
                          "\nПри росте словаря от 2048 до 4096 коэффициент сжатия вырос на ~0.21." +
                          "\nПри росте от 32768 до 65536 прирост составил лишь ~0.12." +
                          "\nДля текста справа налево сжатие сработало лучше на небольших размерах словаря." +
                          "\nМожет потому, что окончания слов чаще совпадают, чем начала слов." +
                          "\nНапример, часто в конце предложения идёт точка.");
    }
}