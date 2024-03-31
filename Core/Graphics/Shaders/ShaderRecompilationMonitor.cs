using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public class ShaderRecompilationMonitor : ModSystem
    {
        internal static Queue<CompilingFile> CompilingFiles
        {
            get;
            private set;
        }

        internal static List<ShaderWatcher> ShaderWatchers
        {
            get;
            private set;
        } = [];

        public record ShaderWatcher(string EffectsPath, string CompilerPath, string ModName, FileSystemWatcher FileWatcher);

        public record CompilingFile(string FilePath, bool CompileAsFilter);

        public override void OnModLoad()
        {
            CompilingFiles = new();
            if (Main.netMode != NetmodeID.SinglePlayer)
                return;

            foreach (Mod mod in ModLoader.Mods)
                LoadForMod(mod);
        }

        private static void LoadForMod(Mod mod)
        {
            // Check to see if the user has a folder that corresponds to the shaders for this mod.
            // If this folder is not present, that means that they are not a developer and thusly this system would be irrelevant.
            string modSourcesPath = $"{Path.Combine(Program.SavePathShared, "ModSources")}\\{mod.Name}".Replace("\\..\\tModLoader", string.Empty);
            if (!Directory.Exists(modSourcesPath))
                return;

            // Verify that the Assets/AutoloadedEffects directory exists.
            string effectsPath = $"{modSourcesPath}\\Assets\\{ShaderManager.AutoloadDirectoryShaders.Replace("/", "\\")}";
            if (!Directory.Exists(effectsPath))
                return;

            // Verify that the mod has a compiler path.
            // Before you ask, no, this isn't something that should be built into the library.
            // Remember, this is checking local directories on the developer's computer, NOT within the mod file data.
            // Having the compiler be a part of the library would require that developers have a redundant local copy of the library's source.
            string compilerPath = $"{modSourcesPath}\\Assets\\AutoloadedEffects\\Compiler";
            if (!Directory.Exists(compilerPath))
                return;

            // If the Assets/AutoloadedEffects directory exists, watch over it.
            FileSystemWatcher watcher = new(effectsPath)
            {
                Filter = "*.fx",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security
            };
            watcher.Changed += RecompileShader;
            ShaderWatchers.Add(new(effectsPath, compilerPath, mod.Name, watcher));

            string filtersPath = effectsPath.Replace("\\Shaders", "\\Filters");
            if (Directory.Exists(filtersPath))
            {
                FileSystemWatcher filterWatcher = new(filtersPath)
                {
                    Filter = "*.fx",
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security
                };
                filterWatcher.Changed += RecompileShader;
                ShaderWatchers.Add(new(filtersPath, compilerPath, mod.Name, filterWatcher));
            }
        }

        private static void TryToWatchPath(Mod mod, string path, string compilerPath)
        {
            if (!Directory.Exists(path))
                return;

            FileSystemWatcher filterWatcher = new(path)
            {
                Filter = "*.fx",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security
            };
            filterWatcher.Changed += RecompileShader;
            ShaderWatchers.Add(new(path, compilerPath, mod.Name, filterWatcher));
        }

        private static void UpdateForModWatcher(ShaderWatcher watcher)
        {
            bool shaderIsCompiling = false;
            List<CompilingFile> compiledFiles = [];
            string compilerDirectory = watcher.CompilerPath + "//";
            while (true)
            {
                if (!CompilingFiles.TryPeek(out CompilingFile file))
                    break;

                // Take the contents of the new shader and copy them over to the compiler folder so that the XNB can be regenerated.
                string shaderPath = file.FilePath;
                string shaderPathInCompilerDirectory = compilerDirectory + Path.GetFileName(shaderPath);

                if (!shaderPath.Contains(watcher.ModName))
                    return;

                File.Delete(shaderPathInCompilerDirectory);
                try
                {
                    File.WriteAllText(shaderPathInCompilerDirectory, File.ReadAllText(shaderPath));
                }
                catch { }
                shaderIsCompiling = true;
                compiledFiles.Add(file);

                CompilingFiles.Dequeue();
            }

            if (compiledFiles.Count >= 10)
                return;

            if (shaderIsCompiling)
            {
                // Execute EasyXNB.
                Process easyXnb = new()
                {
                    StartInfo = new()
                    {
                        FileName = watcher.CompilerPath + "\\EasyXnb.exe",
                        WorkingDirectory = compilerDirectory,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                easyXnb.Start();
                if (!easyXnb.WaitForExit(3000))
                {
                    Main.NewText("Shader compiler timed out. Likely error.");
                    easyXnb.Kill();
                    return;
                }

                easyXnb.Kill();
            }

            for (int i = 0; i < compiledFiles.Count; i++)
            {
                // Copy over the XNB from the compiler, and delete the copy in the Compiler folder.
                var file = compiledFiles[i];
                bool compileAsFilter = file.CompileAsFilter;
                string shaderPath = file.FilePath;
                string modName = $"{watcher.ModName}.";
                string compiledXnbPath = watcher.CompilerPath + "\\" + Path.GetFileNameWithoutExtension(shaderPath) + ".xnb";
                string originalXnbPath = shaderPath.Replace(".fx", ".xnb");

                File.Delete(originalXnbPath);
                try
                {
                    File.Copy(compiledXnbPath, originalXnbPath);
                }
                catch
                {
                    return;
                }
                File.Delete(compiledXnbPath);

                // Finally, load the new XNB's shader data into the game's managed wrappers that reference it.
                string shaderPathInCompilerDirectory = compilerDirectory + Path.GetFileName(shaderPath);
                File.Delete(shaderPathInCompilerDirectory);
                Main.QueueMainThreadAction(() =>
                {
                    ContentManager tempManager = new(Main.instance.Content.ServiceProvider, Path.GetDirectoryName(originalXnbPath));
                    string assetName = Path.GetFileNameWithoutExtension(originalXnbPath);
                    Effect recompiledEffect = tempManager.Load<Effect>(assetName);
                    Ref<Effect> refEffect = new(recompiledEffect);

                    string shaderIdentifier = Path.GetFileNameWithoutExtension(compiledXnbPath);
                    if (compileAsFilter)
                    {
                        if (ShaderManager.filters.TryGetValue(shaderIdentifier, out ManagedScreenFilter oldShader))
                            oldShader.Effect = refEffect;
                        else
                            ShaderManager.SetFilter(modName + shaderIdentifier, refEffect);
                    }
                    else
                        ShaderManager.SetShader(modName + shaderIdentifier, refEffect);

                    Main.NewText($"Shader with the file name '{Path.GetFileName(shaderPath)}' has been successfully recompiled.");
                });
            }
        }

        private static void RecompileShader(object sender, FileSystemEventArgs e)
        {
            // Prevent the shader watcher from looking in the compiler folder.
            if (e.FullPath.Contains("\\Compiler"))
                return;

            // Prevent compiling files from being listed twice.
            if (CompilingFiles.Any(f => f.FilePath == e.FullPath))
                return;

            if (e.FullPath.Contains("Luminance"))
                return;

            CompilingFiles.Enqueue(new(e.FullPath, e.FullPath.Contains("\\Filters")));
        }

        public override void PostUpdateEverything()
        {
            foreach (ShaderWatcher watcher in ShaderWatchers)
                UpdateForModWatcher(watcher);
        }

        public override void OnModUnload()
        {
            foreach (ShaderWatcher watcher in ShaderWatchers)
                watcher.FileWatcher?.Dispose();
        }
    }
}
