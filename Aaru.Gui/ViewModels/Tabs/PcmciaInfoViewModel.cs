using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using Aaru.Console;
using Aaru.Decoders.PCMCIA;
using Aaru.Gui.Models;
using Avalonia.Controls;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Tabs
{
    public class PcmciaInfoViewModel : ViewModelBase
    {
        readonly Window _view;
        readonly byte[] cis;
        string          _pcmciaCisText;
        PcmciaCisModel  _selectedCis;

        internal PcmciaInfoViewModel(byte[] pcmciaCis, Window view)
        {
            if(pcmciaCis == null)
                return;

            cis                  = pcmciaCis;
            cisList              = new ObservableCollection<PcmciaCisModel>();
            SavePcmciaCisCommand = ReactiveCommand.Create(ExecuteSavePcmciaCisCommand);

            _view = view;

            Tuple[] tuples = CIS.GetTuples(cis);

            if(tuples != null)
                foreach(Tuple tuple in tuples)
                {
                    string tupleCode;
                    string tupleDescription;

                    switch(tuple.Code)
                    {
                        case TupleCodes.CISTPL_NULL:
                        case TupleCodes.CISTPL_END: continue;
                        case TupleCodes.CISTPL_DEVICEGEO:
                        case TupleCodes.CISTPL_DEVICEGEO_A:
                            tupleCode        = "Device Geometry Tuples";
                            tupleDescription = CIS.PrettifyDeviceGeometryTuple(tuple);

                            break;
                        case TupleCodes.CISTPL_MANFID:
                            tupleCode        = "Manufacturer Identification Tuple";
                            tupleDescription = CIS.PrettifyManufacturerIdentificationTuple(tuple);

                            break;
                        case TupleCodes.CISTPL_VERS_1:
                            tupleCode        = "Level 1 Version / Product Information Tuple";
                            tupleDescription = CIS.PrettifyLevel1VersionTuple(tuple);

                            break;
                        case TupleCodes.CISTPL_ALTSTR:
                        case TupleCodes.CISTPL_BAR:
                        case TupleCodes.CISTPL_BATTERY:
                        case TupleCodes.CISTPL_BYTEORDER:
                        case TupleCodes.CISTPL_CFTABLE_ENTRY:
                        case TupleCodes.CISTPL_CFTABLE_ENTRY_CB:
                        case TupleCodes.CISTPL_CHECKSUM:
                        case TupleCodes.CISTPL_CONFIG:
                        case TupleCodes.CISTPL_CONFIG_CB:
                        case TupleCodes.CISTPL_DATE:
                        case TupleCodes.CISTPL_DEVICE:
                        case TupleCodes.CISTPL_DEVICE_A:
                        case TupleCodes.CISTPL_DEVICE_OA:
                        case TupleCodes.CISTPL_DEVICE_OC:
                        case TupleCodes.CISTPL_EXTDEVIC:
                        case TupleCodes.CISTPL_FORMAT:
                        case TupleCodes.CISTPL_FORMAT_A:
                        case TupleCodes.CISTPL_FUNCE:
                        case TupleCodes.CISTPL_FUNCID:
                        case TupleCodes.CISTPL_GEOMETRY:
                        case TupleCodes.CISTPL_INDIRECT:
                        case TupleCodes.CISTPL_JEDEC_A:
                        case TupleCodes.CISTPL_JEDEC_C:
                        case TupleCodes.CISTPL_LINKTARGET:
                        case TupleCodes.CISTPL_LONGLINK_A:
                        case TupleCodes.CISTPL_LONGLINK_C:
                        case TupleCodes.CISTPL_LONGLINK_CB:
                        case TupleCodes.CISTPL_LONGLINK_MFC:
                        case TupleCodes.CISTPL_NO_LINK:
                        case TupleCodes.CISTPL_ORG:
                        case TupleCodes.CISTPL_PWR_MGMNT:
                        case TupleCodes.CISTPL_SPCL:
                        case TupleCodes.CISTPL_SWIL:
                        case TupleCodes.CISTPL_VERS_2:
                            tupleCode        = $"Undecoded tuple ID {tuple.Code}";
                            tupleDescription = $"Undecoded tuple ID {tuple.Code}";

                            break;
                        default:
                            tupleCode        = $"0x{(byte)tuple.Code:X2}";
                            tupleDescription = $"Found unknown tuple ID 0x{(byte)tuple.Code:X2}";

                            break;
                    }

                    cisList.Add(new PcmciaCisModel
                    {
                        Code = tupleCode, Description = tupleDescription
                    });
                }
            else
                AaruConsole.DebugWriteLine("Device-Info command", "PCMCIA CIS returned no tuples");
        }

        public ObservableCollection<PcmciaCisModel> cisList { get; }

        public string PcmciaCisText
        {
            get => _pcmciaCisText;
            set => this.RaiseAndSetIfChanged(ref _pcmciaCisText, value);
        }

        public PcmciaCisModel SelectedCis
        {
            get => _selectedCis;
            set
            {
                if(_selectedCis == value)
                    return;

                PcmciaCisText = value?.Description;
                this.RaiseAndSetIfChanged(ref _selectedCis, value);
            }
        }

        public ReactiveCommand<Unit, Unit> SavePcmciaCisCommand { get; }

        async void ExecuteSavePcmciaCisCommand()
        {
            var dlgSaveBinary = new SaveFileDialog();

            dlgSaveBinary.Filters.Add(new FileDialogFilter
            {
                Extensions = new List<string>(new[]
                {
                    "*.bin"
                }),
                Name = "Binary"
            });

            string result = await dlgSaveBinary.ShowAsync(_view);

            if(result is null)
                return;

            var saveFs = new FileStream(result, FileMode.Create);
            saveFs.Write(cis, 0, cis.Length);

            saveFs.Close();
        }
    }
}