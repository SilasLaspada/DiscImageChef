﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reflection;
using Aaru.CommonTypes.Interop;
using Aaru.Console;
using Aaru.Gui.Views;
using Avalonia.Controls;
using ReactiveUI;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Gui.ViewModels
{
    public class ConsoleWindowViewModel : ViewModelBase
    {
        bool                   _debugChecked;
        readonly ConsoleWindow _view;

        public ConsoleWindowViewModel(ConsoleWindow view)
        {
            _view        = view;
            SaveCommand  = ReactiveCommand.Create(ExecuteSaveCommand);
            ClearCommand = ReactiveCommand.Create(ExecuteClearCommand);
        }

        public string                         Title        => "Console";
        public ReactiveCommand<Unit, Unit>    ClearCommand { get; }
        public ReactiveCommand<Unit, Unit>    SaveCommand  { get; }
        public ObservableCollection<LogEntry> Entries      => ConsoleHandler.Entries;
        public string                         DebugText    => "Enable debug console";
        public string                         SaveLabel    => "Save";
        public string                         ClearLabel   => "Clear";

        public bool DebugChecked
        {
            get => _debugChecked;
            set
            {
                ConsoleHandler.Debug = value;
                this.RaiseAndSetIfChanged(ref _debugChecked, value);
            }
        }

        async void ExecuteSaveCommand()
        {
            var dlgSave = new SaveFileDialog();

            dlgSave.Filters.Add(new FileDialogFilter
            {
                Extensions = new List<string>(new[]
                {
                    "log"
                }),
                Name = "Log files"
            });

            string result = await dlgSave.ShowAsync(_view);

            if(result is null)
                return;

            try
            {
                var logFs = new FileStream(result, FileMode.Create, FileAccess.ReadWrite);
                var logSw = new StreamWriter(logFs);

                logSw.WriteLine("Log saved at {0}", DateTime.Now);

                PlatformID platId  = DetectOS.GetRealPlatformID();
                string     platVer = DetectOS.GetVersion();

                var assemblyVersion =
                    Attribute.GetCustomAttribute(typeof(AaruConsole).Assembly,
                                                 typeof(AssemblyInformationalVersionAttribute)) as
                        AssemblyInformationalVersionAttribute;

                logSw.WriteLine("################# System information #################");

                logSw.WriteLine("{0} {1} ({2}-bit)", DetectOS.GetPlatformName(platId, platVer), platVer,
                                Environment.Is64BitOperatingSystem ? 64 : 32);

                logSw.WriteLine(".NET Core {0}", Version.GetNetCoreVersion());

                logSw.WriteLine();

                logSw.WriteLine("################# Program information ################");
                logSw.WriteLine("Aaru {0}", assemblyVersion?.InformationalVersion);
                logSw.WriteLine("Running in {0}-bit", Environment.Is64BitProcess ? 64 : 32);
            #if DEBUG
                logSw.WriteLine("DEBUG version");
            #endif
                logSw.WriteLine("Command line: {0}", Environment.CommandLine);
                logSw.WriteLine();

                logSw.WriteLine("################# Console ################");

                foreach(LogEntry entry in ConsoleHandler.Entries)
                    if(entry.Type != "Info")
                        logSw.WriteLine("{0}: ({1}) {2}", entry.Timestamp, entry.Type.ToLower(), entry.Message);
                    else
                        logSw.WriteLine("{0}: {1}", entry.Timestamp, entry.Message);

                logSw.Close();
                logFs.Close();
            }
            catch(Exception exception)
            {
                Eto.Forms.MessageBox.Show("Exception {0} trying to save logfile, details has been sent to console.",
                                          exception.Message);

                AaruConsole.ErrorWriteLine("Console", exception.Message);
                AaruConsole.ErrorWriteLine("Console", exception.StackTrace);
            }
        }

        void ExecuteClearCommand() => ConsoleHandler.Entries.Clear();
    }
}