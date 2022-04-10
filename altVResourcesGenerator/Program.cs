using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace altVResourcesGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Alt:V resources generator");
            Console.WriteLine("Generating...");
            
            var config = new Config<MainConfig>();
            if (config.Entries.ServerDirectory is null || !File.Exists(Path.Combine(config.Entries.ServerDirectory, "server.cfg")))
            {
                Console.WriteLine("Server.cfg does not exists in config.json path. Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            
            var parser = new ServerCfgParser(config.Entries.ServerDirectory);
            parser.ParseFile();
            
            var oldResources = (List<string>)parser.GetParameter("resources");
            var newResources = oldResources.Concat(GetResources(oldResources)).ToList();
            
            parser.SetParameter("resources", newResources);
            parser.SaveServerConfig();
            
            Console.WriteLine("Generated... \nPress any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static List<string> GetResources(List<string> oldResources)
        {
            var config = new Config<MainConfig>();
            if (config.Entries.ServerDirectory is null)
            {
                Console.WriteLine($"Server.cfg does not exists in config.json path. Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            var directory = new DirectoryInfo(Path.Combine(config.Entries.ServerDirectory, "resources"));
            var subDirectories = directory.GetDirectories();

            return (from dir in subDirectories where !oldResources.Contains(dir.Name) && dir.Name[0] != '.' select dir.Name).ToList();
        }
    }
}