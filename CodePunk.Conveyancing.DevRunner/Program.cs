using System.Diagnostics;

static Process StartProcess(string fileName, string workingDir, IDictionary<string,string?> env, params string[] args)
{
    var psi = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workingDir,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    foreach (var kv in env)
        psi.Environment[kv.Key] = kv.Value;
    foreach (var a in args)
        psi.ArgumentList.Add(a);

    var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
    p.OutputDataReceived += (_, e) => { if (e.Data is not null) Console.WriteLine(e.Data); };
    p.ErrorDataReceived += (_, e) => { if (e.Data is not null) Console.Error.WriteLine(e.Data); };
    p.Start();
    p.BeginOutputReadLine();
    p.BeginErrorReadLine();
    return p;
}

Console.WriteLine("Starting API and Portal (non-Aspire dev runner)...");

var repoRoot = Directory.GetCurrentDirectory();

var apiDir = Path.Combine(repoRoot, "src", "CodePunk.Conveyancing.Api");
var portalDir = Path.Combine(repoRoot, "apps", "portal");

if (!Directory.Exists(apiDir) || !Directory.Exists(portalDir))
{
    Console.Error.WriteLine("Could not locate API or Portal directories. Run from repo root.");
    return 1;
}

var apiEnv = new Dictionary<string,string?>
{
    ["ASPNETCORE_URLS"] = "http://localhost:5228"
};

var portalEnv = new Dictionary<string,string?>
{
    ["NEXT_PUBLIC_API_BASE"] = "http://localhost:5228/api"
};

var api = StartProcess("dotnet", apiDir, apiEnv, "run");
var portal = StartProcess("npm", portalDir, portalEnv, "run", "dev");

Console.WriteLine("\nAPI:    http://localhost:5228\nPortal: http://localhost:3000\nPress Ctrl+C to stop.\n");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (TaskCanceledException) { }

void TryKill(Process p)
{
    try { if (!p.HasExited) p.Kill(entireProcessTree: true); } catch { }
}

TryKill(portal);
TryKill(api);

Console.WriteLine("Stopped.");
return 0;

