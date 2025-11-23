using BppGrasp.Core.Models;

namespace BppGrasp.Core.IO;

public sealed class InstanceReader
{
    public static BppInstance ReadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Instance file not found: {filePath}");

        var lines = File.ReadAllLines(filePath)
            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.Trim().StartsWith("#"))
            .ToList();

        var name = Path.GetFileNameWithoutExtension(filePath);
        var lineIndex = 0;

        var firstLine = lines[lineIndex++].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var itemCount = int.Parse(firstLine[0]);
        var binCapacity = double.Parse(firstLine[1]);

        var instance = new BppInstance(name, binCapacity);

        for (int i = 0; i < itemCount && lineIndex < lines.Count; i++)
        {
            var itemLine = lines[lineIndex++].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var weight = double.Parse(itemLine[0]);
            instance.AddItem(new Item(i, weight));
        }

        if (lineIndex < lines.Count)
        {
            var precedenceCountLine = lines[lineIndex++].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var precedenceCount = int.Parse(precedenceCountLine[0]);

            for (int i = 0; i < precedenceCount && lineIndex < lines.Count; i++)
            {
                var precLine = lines[lineIndex++].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var itemA = int.Parse(precLine[0]);
                var itemB = int.Parse(precLine[1]);
                var minSep = int.Parse(precLine[2]);

                instance.AddPrecedence(new PrecedenceConstraint(itemA, itemB, minSep));
            }
        }

        return instance;
    }
}
