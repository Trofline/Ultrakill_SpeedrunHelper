using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DLLInjector
{
    class Program
    {
        static void Main(string[] args)
        {
            // Fenster-Titel anpassen und Cursor verstecken für cleanen Look
            Console.Title = "SpeedrunHelper - Injector v1.0";
            Console.CursorVisible = false;

            // 1. ASCII Art Logo (Premium Feeling)
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(@"
  _____ ___  ___  ___ _    ___ _  _ ___   ___ _      _   ___ _  __
 |_   _| _ \/ _ \| __| |  |_ _| \| | __| | _ ) |    /_\ / __| |/ /
   | | |   | (_) | _|| |__ | || .` | _|  | _ \ |__ / _ \ (__| ' < 
   |_| |_|_|\___/|_| |____|___|_|\_|___| |___/____/_/ \_\___|_|\_\
            ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("              [ VERSION 1.0 - ULTRAKILL EDITION ]\n");

            // 2. DYNAMISCHE PFADE DEFINIEREN (Standalone Modus!)
            // Das Tool sucht jetzt exakt in dem Ordner, aus dem es gestartet wird.
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dllPath = Path.Combine(baseDirectory, "ModulesDLL.dll");
  
            string smiPath = Path.Combine(baseDirectory, "smi.exe");
            string processName = "ULTRAKILL";

            // 3. Fake Boot-Sequenz für die Ästhetiks
            SimulateLoading("Initializing Core Modules", 15);
            SimulateLoading("Preparing SharpMonoInjector", 5);
            Console.WriteLine();

            // 4. Prozess-Suche
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[~] Scanning memory for target process '");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(processName);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("'...");
            Thread.Sleep(800);

            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
            {
                ShowError("Target process not found. Please launch the game first.");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[+] Process found! (PID: {processes[0].Id})");
            Thread.Sleep(500);

            // Check, ob die eigene Cheat-DLL im Ordner liegt
            if (!File.Exists(dllPath))
            {
                ShowError($"Payload not found!\nBitte stelle sicher, dass 'ModulesDLL.dll' im selben Ordner wie der Injector liegt.");
                return;
            }

            // Check, ob die smi.exe im Ordner liegt
            if (!File.Exists(smiPath))
            {
                ShowError($"Injector-Engine not found!");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[+] Engine secured. Preparing injection sequence...\n");
            Thread.Sleep(800);

            // 5. Der animierte Ladebalken
            DrawProgressBar();

            // 6. Die eigentliche Injektion (unsichtbar im Hintergrund)
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[~] Executing native injection via Mono-Runtime...");

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = smiPath;
            psi.Arguments = $"inject -p ULTRAKILL -a \"{dllPath}\" -n \"TroflineMod\" -c Loader -m Init";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;

            try
            {
                Process smiProcess = Process.Start(psi);
                string output = smiProcess.StandardOutput.ReadToEnd();
                smiProcess.WaitForExit();

                // Auswertung
                if (output.Contains("Success") || output.Contains("successfully") || output.Contains("0x"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n==================================================");
                    Console.WriteLine(" [SUCCESS] SPEEDRUNHELPER HOOKED SUCCESSFULLY!");
                    Console.WriteLine("==================================================");
                }
                else
                {
                    ShowError("Injection failed. Engine response:\n" + output);
                }
            }
            catch (Exception ex)
            {
                ShowError("Critical execution error: " + ex.Message);
            }

            // 7. Beenden
            Console.ResetColor();
            Console.CursorVisible = true;
            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        // --- HILFSFUNKTIONEN FÜR DIE UI ---

        static void SimulateLoading(string task, int delay)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[*] {task} ");
            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(delay * 10);
                Console.Write(".");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" OK");
        }

        static void DrawProgressBar()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            int barLength = 40;
            for (int i = 0; i <= barLength; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("=");
                Thread.Sleep(20); // Geschwindigkeit des Balkens
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("] 100%");
        }

        static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[-] ERROR: " + message);
            Console.ResetColor();
            Console.CursorVisible = true;
            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }
}