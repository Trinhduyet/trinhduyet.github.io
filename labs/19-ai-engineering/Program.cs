using AiEngineeringLab;

string command = args.FirstOrDefault()?.Trim().ToLowerInvariant() ?? "demo";

if (command.StartsWith("meai-", StringComparison.Ordinal))
{
    return await MeaiLab.RunAsync(args);
}

return await LabCli.RunAsync(args);
