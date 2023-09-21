using IWshRuntimeLibrary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;


string userProfilePath =
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string currentDirectory = Directory.GetCurrentDirectory();
string folderName = "WindowsCachClean";
string installPath = $"C:\\Program Files\\{folderName}";
string installShortcutPath = "Programs";
string sourceFilePath = $"{currentDirectory}\\SetupFile";
string exeFilteName = $"{sourceFilePath}\\{folderName}.exe";
string shortCutTargetPath = $"{installPath}\\{folderName}.exe";

Console.WriteLine("Please wait program has started...");
Console.WriteLine(Environment.NewLine, Environment.NewLine);

if (RequireAdministrator())
{
    if (await programRun())
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

async Task<bool> programRun()
{
    bool isResult = false;
    bool isInstallApp = await Task.Run(() => InstallApp());
    bool isSetAdminstration = await Task.Run(() => SetAdminstration(exeFilteName));
    bool isCreateShortcut = await Task.Run(() => CreateShortcut(installShortcutPath, shortCutTargetPath, folderName));

    if (isInstallApp && isSetAdminstration && isCreateShortcut)
        isResult = true;

    return isResult;
}

bool InstallApp()
{
    bool isResult = false;
    try
    {
        if (!Directory.EnumerateFiles(sourceFilePath).Any())
            return isResult;

        if (!Directory.Exists(installPath))
        {
            Directory.CreateDirectory(installPath);
        }

        string[] files = Directory.GetFiles(sourceFilePath);
        Array.ForEach(files, file =>
        {
            string fileName = Path.GetFileName(file);
            string destinationPath = Path.Combine(installPath, fileName);
            System.IO.File.Copy(file, destinationPath, true);
        });

        isResult = true;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

    return isResult;
}

bool CreateShortcut(string destinationDirectoryName, string targetPath, string fileName)
{
    bool isResult = false;
    try
    {
        object shpath = (object)destinationDirectoryName;
        var shell = new WshShell();
        string shortcutAddress = (string)shell.SpecialFolders.Item(ref shpath) + @"\" + fileName + ".lnk";
        // Create a shortcut
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
        shortcut.TargetPath = targetPath; // Replace with the path to the target file or program
        shortcut.WorkingDirectory = shortcutAddress; // Optional: set the working directory
        //shortcut.Description = "My Shortcut"; // Optional: set a description for the shortcut
        //shortcut.IconLocation = @"C:\Path\To\Your\Icon.ico"; // Optional: set the icon for the shortcut
        shortcut.Save();

        isResult = true;

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

    return isResult;
}

bool SetAdminstration(string fileName)
{
    bool isResult = false;
    try
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(fileName);
        startInfo.UseShellExecute = true;
        startInfo.Verb = "runas"; // This will run the executable as administrator

        Process.Start(startInfo);

        isResult = true;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

    return isResult;
}
