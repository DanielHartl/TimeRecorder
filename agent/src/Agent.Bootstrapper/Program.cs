using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace TimeRecorder.Agent.Bootstrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            var baseTempPath = Path.Combine(Path.GetTempPath(), "tr");
            var zipDownloadFolder = Path.Combine(baseTempPath, "zip");
            var dllCachePath = Path.Combine(baseTempPath, "dllcache");

            foreach (var path in new[] { baseTempPath, zipDownloadFolder, dllCachePath })
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            var zipPath = Path.Combine(zipDownloadFolder, "TimeRecorder.Agent.zip");

            var zipAsBytes = await Download(new Uri("https://timerec.blob.core.windows.net/public/TimeRecorder.Agent.zip"));
            if (zipAsBytes != null)
            {
                File.WriteAllBytes(zipPath, zipAsBytes);
                Directory.Delete(dllCachePath, true);
                ZipFile.ExtractToDirectory(zipPath, dllCachePath);
            }

            Assembly programAssembly = null;

            var assemblies = new Dictionary<string, Assembly>();

            AssemblyLoadContext.Default.Resolving += (context, assemblyName) => assemblies[assemblyName.FullName];

            foreach (var assemblyPath in Directory.GetFiles(dllCachePath).Where(f => Path.GetExtension(f) == ".dll"))
            {
                var assembly = Assembly.LoadFile(assemblyPath);
                assemblies.Add(assembly.GetName().FullName, assembly);

                if (assembly.FullName == "TimeRecorder.Agent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")
                {
                    programAssembly = assembly;
                }
            }

            if (programAssembly != null)
            {
                var programType = programAssembly
                    .GetTypes()
                    .FirstOrDefault(t => t.FullName == "TimeRecorder.Agent.App.Program");

                if (programType != null)
                {
                    var mainMethodInfo = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
                    mainMethodInfo.Invoke(null, new[] { args });
                }
            }
        }

        private static async Task<byte[]> Download(Uri uri)
        {
            try
            {
                var httpClient = new HttpClient();
                return await httpClient.GetByteArrayAsync(uri);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Unable to download dlls {exception}");
                return null;
            }
        }
    }
}
