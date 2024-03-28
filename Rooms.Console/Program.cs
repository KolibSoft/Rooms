namespace KolibSoft.Rooms.Console;

public static class Program
{

    public static string? Prompt(string? hint = "> ")
    {
        System.Console.Write(hint);
        var input = System.Console.ReadLine();
        return input;
    }

    public static string? GetArgument(this string[] args, string name, string? hint = null, bool required = false)
    {
        var argName = $"--{name}";
        string? argument = args.FirstOrDefault(x => x.StartsWith(argName) && (x.Length == argName.Length || x[argName.Length] == '='));
        if (argument != null)
        {
            if (argument.Length == argName.Length) return name;
            if (argument[argName.Length] == '=') return argument[(argName.Length + 1)..];
        }
        while (required && string.IsNullOrWhiteSpace(argument)) argument = Prompt(hint ?? $"{name}: ");
        return argument;
    }

    public static string? GetOption(this string[] args, string name, string[] options, string? hint = null, bool required = false)
    {
        string? option = args.GetArgument(name, hint, required);
        while (required && !options.Contains(option)) option = Prompt(hint ?? $"{name}: ");
        return option;
    }

    public static async Task Main(params string[] args)
    {
        var mode = args.GetOption("mode", ["Server", "Client"], null, true);
        if (mode == "Server")
        {

        }
        else if (mode == "Client")
        {

        }
    }

}