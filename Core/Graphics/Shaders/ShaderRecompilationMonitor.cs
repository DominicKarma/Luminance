using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    /// <summary>
    /// The shader recompilation manager, which is responsible for ensuring that changes to .fx files are reflected in-game automatically.
    /// </summary>
    public sealed class ShaderRecompilationMonitor : ModSystem
    {
        internal static Queue<CompilingFile> CompilingFiles
        {
            get;
            private set;
        } = [];

        internal static List<ShaderWatcher> ShaderWatchers
        {
            get;
            private set;
        } = [];

        /// <summary>
        /// The path to the central mod compiler.
        /// </summary>
        public static string CompilerDirectory => Path.Combine(Main.SavePath, "FXC");

        /// <summary>
        /// Represents a watcher that looks over a given directory with .fx files.
        /// </summary>
        /// <param name="EffectsPath">The path that this watcher oversees.</param>
        /// <param name="ModName">The name of the mod responsible for this shader watcher.</param>
        /// <param name="FileWatcher">The file watcher that fires signals when an .fx file changes.</param>
        public record ShaderWatcher(string EffectsPath, string ModName, FileSystemWatcher FileWatcher);

        /// <summary>
        /// Represents a .fx file that is being compiled.
        /// </summary>
        /// <param name="FilePath">The path to the associated file.</param>
        /// <param name="CompileAsFilter">Whether the file represents a screen filter or not.</param>
        public record CompilingFile(string FilePath, bool CompileAsFilter);

        /// <summary>
        /// Processes all shader watchers, checking if anything needs to be compiled.
        /// </summary>
        public override void PostUpdateEverything()
        {
            foreach (ShaderWatcher watcher in ShaderWatchers)
                ProcessCompilationsForWatcher(watcher);
        }

        /// <summary>
        /// Fixes an odd bug where the compiling files are filled during mod loading.
        /// </summary>
        public override void PostSetupContent()
        {
            CompilingFiles.Clear();
        }

        /// <summary>
        /// Handles on-mod-load effects for the library, ensuring that the compiler directory is unpacked.
        /// </summary>
        public override void OnModLoad()
        {
            ClearCompilationDirectory();

            if (Directory.Exists(CompilerDirectory) || Main.netMode != NetmodeID.SinglePlayer)
                return;

            CreateCompilerDirectory();
        }

        /// <summary>
        /// Handles on-mod-unload effects for all shader watchers, disposing of unmanaged file watchers.
        /// </summary>
        public override void OnModUnload()
        {
            foreach (ShaderWatcher watcher in ShaderWatchers)
                watcher.FileWatcher?.Dispose();
        }

        /// <summary>
        /// Creates the compiler directory in accordance with <see cref="CompilerDirectory"/>.
        /// </summary>
        internal void CreateCompilerDirectory()
        {
            Directory.CreateDirectory(CompilerDirectory);

            var fileNames = Mod.GetFileNames();
            var compilerFileNames = fileNames.Where(f => f.Contains("Assets/AutoloadedEffects/Compiler"));
            foreach (string compilerFileName in compilerFileNames)
            {
                byte[] fileData = Mod.GetFileBytes(compilerFileName);
                string copyFileName = Path.Combine(CompilerDirectory, Path.GetFileName(compilerFileName));
                File.WriteAllBytes(copyFileName, fileData);
            }
        }

        /// <summary>
        /// Attempts to load all potential shader watchers for a given mod.
        /// </summary>
        /// 
        /// <remarks>
        /// In order for this method to do anything, the following conditions must be met:
        /// <list type="bullet">
        ///     <item>The mod being searched must have an Assets/AutoloadedEffects directory, as an indicator that the mod is using the Luminance library.</item>
        ///     <item>The mod being searched must have an Assets/AutoloadedEffects/Compiler directory.</item>
        ///     <item>The user executing this method must have a relevant mod source folder that corresponds with the mod.</item>
        /// </list>
        /// </remarks>
        /// <param name="mod">The mod to check for.</param>
        internal static void LoadForMod(Mod mod)
        {
            // This system is completely unnecessary in multiplayer, as it is solely a client-side development tool.
            // As such, don't load anything in multiplayer.
            if (Main.netMode != NetmodeID.SinglePlayer)
                return;

            // Check to see if the user has a folder that corresponds to the shaders for this mod.
            // If this folder is not present, that means that they are not a developer and thusly this system would be irrelevant.
            string modSourcesPath = $"{Path.Combine(Program.SavePathShared, "ModSources")}\\{mod.Name}".Replace("\\..\\tModLoader", string.Empty);
            if (!Directory.Exists(modSourcesPath))
                return;

            // Verify that the Assets/AutoloadedEffects directory exists.
            string effectsPath = $"{modSourcesPath}\\Assets\\{ShaderManager.AutoloadDirectoryShaders.Replace("/", "\\")}";
            if (!Directory.Exists(effectsPath))
                return;

            string filtersPath = effectsPath.Replace("\\Shaders", "\\Filters");
            TryToWatchPath(mod, effectsPath);
            TryToWatchPath(mod, filtersPath);
        }

        /// <summary>
        /// Clears all .fx, .xnb, and .fxc files in the compiler directory.
        /// </summary>
        private static void ClearCompilationDirectory()
        {
            if (!Directory.Exists(CompilerDirectory))
                return;

            foreach (string fxFile in Directory.GetFiles(CompilerDirectory, "*.fx"))
                File.Delete(fxFile);
            foreach (string xnbFile in Directory.GetFiles(CompilerDirectory, "*.xnb"))
                File.Delete(xnbFile);
            foreach (string fxcFile in Directory.GetFiles(CompilerDirectory, "*.fxc"))
                File.Delete(fxcFile);
        }

        /// <summary>
        /// Attempts to create a new shader watcher for a given mod over a given path.
        /// </summary>
        /// <param name="mod">The mod that should own the shader watcher.</param>
        /// <param name="path">The path that the shader watcher should oversee.</param>
        private static void TryToWatchPath(Mod mod, string path)
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
            filterWatcher.Changed += MarkFileAsNeedingCompilation;
            ShaderWatchers.Add(new(path, mod.Name, filterWatcher));
        }

        /// <summary>
        /// Processes a given shader watcher, compiling files as they're modified.
        /// </summary>
        /// <param name="watcher">The shader watcher to process</param>
        private static void ProcessCompilationsForWatcher(ShaderWatcher watcher)
        {
            List<CompilingFile> compiledFiles = [];
            string compilerDirectory = CompilerDirectory;
            while (CompilingFiles.TryPeek(out CompilingFile file))
            {
                if (!file.FilePath.Contains(watcher.ModName))
                    return;

                MoveFileToCompilingFolder(file);

                compiledFiles.Add(file);
                CompilingFiles.Dequeue();
            }

            if (compiledFiles.Count <= 0)
                return;

            for (int i = 0; i < compiledFiles.Count; i++)
            {
                StartCompilerProcess(compiledFiles[i].FilePath);
                ProcessCompiledFile(compiledFiles[i], watcher, compilerDirectory);
            }
        }

        /// <summary>
        /// Starts the fxc compiler and runs it for a given .fx file.
        /// </summary>
        /// <param name="fxPath">The path to the .fx file.</param>
        private static void StartCompilerProcess(string fxPath)
        {
            fxPath = Path.GetFileName(fxPath);
            string outputPath = fxPath.Replace(".fx", ".fxc");
            string compilationCommand = $"/T fx_2_0 {fxPath} /Fo {outputPath}";

            Process fxcCompiler = new()
            {
                StartInfo = new($"{CompilerDirectory}\\fxc.exe")
                {
                    WorkingDirectory = CompilerDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = compilationCommand
                }
            };
            fxcCompiler.Start();
            if (!fxcCompiler.WaitForExit(2500))
            {
                Main.NewText("Shader compiler timed out. Likely error.", Color.OrangeRed);
                fxcCompiler.Kill();
                return;
            }

            string error = fxcCompiler.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                string[] errorLines = error.Split(Environment.NewLine);
                foreach (string errorLine in errorLines)
                {
                    if (errorLine.Contains("implicit truncation"))
                        continue;
                    if (errorLine.Contains("Effects deprecated"))
                        continue;
                    if (string.IsNullOrEmpty(errorLine))
                        continue;

                    Main.NewText(errorLine, Color.OrangeRed);
                }
            }

            fxcCompiler.Kill();
        }

        /// <summary>
        /// Processes a given compiled file after compilation, handling the deletion of old files and the setting of shaders in the central management system.
        /// </summary>
        /// <param name="file">The file to process.</param>
        /// <param name="watcher">The shader watcher responsible for the file.</param>
        /// <param name="compilerDirectory">The directory that contains the compiler executable.</param>
        private static void ProcessCompiledFile(CompilingFile file, ShaderWatcher watcher, string compilerDirectory)
        {
            bool compileAsFilter = file.CompileAsFilter;
            string shaderPath = file.FilePath;
            string modName = $"{watcher.ModName}.";
            string compiledFxcPath = CompilerDirectory + "\\" + Path.GetFileNameWithoutExtension(shaderPath) + ".fxc";
            string originalFxcPath = shaderPath.Replace(".fx", ".fxc");
            string shaderPathInCompilerDirectory = compilerDirectory + Path.DirectorySeparatorChar + Path.GetFileName(shaderPath);

            // Copy over the XNB from the compiler, and delete the copy in the Compiler folder.
            try
            {
                if (File.Exists(originalFxcPath))
                    File.Delete(originalFxcPath);

                File.Copy(compiledFxcPath, originalFxcPath);

                // If the old .xnb file format existed at compilation time, delete it, so that there aren't ambiguities between it and the new .fxc file.
                string oldXnbPath = shaderPath.Replace(".fx", ".xnb");
                if (File.Exists(oldXnbPath))
                    File.Delete(oldXnbPath);
            }
            catch
            {
                return;
            }
            finally
            {
                File.Delete(compiledFxcPath);
                File.Delete(shaderPathInCompilerDirectory);
            }

            // Finally, load the new XNB's shader data into the game's managed wrappers that reference it.
            Main.QueueMainThreadAction(() =>
            {
                string shaderIdentifier = modName + Path.GetFileNameWithoutExtension(compiledFxcPath);
                using FileStream shaderFileData = new(originalFxcPath, FileMode.Open);
                using MemoryStream shaderData = new();
                shaderFileData.CopyTo(shaderData);

                Ref<Effect> refEffect = new(new(Main.instance.GraphicsDevice, shaderData.ToArray()));

                if (compileAsFilter)
                {
                    if (ShaderManager.filters.TryGetValue(shaderIdentifier, out ManagedScreenFilter oldShader))
                    {
                        oldShader.Shader = refEffect;
                        oldShader.parameterCache?.Clear();
                    }
                    else
                        ShaderManager.SetFilter(shaderIdentifier, refEffect);
                }
                else
                    ShaderManager.SetShader(shaderIdentifier, refEffect);

                string shaderName = Path.GetFileName(shaderPath);
                Main.NewText($"Shader with the file name '{shaderName}' has been successfully recompiled.");
            });
        }

        /// <summary>
        /// Moves a given compiling file to the compilation directory so that the compiler can run on it and acquire a new compiled shader.
        /// </summary>
        /// <param name="file">The file to move.</param>
        private static void MoveFileToCompilingFolder(CompilingFile file)
        {
            string shaderPath = file.FilePath;
            string shaderPathInCompilerDirectory = Path.Combine(CompilerDirectory, Path.GetFileName(shaderPath));

            File.Delete(shaderPathInCompilerDirectory);
            try
            {
                File.WriteAllText(shaderPathInCompilerDirectory, File.ReadAllText(shaderPath));
            }
            catch { }
        }

        /// <summary>
        /// Marks a given file as needing compilation. Called as a consequence of a file watcher event firing.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments that specify file data.</param>
        private static void MarkFileAsNeedingCompilation(object sender, FileSystemEventArgs e)
        {
            if (Main.gameMenu)
                return;

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
    }
}
