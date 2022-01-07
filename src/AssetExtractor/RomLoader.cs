using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Tinke;

namespace AssetExtractor
{
    public static class RomLoader
    {
        private static object _locker = new object();
        public const string PlatinumPath = "platinum_assets";
        public const string HgSsPath = "hgss_assets";

        private static void EnsureRomExtracted(string path, string displayName, params string[] internalEdition)
        {
            lock (_locker)
            {
                var successPath = Path.Combine(path, "extracted.bin");
                if (File.Exists(successPath))
                    return;

                if (MessageBox.Show($"{displayName} assets are missing. Please open a corresponding ROM.", "Error", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    throw new FileNotFoundException();
                while (true)
                {
                    string file = null;
                    var openFileDialog = new OpenFileDialog
                    {
                        Filter = "ROM (*.nds)|*.nds|All files (*.*)|*.*"
                    };
                    if (openFileDialog.ShowDialog() == true)
                        file = openFileDialog.FileName;
                    if (file != null)
                    {
                        var sis = new Sistema();
                        try
                        {
                            sis.ReadGame(file);
                            if (internalEdition.Any(sis.romInfo.Banner.englishTitle.Contains))
                            {
                                var progressWindow = new ProgressBarWindow
                                {
                                    Title = $"{displayName} Extractor"
                                };
                                var extractionSuccessful = false;
                                var extractingThread = new Thread(() =>
                                {
                                    try
                                    {
                                        sis.Recursivo_UnpackFolder(sis.accion.Root);
                                        sis.Recursivo_UnpackFolder(sis.accion.Root);
                                        if (Directory.Exists(path))
                                            Directory.Delete(path, true);
                                        sis.RecursivoExtractFolder(sis.accion.Root, path);
                                        using (var successFile = new StreamWriter(successPath))
                                            successFile.Write(DateTime.Now);
                                        extractionSuccessful = true;
                                    }
                                    catch (Exception) { }
                                    finally
                                    {
                                        progressWindow.Dispatcher.Invoke(() => progressWindow.Close());
                                    }
                                });
                                extractingThread.Start();
                                progressWindow.ShowDialog();
                                if (extractionSuccessful)
                                    return;
                            }
                        }
                        catch (Exception) { }
                        finally
                        {
                            sis.accion.Dispose();
                        }
                    }
                    if (MessageBox.Show($"Invalid {displayName} File.", "Error", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        throw new FileNotFoundException();
                }
            }
        }
        private static bool _assetsChecked;
        public static void EnsureAssetsExist()
        {
            lock (_locker)
            {
                if (_assetsChecked) return;
                EnsureRomExtracted(PlatinumPath, "Platinum", "Platinum");
                EnsureRomExtracted(HgSsPath, "Heart Gold / Soul Silver", "HeartGold", "SoulSilver");
                _assetsChecked = true;
            }
        }
    }
}
