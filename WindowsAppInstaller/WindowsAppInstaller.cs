using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;


string userProfilePath =
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string currentDirectory = Directory.GetCurrentDirectory();
string folderName = "WindowsCachClean";
string installPath = $"C:\\Program Files\\{folderName}";
string installShortcutPath = $"{userProfilePath}\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs";
string sourceFilePath = $"{currentDirectory}\\SetupFile";
string exeFilteName = $"{sourceFilePath}\\{folderName}.exe";

Console.WriteLine("Please wait program has started...");
Console.WriteLine(Environment.NewLine, Environment.NewLine);

if (RequireAdministrator())
{
    await Task.Run(() => InstallApp());
    await Task.Run(() => SetAdminstration());
    await Task.Run(() => CreateShortcut());

    Console.WriteLine("Install Successfully...");
}
else
{
    Console.WriteLine("Please run to adminstration...");
}

Console.WriteLine(Environment.NewLine, Environment.NewLine);
Console.WriteLine("Press any key to close the console window...");
Console.ReadKey();

[DllImport("libc")]
static extern uint getuid();

bool RequireAdministrator()
{
    string name = System.AppDomain.CurrentDomain.FriendlyName;
    try
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    return false;
                }
            }
        }
        else if (getuid() != 0)
        {
            return false;
        }
    }
    catch (Exception ex)
    {
        throw new ApplicationException("Unable to determine administrator or root status", ex);
    }

    return true;
}

void InstallApp()
{
    try
    {
        if (!Directory.EnumerateFiles(sourceFilePath).Any())
            return;

        if (!Directory.Exists(installPath))
        {
            Directory.CreateDirectory(installPath);
        }

        File.Copy(sourceFilePath, installPath, true);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

void CreateShortcut()
{
    try
    {
        // Create a WshShell object
        var shell = new WshShell();

        // Create a new shortcut
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(installShortcutPath);

        // Set the properties of the shortcut
        shortcut.TargetPath = installPath;
        shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(installPath); // Set working directory
        shortcut.Arguments = ""; // Set any command-line arguments here
        shortcut.Description = "My Shortcut"; // Set a description for the shortcut
        shortcut.IconLocation = installPath; // Set the icon for the shortcut

        // Set to run as administrator
        shortcut.Save();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(installShortcutPath);
        startInfo.UseShellExecute = true;
        startInfo.Verb = "runas"; // This will run the shortcut as administrator

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

void SetAdminstration()
{
    try
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(exeFilteName);
        startInfo.UseShellExecute = true;
        startInfo.Verb = "runas"; // This will run the executable as administrator

        Process.Start(startInfo);

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}