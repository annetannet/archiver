namespace ConsoleApp2;

public class TrieNode
{
    public int Number { get; set; } = -1;
    public Dictionary<byte, TrieNode> Children { get; } = new();
}