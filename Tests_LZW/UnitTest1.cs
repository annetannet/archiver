using System.Text;
using ConsoleApp2;
using FluentAssertions;

namespace Tests_LZW;

public class Tests
{
    [TestCase("Hello World!")]
    [TestCase("abracadabra")]
    [TestCase("BanBananana")]
    [TestCase("BanBanananananana")]
    [TestCase(" ")]
    [TestCase("        ")]
    [TestCase("ф")]
    public void Test_HelloWorld(string text)
    {
        var txtBytes = Encoding.UTF8.GetBytes(text);
        var res = LZW.Encode(txtBytes);

        var decoded = LZW.Decode(res);
        decoded.Should().BeEquivalentTo(txtBytes);
    }
}