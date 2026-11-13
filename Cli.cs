namespace FolderProfiler;

internal static class Cli
{
    public static void Title(string text)
    {
        Blank();
        Rule('=');
        Console.WriteLine(Center(text));
        Rule('=');
        Blank();
    }

    public static void Section(string text)
    {
        Blank();
        Rule('-');
        Console.WriteLine($"  {text}");
        Rule('-');
    }

    public static void Field(string label, string value)
    {
        //  Projeto........................ valor
        //  Perfil atual................... valor
        var left = $"  {label.TrimEnd('.', ' ')}";
        var valueColumn = Math.Max(22, LineWidth() - 18);
        var dots = Math.Max(1, valueColumn - left.Length);
        Console.WriteLine($"{left}{new string('.', dots)} {value}");
    }

    public static void Option(int number, string text)
    {
        Console.WriteLine($"  [{number}] {text}");
    }

    public static void Info(string text)
    {
        Console.WriteLine($"  {text}");
    }

    public static void Step(string text)
    {
        Console.WriteLine($"  > {text}");
    }

    public static void Path(string text)
    {
        Console.WriteLine($"      {text}");
    }

    public static void Success(string text)
    {
        Blank();
        Rule('-');
        Console.WriteLine($"  {text}");
        Rule('-');
    }

    public static void Error(string text)
    {
        Blank();
        Rule('-');
        Console.WriteLine($"  ERRO: {text}");
        Rule('-');
    }

    public static void Blank() => Console.WriteLine();

    public static void Rule(char ch)
    {
        Console.WriteLine(new string(ch, LineWidth()));
    }

    public static string? Prompt(string label)
    {
        Console.Write($"  {label} ");
        return Console.ReadLine()?.Trim();
    }

    public static void WaitForExit()
    {
        Blank();
        Console.WriteLine("  Pressione qualquer tecla para encerrar.");
        try
        {
            Console.ReadKey(intercept: true);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static string Center(string text)
    {
        var width = LineWidth();
        if (text.Length >= width)
            return text;

        var pad = (width - text.Length) / 2;
        return new string(' ', pad) + text;
    }

    private static int LineWidth()
    {
        try
        {
            return Math.Clamp(Console.WindowWidth - 1, 40, 56);
        }
        catch
        {
            return 48;
        }
    }
}
