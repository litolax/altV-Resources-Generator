using Tomlyn;
using Tomlyn.Model;

namespace altVResourcesGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Alt:V resources generator");
            Console.WriteLine("Generating...");

            var config = new Config<MainConfig>();
            if (config.Entries.ServerDirectory is null ||
                !File.Exists(Path.Combine(config.Entries.ServerDirectory, "server.toml")))
            {
                Console.WriteLine("Server.cfg does not exists in config.json path. Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            var path = Path.Combine(config.Entries.ServerDirectory, "server.toml");

            var model = Toml.ToModel(File.ReadAllText(path));
            var resources = (TomlArray)model["resources"];

            var newResources = resources.Concat(GetResources(resources)).ToList();

            model["resources"] = newResources;

            File.WriteAllText(path, Toml.FromModel(model));

            Console.WriteLine("Generated... \nPress any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        private static IEnumerable<string> GetResources(TomlArray oldResources)
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

            return (from dir in subDirectories
                where !oldResources.Contains(dir.Name) && dir.Name[0] != '.'
                select dir.Name).ToList();
        }
    }
}