using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using EvaJimaSettings;
using Newtonsoft.Json;

namespace udater
{
    class Program
    {

        static void Main1(string[] args)
        {
            var fileName = "EvaJima_2003.exe";

            try
            {
                string process = fileName.Replace(".exe", "");

                Console.WriteLine("Terminate process!");
                while (Process.GetProcessesByName(process).Length > 0)
                {
                    Process[] myProcesses2 = Process.GetProcessesByName(process);
                    for (int i = 1; i < myProcesses2.Length; i++) { myProcesses2[i].Kill(); }

                    Thread.Sleep(300);
                }

                //if (File.Exits(args[1])) { File.Delete(args[1]); }

                //File.Move(fileName, args[0]);

                Console.WriteLine("Starting " + fileName);
                Process.Start(fileName);
            }
            catch (Exception) { }
        }


        static void Main(string[] args)
        {
            var fileName = "EvaJima_2008.exe";

            var Server_update_uri_version = "http://evejima.mikotaj.com/Version.txt";
            var Server_update_content_version = "http://evejima.mikotaj.com/VersionContent.txt";

            //var Settings = new Settings();

            string process = fileName.Replace(".exe", "");

            Console.WriteLine("Terminate process!");
            while (Process.GetProcessesByName(process).Length > 0)
            {
                Process[] myProcesses2 = Process.GetProcessesByName(process);
                for (int i = 1; i < myProcesses2.Length; i++) { myProcesses2[i].Kill(); }

                Thread.Sleep(300);
            }

            Console.WriteLine("Start update Evajima application");
            File.AppendAllText(@"updater_log.txt",  DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Start update Evajima application" + Environment.NewLine);
            Thread.Sleep(500);
            Console.WriteLine("Connecting to server.");
            Thread.Sleep(500);
            Console.WriteLine("Connecting to server..");
            Thread.Sleep(500);
            Console.WriteLine("Connecting to server...");


            if (File.Exists("EvajimaBase.dll"))
            {
                Console.WriteLine("Start delete EvajimaBase.dll");
                File.AppendAllText(@"updater_log.txt",  DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Start delete EvajimaBase.dll" + Environment.NewLine);
                File.Delete("EvajimaBase.dll");
                Console.WriteLine("Delete EvajimaBase.dll successfully");
                File.AppendAllText(@"updater_log.txt",  DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Delete EvajimaBase.dll successfully" + Environment.NewLine);
            }

            var client = new WebClient();


            Console.WriteLine("File downloaded successfully");
            File.AppendAllText(@"updater_log.txt",  DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "File downloaded successfully" + Environment.NewLine);

            Console.WriteLine("Get version number from server");
            File.AppendAllText(@"updater_log.txt",  DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Get version number from server" + Environment.NewLine);

            var version = client.DownloadString(Server_update_uri_version);

            Console.WriteLine("Version number is " + version);
            File.AppendAllText(@"updater_log.txt",  DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Version number is " + version + Environment.NewLine);


            var versionContent = client.DownloadString(Server_update_content_version);

            Console.WriteLine("Version content is " + versionContent);
            File.AppendAllText(@"updater_log.txt", DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Version content is " + versionContent + Environment.NewLine);


            foreach (var file in GetVersionContent(versionContent).Files)
            {
                try
                {
                    Console.WriteLine("Start download file " + file.Name + " from " + file.Address + "");
                    File.AppendAllText(@"updater_log.txt", DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Start download file " + file.Name + " from " + file.Address + "" + Environment.NewLine);

                    client.DownloadFile(file.Address, file.Name);

                    Console.WriteLine("File " + file.Name + " downloaded successfully");
                    File.AppendAllText(@"updater_log.txt", DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "File " + file.Name + " downloaded successfully" + Environment.NewLine);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Critical error in download and update file " + file.Name + ". Exception is " + ex.Message);
                    File.AppendAllText(@"updater_log.txt", DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Critical error in download and update file " + file.Name + ". Exception is " + ex + Environment.NewLine);

                }
            }

            try
            {
                //var startInfo = new ProcessStartInfo
                //{

                //    UseShellExecute = false,
                //    FileName = AppDomain.CurrentDomain.BaseDirectory + "EvaJima_2002.exe"
                //};

                try
                {
                   

                    //using (StreamWriter writetext = new StreamWriter("Version.txt"))
                    //{
                    //    writetext.WriteLine(version);
                    //}


                    File.AppendAllText(@"updater_log.txt",  DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Revrate version to " + version + Environment.NewLine);

                    File.AppendAllText(@"updater_log.txt",  DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Start EvaJima.exe" + Environment.NewLine);
                    // Start the process with the info we specified.
                    //using (Process exeProcess = Process.Start(startInfo))
                    //{

                    //}
                    Process.Start(fileName);
                    //var runner = new ProcessHelper();

                    //runner.StartProcess(startInfo);

                    File.AppendAllText(@"updater_log.txt",  DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Start EvaJima.exe successfully" + Environment.NewLine);
                }
                catch (Exception ex2)
                {
                    File.AppendAllText(@"updater_log.txt",  DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Error: " + ex2 + Environment.NewLine);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                File.AppendAllText(@"updater_log.txt",  DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + " " + "Error: " + e + Environment.NewLine);
            }

            

            //Environment.Exit(0);
        }

        //private static void TestSerialization()
        //{
        //    var content = new VersionContent();

        //    content.Files.Add(new VersionFile { Address = "https://s3-us-west-2.amazonaws.com/cage-vitaly-test/2017-01-20/1060882024202267.zip", Name = "EvajimaBase.dll" });

        //    content.Files.Add(new VersionFile { Address = "https://s3-us-west-2.amazonaws.com/cage-vitaly-test/2017-01-20/1060882024202268.zip", Name = "VersionToken.txt" });

        //    var json = JsonConvert.SerializeObject(content);

        //    File.WriteAllText(@"VersionContent.txt", json);

        //    var v = GetVersionContent(File.ReadAllText(@"VersionContent.txt"));
        //}

        private static VersionContent GetVersionContent(string json)
        {
            var result = JsonConvert.DeserializeObject<VersionContent>(json);

            return result;
        }
    }
}
