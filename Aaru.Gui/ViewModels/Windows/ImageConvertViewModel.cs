using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Aaru.Gui.Models;
using Avalonia.Controls;
using Avalonia.Threading;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using Schemas;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Gui.ViewModels.Windows
{
    public class ImageConvertViewModel : ViewModelBase
    {
        readonly Window      _view;
        readonly IMediaImage inputFormat;
        bool                 _cicmXmlFromImageVisible;
        string               _cicmXmlText;
        bool                 _closeVisible;
        string               _commentsText;
        bool                 _commentsVisible;
        string               _creatorText;

        bool   _creatorVisible;
        bool   _destinationEnabled;
        string _destinationText;

        bool   _destinationVisible;
        string _driveFirmwareRevisionText;
        bool   _driveFirmwareRevisionVisible;
        string _driveManufacturerText;
        bool   _driveManufacturerVisible;
        string _driveModelText;
        bool   _driveModelVisible;
        string _driveSerialNumberText;
        bool   _driveSerialNumberVisible;
        bool   _forceChecked;

        bool                   _formatReadOnly;
        double                 _lastMediaSequenceValue;
        bool                   _lastMediaSequenceVisible;
        string                 _mediaBarcodeText;
        bool                   _mediaBarcodeVisible;
        string                 _mediaManufacturerText;
        bool                   _mediaManufacturerVisible;
        string                 _mediaModelText;
        bool                   _mediaModelVisible;
        string                 _mediaPartNumberText;
        bool                   _mediaPartNumberVisible;
        double                 _mediaSequenceValue;
        bool                   _mediaSequenceVisible;
        string                 _mediaSerialNumberText;
        bool                   _mediaSerialNumberVisible;
        string                 _mediaTitleText;
        bool                   _mediaTitleVisible;
        bool                   _optionsVisible;
        bool                   _progress1Visible;
        bool                   _progress2Indeterminate;
        double                 _progress2MaxValue;
        string                 _progress2Text;
        double                 _progress2Value;
        bool                   _progress2Visible;
        bool                   _progressIndeterminate;
        double                 _progressMaxValue;
        string                 _progressText;
        double                 _progressValue;
        bool                   _progressVisible;
        bool                   _resumeFileFromImageVisible;
        string                 _resumeFileText;
        double                 _sectorsValue;
        ImagePluginModel       _selectedPlugin;
        string                 _sourceText;
        bool                   _startVisible;
        bool                   _stopEnabled;
        bool                   _stopVisible;
        string                 _title;
        bool                   cancel;
        CICMMetadataType       cicmMetadata;
        List<DumpHardwareType> dumpHardware;

        public ImageConvertViewModel(IMediaImage inputFormat, string imageSource, Window view)
        {
            _view                        = view;
            this.inputFormat             = inputFormat;
            cancel                       = false;
            DestinationCommand           = ReactiveCommand.Create(ExecuteDestinationCommand);
            CreatorCommand               = ReactiveCommand.Create(ExecuteCreatorCommand);
            MediaTitleCommand            = ReactiveCommand.Create(ExecuteMediaTitleCommand);
            MediaManufacturerCommand     = ReactiveCommand.Create(ExecuteMediaManufacturerCommand);
            MediaModelCommand            = ReactiveCommand.Create(ExecuteMediaModelCommand);
            MediaSerialNumberCommand     = ReactiveCommand.Create(ExecuteMediaSerialNumberCommand);
            MediaBarcodeCommand          = ReactiveCommand.Create(ExecuteMediaBarcodeCommand);
            MediaPartNumberCommand       = ReactiveCommand.Create(ExecuteMediaPartNumberCommand);
            MediaSequenceCommand         = ReactiveCommand.Create(ExecuteMediaSequenceCommand);
            LastMediaSequenceCommand     = ReactiveCommand.Create(ExecuteLastMediaSequenceCommand);
            DriveManufacturerCommand     = ReactiveCommand.Create(ExecuteDriveManufacturerCommand);
            DriveModelCommand            = ReactiveCommand.Create(ExecuteDriveModelCommand);
            DriveSerialNumberCommand     = ReactiveCommand.Create(ExecuteDriveSerialNumberCommand);
            DriveFirmwareRevisionCommand = ReactiveCommand.Create(ExecuteDriveFirmwareRevisionCommand);
            CommentsCommand              = ReactiveCommand.Create(ExecuteCommentsCommand);
            CicmXmlFromImageCommand      = ReactiveCommand.Create(ExecuteCicmXmlFromImageCommand);
            CicmXmlCommand               = ReactiveCommand.Create(ExecuteCicmXmlCommand);
            ResumeFileFromImageCommand   = ReactiveCommand.Create(ExecuteResumeFileFromImageCommand);
            ResumeFileCommand            = ReactiveCommand.Create(ExecuteResumeFileCommand);
            StartCommand                 = ReactiveCommand.Create(ExecuteStartCommand);
            CloseCommand                 = ReactiveCommand.Create(ExecuteCloseCommand);
            StopCommand                  = ReactiveCommand.Create(ExecuteStopCommand);

            SourceText               = imageSource;
            CreatorVisible           = !string.IsNullOrWhiteSpace(inputFormat.Info.Creator);
            MediaTitleVisible        = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaTitle);
            CommentsVisible          = !string.IsNullOrWhiteSpace(inputFormat.Info.Comments);
            MediaManufacturerVisible = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaManufacturer);
            MediaModelVisible        = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaModel);
            MediaSerialNumberVisible = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaSerialNumber);
            MediaBarcodeVisible      = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaBarcode);
            MediaPartNumberVisible   = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaPartNumber);

            MediaSequenceVisible = inputFormat.Info.MediaSequence != 0 && inputFormat.Info.LastMediaSequence != 0;

            LastMediaSequenceVisible = inputFormat.Info.MediaSequence != 0 && inputFormat.Info.LastMediaSequence != 0;

            DriveManufacturerVisible     = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveManufacturer);
            DriveModelVisible            = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveModel);
            DriveSerialNumberVisible     = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveSerialNumber);
            DriveFirmwareRevisionVisible = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveFirmwareRevision);

            PluginBase plugins = GetPluginBase.Instance;

            foreach(IWritableImage plugin in
                plugins.WritableImages.Values.Where(p => p.SupportedMediaTypes.Contains(inputFormat.Info.MediaType)))
                PluginsList.Add(new ImagePluginModel
                {
                    Plugin = plugin
                });

            CicmXmlFromImageVisible    = inputFormat.CicmMetadata != null;
            ResumeFileFromImageVisible = inputFormat.DumpHardware != null && inputFormat.DumpHardware.Any();
            cicmMetadata               = inputFormat.CicmMetadata;

            dumpHardware = inputFormat.DumpHardware != null && inputFormat.DumpHardware.Any() ? inputFormat.DumpHardware
                               : null;

            CicmXmlText    = cicmMetadata == null ? "" : "<From image>";
            ResumeFileText = dumpHardware == null ? "" : "<From image>";
        }

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string SourceText
        {
            get => _sourceText;
            set => this.RaiseAndSetIfChanged(ref _sourceText, value);
        }

        public ObservableCollection<ImagePluginModel> PluginsList { get; }

        public ImagePluginModel SelectedPlugin
        {
            get => _selectedPlugin;
            set => this.RaiseAndSetIfChanged(ref _selectedPlugin, value);
        }

        public string DestinationText
        {
            get => _destinationText;
            set => this.RaiseAndSetIfChanged(ref _destinationText, value);
        }

        public bool OptionsVisible
        {
            get => _optionsVisible;
            set => this.RaiseAndSetIfChanged(ref _optionsVisible, value);
        }

        public double SectorsValue
        {
            get => _sectorsValue;
            set => this.RaiseAndSetIfChanged(ref _sectorsValue, value);
        }

        public bool ForceChecked
        {
            get => _forceChecked;
            set => this.RaiseAndSetIfChanged(ref _forceChecked, value);
        }

        public string CreatorText
        {
            get => _creatorText;
            set => this.RaiseAndSetIfChanged(ref _creatorText, value);
        }

        public string MediaTitleText
        {
            get => _mediaTitleText;
            set => this.RaiseAndSetIfChanged(ref _mediaTitleText, value);
        }

        public string MediaManufacturerText
        {
            get => _mediaManufacturerText;
            set => this.RaiseAndSetIfChanged(ref _mediaManufacturerText, value);
        }

        public string MediaModelText
        {
            get => _mediaModelText;
            set => this.RaiseAndSetIfChanged(ref _mediaModelText, value);
        }

        public string MediaSerialNumberText
        {
            get => _mediaSerialNumberText;
            set => this.RaiseAndSetIfChanged(ref _mediaSerialNumberText, value);
        }

        public string MediaBarcodeText
        {
            get => _mediaBarcodeText;
            set => this.RaiseAndSetIfChanged(ref _mediaBarcodeText, value);
        }

        public string MediaPartNumberText
        {
            get => _mediaPartNumberText;
            set => this.RaiseAndSetIfChanged(ref _mediaPartNumberText, value);
        }

        public double MediaSequenceValue
        {
            get => _mediaSequenceValue;
            set => this.RaiseAndSetIfChanged(ref _mediaSequenceValue, value);
        }

        public double LastMediaSequenceValue
        {
            get => _lastMediaSequenceValue;
            set => this.RaiseAndSetIfChanged(ref _lastMediaSequenceValue, value);
        }

        public string DriveManufacturerText
        {
            get => _driveManufacturerText;
            set => this.RaiseAndSetIfChanged(ref _driveManufacturerText, value);
        }

        public string DriveModelText
        {
            get => _driveModelText;
            set => this.RaiseAndSetIfChanged(ref _driveModelText, value);
        }

        public string DriveSerialNumberText
        {
            get => _driveSerialNumberText;
            set => this.RaiseAndSetIfChanged(ref _driveSerialNumberText, value);
        }

        public string DriveFirmwareRevisionText
        {
            get => _driveFirmwareRevisionText;
            set => this.RaiseAndSetIfChanged(ref _driveFirmwareRevisionText, value);
        }

        public string CommentsText
        {
            get => _commentsText;
            set => this.RaiseAndSetIfChanged(ref _commentsText, value);
        }

        public string CicmXmlText
        {
            get => _cicmXmlText;
            set => this.RaiseAndSetIfChanged(ref _cicmXmlText, value);
        }

        public string ResumeFileText
        {
            get => _resumeFileText;
            set => this.RaiseAndSetIfChanged(ref _resumeFileText, value);
        }

        public bool ProgressVisible
        {
            get => _progressVisible;
            set => this.RaiseAndSetIfChanged(ref _progressVisible, value);
        }

        public bool Progress1Visible
        {
            get => _progress1Visible;
            set => this.RaiseAndSetIfChanged(ref _progress1Visible, value);
        }

        public string ProgressText
        {
            get => _progressText;
            set => this.RaiseAndSetIfChanged(ref _progressText, value);
        }

        public double ProgressValue
        {
            get => _progressValue;
            set => this.RaiseAndSetIfChanged(ref _progressValue, value);
        }

        public double ProgressMaxValue
        {
            get => _progressMaxValue;
            set => this.RaiseAndSetIfChanged(ref _progressMaxValue, value);
        }

        public bool ProgressIndeterminate
        {
            get => _progressIndeterminate;
            set => this.RaiseAndSetIfChanged(ref _progressIndeterminate, value);
        }

        public bool Progress2Visible
        {
            get => _progress2Visible;
            set => this.RaiseAndSetIfChanged(ref _progress2Visible, value);
        }

        public string Progress2Text
        {
            get => _progress2Text;
            set => this.RaiseAndSetIfChanged(ref _progress2Text, value);
        }

        public double Progress2Value
        {
            get => _progress2Value;
            set => this.RaiseAndSetIfChanged(ref _progress2Value, value);
        }

        public double Progress2MaxValue
        {
            get => _progress2MaxValue;
            set => this.RaiseAndSetIfChanged(ref _progress2MaxValue, value);
        }

        public bool Progress2Indeterminate
        {
            get => _progress2Indeterminate;
            set => this.RaiseAndSetIfChanged(ref _progress2Indeterminate, value);
        }

        public bool StartVisible
        {
            get => _startVisible;
            set => this.RaiseAndSetIfChanged(ref _startVisible, value);
        }

        public bool CloseVisible
        {
            get => _closeVisible;
            set => this.RaiseAndSetIfChanged(ref _closeVisible, value);
        }

        public bool StopVisible
        {
            get => _stopVisible;
            set => this.RaiseAndSetIfChanged(ref _stopVisible, value);
        }

        public bool StopEnabled
        {
            get => _stopEnabled;
            set => this.RaiseAndSetIfChanged(ref _stopEnabled, value);
        }

        public bool CreatorVisible
        {
            get => _creatorVisible;
            set => this.RaiseAndSetIfChanged(ref _creatorVisible, value);
        }

        public bool MediaTitleVisible
        {
            get => _mediaTitleVisible;
            set => this.RaiseAndSetIfChanged(ref _mediaTitleVisible, value);
        }

        public bool CommentsVisible
        {
            get => _commentsVisible;
            set => this.RaiseAndSetIfChanged(ref _commentsVisible, value);
        }

        public bool MediaManufacturerVisible
        {
            get => _mediaManufacturerVisible;
            set => this.RaiseAndSetIfChanged(ref _mediaManufacturerVisible, value);
        }

        public bool MediaModelVisible
        {
            get => _mediaModelVisible;
            set => this.RaiseAndSetIfChanged(ref _mediaModelVisible, value);
        }

        public bool MediaSerialNumberVisible
        {
            get => _mediaSerialNumberVisible;
            set => this.RaiseAndSetIfChanged(ref _mediaSerialNumberVisible, value);
        }

        public bool MediaBarcodeVisible
        {
            get => _mediaBarcodeVisible;
            set => this.RaiseAndSetIfChanged(ref _mediaBarcodeVisible, value);
        }

        public bool MediaPartNumberVisible
        {
            get => _mediaPartNumberVisible;
            set => this.RaiseAndSetIfChanged(ref _mediaPartNumberVisible, value);
        }

        public bool MediaSequenceVisible
        {
            get => _mediaSequenceVisible;
            set => this.RaiseAndSetIfChanged(ref _mediaSequenceVisible, value);
        }

        public bool LastMediaSequenceVisible
        {
            get => _lastMediaSequenceVisible;
            set => this.RaiseAndSetIfChanged(ref _lastMediaSequenceVisible, value);
        }

        public bool DriveManufacturerVisible
        {
            get => _driveManufacturerVisible;
            set => this.RaiseAndSetIfChanged(ref _driveManufacturerVisible, value);
        }

        public bool DriveModelVisible
        {
            get => _driveModelVisible;
            set => this.RaiseAndSetIfChanged(ref _driveModelVisible, value);
        }

        public bool DriveSerialNumberVisible
        {
            get => _driveSerialNumberVisible;
            set => this.RaiseAndSetIfChanged(ref _driveSerialNumberVisible, value);
        }

        public bool DriveFirmwareRevisionVisible
        {
            get => _driveFirmwareRevisionVisible;
            set => this.RaiseAndSetIfChanged(ref _driveFirmwareRevisionVisible, value);
        }

        public bool CicmXmlFromImageVisible
        {
            get => _cicmXmlFromImageVisible;
            set => this.RaiseAndSetIfChanged(ref _cicmXmlFromImageVisible, value);
        }

        public bool ResumeFileFromImageVisible
        {
            get => _resumeFileFromImageVisible;
            set => this.RaiseAndSetIfChanged(ref _resumeFileFromImageVisible, value);
        }

        public bool DestinationEnabled
        {
            get => _destinationEnabled;
            set => this.RaiseAndSetIfChanged(ref _destinationEnabled, value);
        }

        public ReactiveCommand<Unit, Unit> DestinationCommand           { get; }
        public ReactiveCommand<Unit, Unit> CreatorCommand               { get; }
        public ReactiveCommand<Unit, Unit> MediaTitleCommand            { get; }
        public ReactiveCommand<Unit, Unit> MediaManufacturerCommand     { get; }
        public ReactiveCommand<Unit, Unit> MediaModelCommand            { get; }
        public ReactiveCommand<Unit, Unit> MediaSerialNumberCommand     { get; }
        public ReactiveCommand<Unit, Unit> MediaBarcodeCommand          { get; }
        public ReactiveCommand<Unit, Unit> MediaPartNumberCommand       { get; }
        public ReactiveCommand<Unit, Unit> MediaSequenceCommand         { get; }
        public ReactiveCommand<Unit, Unit> LastMediaSequenceCommand     { get; }
        public ReactiveCommand<Unit, Unit> DriveManufacturerCommand     { get; }
        public ReactiveCommand<Unit, Unit> DriveModelCommand            { get; }
        public ReactiveCommand<Unit, Unit> DriveSerialNumberCommand     { get; }
        public ReactiveCommand<Unit, Unit> DriveFirmwareRevisionCommand { get; }
        public ReactiveCommand<Unit, Unit> CommentsCommand              { get; }
        public ReactiveCommand<Unit, Unit> CicmXmlFromImageCommand      { get; }
        public ReactiveCommand<Unit, Unit> CicmXmlCommand               { get; }
        public ReactiveCommand<Unit, Unit> ResumeFileFromImageCommand   { get; }
        public ReactiveCommand<Unit, Unit> ResumeFileCommand            { get; }
        public ReactiveCommand<Unit, Unit> StartCommand                 { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand                 { get; }
        public ReactiveCommand<Unit, Unit> StopCommand                  { get; }

        public bool FormatReadOnly
        {
            get => _formatReadOnly;
            set => this.RaiseAndSetIfChanged(ref _formatReadOnly, value);
        }

        public bool DestinationVisible
        {
            get => _destinationVisible;
            set => this.RaiseAndSetIfChanged(ref _destinationVisible, value);
        }

        async void ExecuteStartCommand()
        {
            if(SelectedPlugin is null)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("Error", "Error trying to find selected plugin",
                                                                    icon: Icon.Error).ShowDialog(_view);

                return;
            }

            new Thread(DoWork).Start(SelectedPlugin.Plugin);
        }

        async void DoWork(object plugin)
        {
            bool warning = false;

            if(!(plugin is IWritableImage outputFormat))
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("Error", "Error trying to find selected plugin",
                                                                    icon: Icon.Error).ShowDialog(_view);

                return;
            }

            var inputOptical  = inputFormat as IOpticalMediaImage;
            var outputOptical = outputFormat as IWritableOpticalImage;

            List<Track> tracks;

            try
            {
                tracks = inputOptical?.Tracks;
            }
            catch(Exception)
            {
                tracks = null;
            }

            // Prepare UI
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                CloseVisible       = false;
                StartVisible       = false;
                StopVisible        = true;
                ProgressVisible    = true;
                OptionsVisible     = false;
                StopEnabled        = true;
                FormatReadOnly     = true;
                DestinationVisible = false;

                ProgressMaxValue =  1d;
                ProgressMaxValue += inputFormat.Info.ReadableMediaTags.Count;
                ProgressMaxValue++;

                if(tracks != null)
                    ProgressMaxValue++;

                if(tracks == null)
                {
                    ProgressMaxValue += 2;

                    foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags)
                    {
                        switch(tag)
                        {
                            case SectorTagType.AppleSectorTag:
                            case SectorTagType.CdSectorSync:
                            case SectorTagType.CdSectorHeader:
                            case SectorTagType.CdSectorSubHeader:
                            case SectorTagType.CdSectorEdc:
                            case SectorTagType.CdSectorEccP:
                            case SectorTagType.CdSectorEccQ:
                            case SectorTagType.CdSectorEcc:
                                // This tags are inline in long sector
                                continue;
                        }

                        if(ForceChecked && !outputFormat.SupportedSectorTags.Contains(tag))
                            continue;

                        ProgressMaxValue++;
                    }
                }
                else
                {
                    ProgressMaxValue += tracks.Count;

                    foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.OrderBy(t => t))
                    {
                        switch(tag)
                        {
                            case SectorTagType.AppleSectorTag:
                            case SectorTagType.CdSectorSync:
                            case SectorTagType.CdSectorHeader:
                            case SectorTagType.CdSectorSubHeader:
                            case SectorTagType.CdSectorEdc:
                            case SectorTagType.CdSectorEccP:
                            case SectorTagType.CdSectorEccQ:
                            case SectorTagType.CdSectorEcc:
                                // This tags are inline in long sector
                                continue;
                        }

                        if(ForceChecked && !outputFormat.SupportedSectorTags.Contains(tag))
                            continue;

                        ProgressMaxValue += tracks.Count;
                    }
                }

                if(dumpHardware != null)
                    ProgressMaxValue++;

                if(cicmMetadata != null)
                    ProgressMaxValue++;

                ProgressMaxValue++;
            });

            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
            {
                if(outputFormat.SupportedMediaTags.Contains(mediaTag) || ForceChecked)
                    continue;

                await Dispatcher.UIThread.InvokeAsync(action: async () =>
                {
                    await MessageBoxManager.
                          GetMessageBoxStandardWindow("Error",
                                                      $"Converting image will lose media tag {mediaTag}, not continuing...",
                                                      icon: Icon.Error).ShowDialog(_view);
                });

                return;
            }

            bool useLong = inputFormat.Info.ReadableSectorTags.Count != 0;

            foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags)
            {
                if(outputFormat.SupportedSectorTags.Contains(sectorTag))
                    continue;

                if(ForceChecked)
                {
                    if(sectorTag != SectorTagType.CdTrackFlags &&
                       sectorTag != SectorTagType.CdTrackIsrc  &&
                       sectorTag != SectorTagType.CdSectorSubchannel)
                        useLong = false;

                    continue;
                }

                await Dispatcher.UIThread.InvokeAsync(action: async () =>
                {
                    await MessageBoxManager.
                          GetMessageBoxStandardWindow("Error",
                                                      $"Converting image will lose sector tag {sectorTag}, not continuing...",
                                                      icon: Icon.Error).ShowDialog(_view);
                });

                return;
            }

            Dictionary<string, string> parsedOptions = new Dictionary<string, string>();

            /* TODO:
            if(grpOptions.Content is StackLayout stkImageOptions)
                foreach(Control option in stkImageOptions.Children)
                {
                    if(cancel)
                        break;

                    string value;

                    switch(option)
                    {
                        case CheckBox optBoolean:
                            value = optBooleanChecked?.ToString();

                            break;
                        case NumericStepper optNumber:
                            value = optNumber.Value.ToString(CultureInfo.CurrentCulture);

                            break;
                        case TextBox optString:
                            value = optString.Text;

                            break;
                        default: continue;
                    }

                    string key = option.ID.Substring(3);

                    parsedOptions.Add(key, value);
                }
                */

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText           = "Creating output image";
                Progress2Text          = "";
                Progress2Indeterminate = true;
            });

            if(!outputFormat.Create(DestinationText, inputFormat.Info.MediaType, parsedOptions,
                                    inputFormat.Info.Sectors, inputFormat.Info.SectorSize))
            {
                await Dispatcher.UIThread.InvokeAsync(action: async () =>
                {
                    await MessageBoxManager.
                          GetMessageBoxStandardWindow("Error",
                                                      $"Error {outputFormat.ErrorMessage} creating output image.",
                                                      icon: Icon.Error).ShowDialog(_view);
                });

                AaruConsole.ErrorWriteLine("Error {0} creating output image.", outputFormat.ErrorMessage);

                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText = "Setting image metadata";
                ProgressValue++;
                Progress2Text          = "";
                Progress2Indeterminate = true;
            });

            var metadata = new ImageInfo
            {
                Application       = "Aaru", ApplicationVersion = Version.GetVersion(),
                Comments          = CommentsText,
                Creator           = CreatorText, DriveFirmwareRevision       = DriveFirmwareRevisionText,
                DriveManufacturer = DriveManufacturerText, DriveModel        = DriveModelText,
                DriveSerialNumber = DriveSerialNumberText, LastMediaSequence = (int)LastMediaSequenceValue,
                MediaBarcode      = MediaBarcodeText, MediaManufacturer      = MediaManufacturerText,
                MediaModel        = MediaModelText,
                MediaPartNumber   = MediaPartNumberText, MediaSequence = (int)MediaSequenceValue,
                MediaSerialNumber = MediaSerialNumberText, MediaTitle  = MediaTitleText
            };

            if(!cancel)
                if(!outputFormat.SetMetadata(metadata))
                {
                    AaruConsole.ErrorWrite("Error {0} setting metadata, ", outputFormat.ErrorMessage);

                    if(ForceChecked != true)
                    {
                        await Dispatcher.UIThread.InvokeAsync(action: async () =>
                        {
                            await MessageBoxManager.
                                  GetMessageBoxStandardWindow("Error",
                                                              $"Error {outputFormat.ErrorMessage} setting metadata, not continuing...",
                                                              icon: Icon.Error).ShowDialog(_view);
                        });

                        AaruConsole.ErrorWriteLine("not continuing...");

                        return;
                    }

                    warning = true;
                    AaruConsole.ErrorWriteLine("continuing...");
                }

            if(tracks != null &&
               !cancel        &&
               outputOptical != null)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressText = "Setting tracks list";
                    ProgressValue++;
                    Progress2Text          = "";
                    Progress2Indeterminate = true;
                });

                if(!outputOptical.SetTracks(tracks))
                {
                    await Dispatcher.UIThread.InvokeAsync(action: async () =>
                    {
                        await MessageBoxManager.
                              GetMessageBoxStandardWindow("Error",
                                                          $"Error {outputFormat.ErrorMessage} sending tracks list to output image.",
                                                          icon: Icon.Error).ShowDialog(_view);
                    });

                    AaruConsole.ErrorWriteLine("Error {0} sending tracks list to output image.",
                                               outputFormat.ErrorMessage);

                    return;
                }
            }

            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
            {
                if(cancel)
                    break;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressText = $"Converting media tag {mediaTag}";
                    ProgressValue++;
                    Progress2Text          = "";
                    Progress2Indeterminate = true;
                });

                if(ForceChecked && !outputFormat.SupportedMediaTags.Contains(mediaTag))
                    continue;

                byte[] tag = inputFormat.ReadDiskTag(mediaTag);

                if(outputFormat.WriteMediaTag(tag, mediaTag))
                    continue;

                if(ForceChecked)
                {
                    warning = true;
                    AaruConsole.ErrorWriteLine("Error {0} writing media tag, continuing...", outputFormat.ErrorMessage);
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(action: async () =>
                    {
                        await MessageBoxManager.
                              GetMessageBoxStandardWindow("Error",
                                                          $"Error {outputFormat.ErrorMessage} writing media tag, not continuing...",
                                                          icon: Icon.Error).ShowDialog(_view);
                    });

                    AaruConsole.ErrorWriteLine("Error {0} writing media tag, not continuing...",
                                               outputFormat.ErrorMessage);

                    return;
                }
            }

            ulong doneSectors = 0;

            if(tracks == null &&
               !cancel)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressText =
                        $"Setting geometry to {inputFormat.Info.Cylinders} cylinders, {inputFormat.Info.Heads} heads and {inputFormat.Info.SectorsPerTrack} sectors per track";

                    ProgressValue++;
                    Progress2Text          = "";
                    Progress2Indeterminate = true;
                });

                if(!outputFormat.SetGeometry(inputFormat.Info.Cylinders, inputFormat.Info.Heads,
                                             inputFormat.Info.SectorsPerTrack))
                {
                    warning = true;

                    AaruConsole.ErrorWriteLine("Error {0} setting geometry, image may be incorrect, continuing...",
                                               outputFormat.ErrorMessage);
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressText = "Converting sectors";
                    ProgressValue++;
                    Progress2Text          = "";
                    Progress2Indeterminate = false;
                    Progress2MaxValue      = (int)(inputFormat.Info.Sectors / SectorsValue);
                });

                while(doneSectors < inputFormat.Info.Sectors)
                {
                    if(cancel)
                        break;

                    byte[] sector;

                    uint sectorsToDo;

                    if(inputFormat.Info.Sectors - doneSectors >= (ulong)SectorsValue)
                        sectorsToDo = (uint)SectorsValue;
                    else
                        sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                    ulong sectors = doneSectors;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Progress2Text =
                            $"Converting sectors {sectors} to {sectors + sectorsToDo} ({sectors / (double)inputFormat.Info.Sectors:P2} done)";

                        Progress2Value = (int)(sectors / SectorsValue);
                    });

                    bool result;

                    if(useLong)
                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSectorLong(doneSectors);
                            result = outputFormat.WriteSectorLong(sector, doneSectors);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectorsLong(doneSectors, sectorsToDo);
                            result = outputFormat.WriteSectorsLong(sector, doneSectors, sectorsToDo);
                        }
                    else
                    {
                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSector(doneSectors);
                            result = outputFormat.WriteSector(sector, doneSectors);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectors(doneSectors, sectorsToDo);
                            result = outputFormat.WriteSectors(sector, doneSectors, sectorsToDo);
                        }
                    }

                    if(!result)
                        if(ForceChecked)
                        {
                            warning = true;

                            AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                       outputFormat.ErrorMessage, doneSectors);
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(action: async () =>
                            {
                                await MessageBoxManager.
                                      GetMessageBoxStandardWindow("Error",
                                                                  $"Error {outputFormat.ErrorMessage} writing sector {doneSectors}, not continuing...",
                                                                  icon: Icon.Error).ShowDialog(_view);
                            });

                            AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                       outputFormat.ErrorMessage, doneSectors);

                            return;
                        }

                    doneSectors += sectorsToDo;
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Progress2Text =
                        $"Converting sectors {inputFormat.Info.Sectors} to {inputFormat.Info.Sectors} ({1.0:P2} done)";

                    Progress2Value = Progress2MaxValue;
                });

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags)
                {
                    if(!useLong || cancel)
                        break;

                    switch(tag)
                    {
                        case SectorTagType.AppleSectorTag:
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorSubHeader:
                        case SectorTagType.CdSectorEdc:
                        case SectorTagType.CdSectorEccP:
                        case SectorTagType.CdSectorEccQ:
                        case SectorTagType.CdSectorEcc:
                            // This tags are inline in long sector
                            continue;
                    }

                    if(ForceChecked && !outputFormat.SupportedSectorTags.Contains(tag))
                        continue;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ProgressText = $"Converting tag {tag}";
                        ProgressValue++;
                        Progress2Text          = "";
                        Progress2Indeterminate = false;
                        Progress2MaxValue      = (int)(inputFormat.Info.Sectors / SectorsValue);
                    });

                    doneSectors = 0;

                    while(doneSectors < inputFormat.Info.Sectors)
                    {
                        if(cancel)
                            break;

                        byte[] sector;

                        uint sectorsToDo;

                        if(inputFormat.Info.Sectors - doneSectors >= (ulong)SectorsValue)
                            sectorsToDo = (uint)SectorsValue;
                        else
                            sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                        ulong sectors = doneSectors;

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Progress2Text =
                                $"Converting tag {sectors / (double)inputFormat.Info.Sectors} for sectors {sectors} to {sectors + sectorsToDo} ({sectors / (double)inputFormat.Info.Sectors:P2} done)";

                            Progress2Value = (int)(sectors / SectorsValue);
                        });

                        bool result;

                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSectorTag(doneSectors, tag);
                            result = outputFormat.WriteSectorTag(sector, doneSectors, tag);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectorsTag(doneSectors, sectorsToDo, tag);
                            result = outputFormat.WriteSectorsTag(sector, doneSectors, sectorsToDo, tag);
                        }

                        if(!result)
                            if(ForceChecked)
                            {
                                warning = true;

                                AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                           outputFormat.ErrorMessage, doneSectors);
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(action: async () =>
                                {
                                    await MessageBoxManager.
                                          GetMessageBoxStandardWindow("Error",
                                                                      $"Error {outputFormat.ErrorMessage} writing sector {doneSectors}, not continuing...",
                                                                      icon: Icon.Error).ShowDialog(_view);
                                });

                                AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                           outputFormat.ErrorMessage, doneSectors);

                                return;
                            }

                        doneSectors += sectorsToDo;
                    }

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Progress2Text =
                            $"Converting tag {tag} for sectors {inputFormat.Info.Sectors} to {inputFormat.Info.Sectors} ({1.0:P2} done)";

                        Progress2Value = Progress2MaxValue;
                    });
                }
            }
            else
            {
                foreach(Track track in tracks)
                {
                    if(cancel)
                        break;

                    doneSectors = 0;
                    ulong trackSectors = (track.TrackEndSector - track.TrackStartSector) + 1;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ProgressText = $"Converting sectors in track {track.TrackSequence}";
                        ProgressValue++;
                        Progress2Text          = "";
                        Progress2Indeterminate = false;
                        Progress2MaxValue      = (int)(trackSectors / SectorsValue);
                    });

                    while(doneSectors < trackSectors)
                    {
                        if(cancel)
                            break;

                        byte[] sector;

                        uint sectorsToDo;

                        if(trackSectors - doneSectors >= (ulong)SectorsValue)
                            sectorsToDo = (uint)SectorsValue;
                        else
                            sectorsToDo = (uint)(trackSectors - doneSectors);

                        ulong sectors = doneSectors;

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Progress2Text =
                                $"Converting sectors {sectors + track.TrackStartSector} to {sectors + sectorsToDo + track.TrackStartSector} in track {track.TrackSequence} ({(sectors + track.TrackStartSector) / (double)inputFormat.Info.Sectors:P2} done)";

                            Progress2Value = (int)(sectors / SectorsValue);
                        });

                        bool result;

                        if(useLong)
                            if(sectorsToDo == 1)
                            {
                                sector = inputFormat.ReadSectorLong(doneSectors           + track.TrackStartSector);
                                result = outputFormat.WriteSectorLong(sector, doneSectors + track.TrackStartSector);
                            }
                            else
                            {
                                sector = inputFormat.ReadSectorsLong(doneSectors + track.TrackStartSector, sectorsToDo);

                                result = outputFormat.WriteSectorsLong(sector, doneSectors + track.TrackStartSector,
                                                                       sectorsToDo);
                            }
                        else
                        {
                            if(sectorsToDo == 1)
                            {
                                sector = inputFormat.ReadSector(doneSectors           + track.TrackStartSector);
                                result = outputFormat.WriteSector(sector, doneSectors + track.TrackStartSector);
                            }
                            else
                            {
                                sector = inputFormat.ReadSectors(doneSectors + track.TrackStartSector, sectorsToDo);

                                result = outputFormat.WriteSectors(sector, doneSectors + track.TrackStartSector,
                                                                   sectorsToDo);
                            }
                        }

                        if(!result)
                            if(ForceChecked)
                            {
                                warning = true;

                                AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                           outputFormat.ErrorMessage, doneSectors);
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(action: async () =>
                                {
                                    await MessageBoxManager.
                                          GetMessageBoxStandardWindow("Error",
                                                                      $"Error {outputFormat.ErrorMessage} writing sector {doneSectors}, not continuing...",
                                                                      icon: Icon.Error).ShowDialog(_view);
                                });

                                return;
                            }

                        doneSectors += sectorsToDo;
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Progress2Text =
                        $"Converting sectors {inputFormat.Info.Sectors} to {inputFormat.Info.Sectors} in track {tracks.Count} ({1.0:P2} done)";

                    Progress2Value = Progress2MaxValue;
                });

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.OrderBy(t => t))
                {
                    if(!useLong || cancel)
                        break;

                    switch(tag)
                    {
                        case SectorTagType.AppleSectorTag:
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorSubHeader:
                        case SectorTagType.CdSectorEdc:
                        case SectorTagType.CdSectorEccP:
                        case SectorTagType.CdSectorEccQ:
                        case SectorTagType.CdSectorEcc:
                            // This tags are inline in long sector
                            continue;
                    }

                    if(ForceChecked && !outputFormat.SupportedSectorTags.Contains(tag))
                        continue;

                    foreach(Track track in tracks)
                    {
                        if(cancel)
                            break;

                        doneSectors = 0;
                        ulong  trackSectors = (track.TrackEndSector - track.TrackStartSector) + 1;
                        byte[] sector;
                        bool   result;

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ProgressText = $"Converting tag {tag} in track {track.TrackSequence}.";
                            ProgressValue++;
                            Progress2Text          = "";
                            Progress2Indeterminate = false;
                            Progress2MaxValue      = (int)(trackSectors / SectorsValue);
                        });

                        switch(tag)
                        {
                            case SectorTagType.CdTrackFlags:
                            case SectorTagType.CdTrackIsrc:

                                sector = inputFormat.ReadSectorTag(track.TrackStartSector, tag);
                                result = outputFormat.WriteSectorTag(sector, track.TrackStartSector, tag);

                                if(!result)
                                    if(ForceChecked)
                                    {
                                        warning = true;

                                        AaruConsole.ErrorWriteLine("Error {0} writing tag, continuing...",
                                                                   outputFormat.ErrorMessage);
                                    }
                                    else
                                    {
                                        await Dispatcher.UIThread.InvokeAsync(action: async () =>
                                        {
                                            await MessageBoxManager.
                                                  GetMessageBoxStandardWindow("Error",
                                                                              $"Error {outputFormat.ErrorMessage} writing tag, not continuing...",
                                                                              icon: Icon.Error).ShowDialog(_view);
                                        });

                                        return;
                                    }

                                continue;
                        }

                        while(doneSectors < trackSectors)
                        {
                            if(cancel)
                                break;

                            uint sectorsToDo;

                            if(trackSectors - doneSectors >= (ulong)SectorsValue)
                                sectorsToDo = (uint)SectorsValue;
                            else
                                sectorsToDo = (uint)(trackSectors - doneSectors);

                            ulong sectors = doneSectors;

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Progress2Text =
                                    $"Converting tag {tag} for sectors {sectors + track.TrackStartSector} to {sectors + sectorsToDo + track.TrackStartSector} in track {track.TrackSequence} ({(sectors + track.TrackStartSector) / (double)inputFormat.Info.Sectors:P2} done)";

                                Progress2Value = (int)(sectors / SectorsValue);
                            });

                            if(sectorsToDo == 1)
                            {
                                sector = inputFormat.ReadSectorTag(doneSectors           + track.TrackStartSector, tag);
                                result = outputFormat.WriteSectorTag(sector, doneSectors + track.TrackStartSector, tag);
                            }
                            else
                            {
                                sector = inputFormat.ReadSectorsTag(doneSectors + track.TrackStartSector, sectorsToDo,
                                                                    tag);

                                result = outputFormat.WriteSectorsTag(sector, doneSectors + track.TrackStartSector,
                                                                      sectorsToDo, tag);
                            }

                            if(!result)
                                if(ForceChecked)
                                {
                                    warning = true;

                                    AaruConsole.ErrorWriteLine("Error {0} writing tag for sector {1}, continuing...",
                                                               outputFormat.ErrorMessage, doneSectors);
                                }
                                else
                                {
                                    await Dispatcher.UIThread.InvokeAsync(action: async () =>
                                    {
                                        await MessageBoxManager.
                                              GetMessageBoxStandardWindow("Error",
                                                                          $"Error {outputFormat.ErrorMessage} writing tag for sector {doneSectors}, not continuing...",
                                                                          icon: Icon.Error).ShowDialog(_view);
                                    });

                                    return;
                                }

                            doneSectors += sectorsToDo;
                        }
                    }
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Progress2Visible = false;
                Progress2Visible = false;
            });

            bool ret = false;

            if(dumpHardware != null &&
               !cancel)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressText = "Writing dump hardware list to output image.";
                    ProgressValue++;
                });

                ret = outputFormat.SetDumpHardware(dumpHardware);

                if(!ret)
                    AaruConsole.WriteLine("Error {0} writing dump hardware list to output image.",
                                          outputFormat.ErrorMessage);
            }

            ret = false;

            if(cicmMetadata != null &&
               !cancel)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressText = "Writing CICM XML metadata to output image.";
                    ProgressValue++;
                });

                outputFormat.SetCicmMetadata(cicmMetadata);

                if(!ret)
                    AaruConsole.WriteLine("Error {0} writing CICM XML metadata to output image.",
                                          outputFormat.ErrorMessage);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText          = "Closing output image.";
                ProgressIndeterminate = true;
            });

            if(cancel)
            {
                await Dispatcher.UIThread.InvokeAsync(action: async () =>
                {
                    await MessageBoxManager.
                          GetMessageBoxStandardWindow("Error", "Operation canceled, the output file is not correct.",
                                                      icon: Icon.Error).ShowDialog(_view);

                    CloseVisible    = true;
                    StopVisible     = false;
                    ProgressVisible = false;
                });

                return;
            }

            if(!outputFormat.Close())
            {
                await Dispatcher.UIThread.InvokeAsync(action: async () =>
                {
                    await MessageBoxManager.
                          GetMessageBoxStandardWindow("Error",
                                                      $"Error {outputFormat.ErrorMessage} closing output image... Contents are not correct.",
                                                      icon: Icon.Error).ShowDialog(_view);
                });

                return;
            }

            await Dispatcher.UIThread.InvokeAsync(action: async () =>
            {
                await MessageBoxManager.GetMessageBoxStandardWindow(warning ? "Warning" : "Convertion success",
                                                                    warning
                                                                        ? "Some warnings happened. Check console for more information. Image should be correct."
                                                                        : "Image converted successfully.",
                                                                    icon: warning ? Icon.Warning : Icon.Info).
                                        ShowDialog(_view);

                CloseVisible    = true;
                StopVisible     = false;
                ProgressVisible = false;
            });

            Statistics.AddCommand("convert-image");
        }

        protected void ExecuteCloseCommand() => _view.Close();

        protected internal void ExecuteStopCommand()
        {
            cancel      = true;
            StopEnabled = false;
        }

        /* TODO
                void OnCmbFormatSelectedIndexChanged()
                {
                    txtDestination.Text = "";

                    if(!(cmbFormat.SelectedValue is IWritableImage plugin))
                    {
                        grpOptions.Visible     = false;
                        btnDestination.Enabled = false;

                        return;
                    }

                    btnDestination.Enabled = true;

                    if(!plugin.SupportedOptions.Any())
                    {
                        grpOptions.Content = null;
                        grpOptions.Visible = false;

                        return;
                    }

                    chkForce.Visible = false;

                    foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
                    {
                        if(plugin.SupportedMediaTags.Contains(mediaTag))
                            continue;

                        chkForce.Visible = true;
                        ForceChecked = true;

                        break;
                    }

                    foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags)
                    {
                        if(plugin.SupportedSectorTags.Contains(sectorTag))
                            continue;

                        chkForce.Visible = true;
                        ForceChecked = true;

                        break;
                    }

                    grpOptions.Visible = true;

                    var stkImageOptions = new StackLayout
                    {
                        Orientation = Orientation.Vertical
                    };

                    foreach((string name, Type type, string description, object @default) option in plugin.SupportedOptions)
                        switch(option.type.ToString())
                        {
                            case "System.Boolean":
                                var optBoolean = new CheckBox();
                                optBoolean.ID      = "opt" + option.name;
                                optBoolean.Text    = option.description;
                                optBooleanChecked = (bool)option.@default;
                                stkImageOptions.Items.Add(optBoolean);

                                break;
                            case "System.SByte":
                            case "System.Int16":
                            case "System.Int32":
                            case "System.Int64":
                                var stkNumber = new StackLayout();
                                stkNumber.Orientation = Orientation.Horizontal;
                                var optNumber = new NumericStepper();
                                optNumber.ID    = "opt" + option.name;
                                optNumber.Value = Convert.ToDouble(option.@default);
                                stkNumber.Items.Add(optNumber);
                                var lblNumber = new Label();
                                lblNumber.Text = option.description;
                                stkNumber.Items.Add(lblNumber);
                                stkImageOptions.Items.Add(stkNumber);

                                break;
                            case "System.Byte":
                            case "System.UInt16":
                            case "System.UInt32":
                            case "System.UInt64":
                                var stkUnsigned = new StackLayout();
                                stkUnsigned.Orientation = Orientation.Horizontal;
                                var optUnsigned = new NumericStepper();
                                optUnsigned.ID       = "opt" + option.name;
                                optUnsigned.MinValue = 0;
                                optUnsigned.Value    = Convert.ToDouble(option.@default);
                                stkUnsigned.Items.Add(optUnsigned);
                                var lblUnsigned = new Label();
                                lblUnsigned.Text = option.description;
                                stkUnsigned.Items.Add(lblUnsigned);
                                stkImageOptions.Items.Add(stkUnsigned);

                                break;
                            case "System.Single":
                            case "System.Double":
                                var stkFloat = new StackLayout();
                                stkFloat.Orientation = Orientation.Horizontal;
                                var optFloat = new NumericStepper();
                                optFloat.ID            = "opt" + option.name;
                                optFloat.DecimalPlaces = 2;
                                optFloat.Value         = Convert.ToDouble(option.@default);
                                stkFloat.Items.Add(optFloat);
                                var lblFloat = new Label();
                                lblFloat.Text = option.description;
                                stkFloat.Items.Add(lblFloat);
                                stkImageOptions.Items.Add(stkFloat);

                                break;
                            case "System.Guid":
                                // TODO
                                break;
                            case "System.String":
                                var stkString = new StackLayout();
                                stkString.Orientation = Orientation.Horizontal;
                                var lblString = new Label();
                                lblString.Text = option.description;
                                stkString.Items.Add(lblString);
                                var optString = new TextBox();
                                optString.ID   = "opt" + option.name;
                                optString.Text = (string)option.@default;
                                stkString.Items.Add(optString);
                                stkImageOptions.Items.Add(stkString);

                                break;
                        }

                    grpOptions.Content = stkImageOptions;
                }
        */
        async void ExecuteDestinationCommand()
        {
            if(SelectedPlugin is null)
                return;

            var dlgDestination = new SaveFileDialog
            {
                Title = "Choose destination file"
            };

            dlgDestination.Filters.Add(new FileDialogFilter
            {
                Name = SelectedPlugin.Plugin.Name, Extensions = SelectedPlugin.Plugin.KnownExtensions.ToList()
            });

            string result = await dlgDestination.ShowAsync(_view);

            if(result is null ||
               result.Length != 1)
            {
                DestinationText = "";

                return;
            }

            if(string.IsNullOrEmpty(Path.GetExtension(result)))
                result += SelectedPlugin.Plugin.KnownExtensions.First();

            DestinationText = result;
        }

        void ExecuteCreatorCommand() => CreatorText = inputFormat.Info.Creator;

        void ExecuteMediaTitleCommand() => MediaTitleText = inputFormat.Info.MediaTitle;

        void ExecuteCommentsCommand() => CommentsText = inputFormat.Info.Comments;

        void ExecuteMediaManufacturerCommand() => MediaManufacturerText = inputFormat.Info.MediaManufacturer;

        void ExecuteMediaModelCommand() => MediaModelText = inputFormat.Info.MediaModel;

        void ExecuteMediaSerialNumberCommand() => MediaSerialNumberText = inputFormat.Info.MediaSerialNumber;

        void ExecuteMediaBarcodeCommand() => MediaBarcodeText = inputFormat.Info.MediaBarcode;

        void ExecuteMediaPartNumberCommand() => MediaPartNumberText = inputFormat.Info.MediaPartNumber;

        void ExecuteMediaSequenceCommand() => MediaSequenceValue = inputFormat.Info.MediaSequence;

        void ExecuteLastMediaSequenceCommand() => LastMediaSequenceValue = inputFormat.Info.LastMediaSequence;

        void ExecuteDriveManufacturerCommand() => DriveManufacturerText = inputFormat.Info.DriveManufacturer;

        void ExecuteDriveModelCommand() => DriveModelText = inputFormat.Info.DriveModel;

        void ExecuteDriveSerialNumberCommand() => DriveSerialNumberText = inputFormat.Info.DriveSerialNumber;

        void ExecuteDriveFirmwareRevisionCommand() =>
            DriveFirmwareRevisionText = inputFormat.Info.DriveFirmwareRevision;

        void ExecuteCicmXmlFromImageCommand()
        {
            CicmXmlText  = "<From image>";
            cicmMetadata = inputFormat.CicmMetadata;
        }

        async void ExecuteCicmXmlCommand()
        {
            cicmMetadata = null;
            CicmXmlText  = "";

            var dlgMetadata = new OpenFileDialog
            {
                Title = "Choose existing metadata sidecar"
            };

            dlgMetadata.Filters.Add(new FileDialogFilter
            {
                Name = "CICM XML metadata", Extensions = new List<string>(new[]
                {
                    ".xml"
                })
            });

            string[] result = await dlgMetadata.ShowAsync(_view);

            if(result is null ||
               result.Length != 1)
                return;

            var sidecarXs = new XmlSerializer(typeof(CICMMetadataType));

            try
            {
                var sr = new StreamReader(result[0]);
                cicmMetadata = (CICMMetadataType)sidecarXs.Deserialize(sr);
                sr.Close();
                CicmXmlText = result[0];
            }
            catch
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("Error", "Incorrect metadata sidecar file...",
                                                                    icon: Icon.Error).ShowDialog(_view);
            }
        }

        void ExecuteResumeFileFromImageCommand()
        {
            ResumeFileText = "<From image>";
            dumpHardware   = inputFormat.DumpHardware;
        }

        async void ExecuteResumeFileCommand()
        {
            dumpHardware   = null;
            ResumeFileText = "";

            var dlgMetadata = new OpenFileDialog
            {
                Title = "Choose existing resume file"
            };

            dlgMetadata.Filters.Add(new FileDialogFilter
            {
                Name = "CICM XML metadata", Extensions = new List<string>(new[]
                {
                    ".xml"
                })
            });

            string[] result = await dlgMetadata.ShowAsync(_view);

            if(result is null ||
               result.Length != 1)
                return;

            var sidecarXs = new XmlSerializer(typeof(Resume));

            try
            {
                var sr     = new StreamReader(result[0]);
                var resume = (Resume)sidecarXs.Deserialize(sr);

                if(resume.Tries != null &&
                   !resume.Tries.Any())
                {
                    dumpHardware   = resume.Tries;
                    ResumeFileText = result[0];
                }
                else
                    await MessageBoxManager.
                          GetMessageBoxStandardWindow("Error",
                                                      "Resume file does not contain dump hardware information...",
                                                      icon: Icon.Error).ShowDialog(_view);

                sr.Close();
            }
            catch
            {
                await MessageBoxManager.
                      GetMessageBoxStandardWindow("Error", "Incorrect resume file...", icon: Icon.Error).
                      ShowDialog(_view);
            }
        }
    }
}