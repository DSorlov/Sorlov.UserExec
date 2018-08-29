using System;
using System.Diagnostics;
using System.Security;

namespace Sorlov.Windows.UserExec
{
    class Program
    {
        static SecureString StringToSecureString(string input)
        {
            var password = new SecureString();
            foreach (char c in input)
                password.AppendChar(c);

            return password;
        }

        static void PrintHelp()
        {
            Console.WriteLine("UserExec by Daniel Sörlöv, daniel@sorlov.com, all rights reserved 2010-2013.");
            Console.WriteLine();
            Console.WriteLine("USEREXEC USAGE:");
            Console.WriteLine();
            Console.WriteLine("USEREXEC /u:<username> /p:<password> /c:<command> ");
            Console.WriteLine("         [ /l | /h | /m | /x | /a:<arguments> ]");
            Console.WriteLine();
            Console.WriteLine("USEREXEC /e:<string to encrypt>");
            Console.WriteLine();
            Console.WriteLine("   /l   Causes the users profile to be loaded");
            Console.WriteLine("   /h   Hides the requested application");
            Console.WriteLine("   /s   Starting directory for execution");
            Console.WriteLine("   /m   Runs the command minimized");
            Console.WriteLine("   /x   Use encrypted command line");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("> userexec /l /u:mydomain\\administrator /p:mypass /c:\"cmd.exe\"");
            Console.WriteLine("> userexec /e:\"<string to encrypt>\"");
            Console.WriteLine("> userexec /h /u:mydomain\\administrator /p:mypass /c:\"cmd.exe\" /a:\"/c auto.bat\"");
            Console.WriteLine();
            Environment.Exit(0);
        }

        static void ExitError(int code, string message)
        {
            Console.WriteLine(message);
            Environment.Exit(code);
        }

        static void Main(string[] args)
        {
            // FIND ALL ELEMENTS IN COMMANDLINE
            var cmdArg = new CommandlineArguments(args);

            // IF ENCRYPTED COMMAND USE THAT INSTEAD
            if (cmdArg["x"] != string.Empty)
            {
                string[] decryptedCommands = Crypto.Crypt(cmdArg["x"], "RunasEx1", false).Split();
                cmdArg = new CommandlineArguments(decryptedCommands);
            }

            // PRINT HELP AND EXIT
            if (cmdArg["?"] == "true" || args.Length == 0)
                PrintHelp();

            if (cmdArg["e"] != string.Empty)
                ExitError(0, Crypto.Crypt(cmdArg["e"], "RunasEx1", true));

            // READ COMMANDLINE OPTIONS
            string userName = cmdArg["u"];
            string userPassword = cmdArg["p"];
            string cmdCommand = cmdArg["c"];
            string cmdArgs = cmdArg["a"];
            string cmdStart = cmdArg["s"];

            // SET DEFAULT VALUES FOR SETTINGS
            string userDomain = string.Empty;
            bool loadUserProfile = false;
            var startStyle = ProcessWindowStyle.Normal;

            // CHECK USERNAME AND PASSWORD
            if (userName == string.Empty || userPassword == string.Empty)
                ExitError(-100, "UserExec error: Username and password must be specified.");

            // CHECK COMMAND
            if (cmdCommand == string.Empty)
                ExitError(-100, "UserExec error: Command must be specified.");

            // CHECK IF LOAD PROFILE
            if (cmdArg["l"] == "true")
                loadUserProfile = true;

            // CHECK IF START MINIMIZED
            if (cmdArg["m"] == "true")
                startStyle = ProcessWindowStyle.Minimized;

            // CHECK IF START HIDDEN
            if (cmdArg["h"] == "true")
                startStyle = ProcessWindowStyle.Hidden;

            //CHECK IF STARTDIRECTORY SPECIFIED
            if (cmdStart == string.Empty)
                cmdStart = Environment.CurrentDirectory;

            // CHECK IF TRADITIONAL STYLE USERNAME AND SPLIT
            if (userName.IndexOf(@"\", StringComparison.Ordinal) > 0)
            {
                string[] splitUser = userName.Split('\\');
                userDomain = splitUser[0];
                userName = splitUser[1];
            }

            // CHECK IF UPN STYLE USERNAME IS SPECIFIED AND SPLIT
            if (userName.IndexOf("@", StringComparison.Ordinal) > 0)
            {
                string[] splitUser = userName.Split('@');
                userName = splitUser[0];
                userDomain = splitUser[1];
            }

            // CREATE A NEW PROCESS AND SET VALUES
            var elevatedProcess = new ProcessStartInfo();
            elevatedProcess.WorkingDirectory = cmdStart;
            elevatedProcess.FileName = cmdCommand;
            elevatedProcess.UserName = userName;
            elevatedProcess.Password = StringToSecureString(userPassword);
            elevatedProcess.LoadUserProfile = loadUserProfile;
            elevatedProcess.WindowStyle = startStyle;
            elevatedProcess.UseShellExecute = false;
            elevatedProcess.Verb = "runas";
            //elevatedProcess.RedirectStandardError = true;
            //elevatedProcess.RedirectStandardInput = true;
            //elevatedProcess.RedirectStandardOutput = true;
            if (userDomain != string.Empty) elevatedProcess.Domain = userDomain;
            if (cmdArgs != string.Empty) elevatedProcess.Arguments = cmdArgs;

            // TRY TO LAUNCH AND EXIT IF ERROR
            try
            {
                Process.Start(elevatedProcess);
            }
            catch (Exception e)
            {
                ExitError(-255, "UserExec error: " + e.Message);
            }

            // DEFAULT EXIT
            // NOT NEEDED BUT LOOKS NICE
            Environment.Exit(0);
        }
    }
}
