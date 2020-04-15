using System.Collections.ObjectModel;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.ATA;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Decoders.Xbox;
using Aaru.Gui.Models;
using ReactiveUI;
using BCA = Aaru.Decoders.Bluray.BCA;
using Cartridge = Aaru.Decoders.DVD.Cartridge;
using DDS = Aaru.Decoders.DVD.DDS;
using DMI = Aaru.Decoders.Xbox.DMI;
using Inquiry = Aaru.Decoders.SCSI.Inquiry;
using Spare = Aaru.Decoders.DVD.Spare;

namespace Aaru.Gui.ViewModels
{
    public class DecodeMediaTagsViewModel : ViewModelBase
    {
        const int HEX_COLUMNS = 32;
        string    _decodedText;
        bool      _decodedVisible;
        string    _hexViewText;

        MediaTagModel _selectedTag;

        public DecodeMediaTagsViewModel(IMediaImage inputFormat)
        {
            TagsList = new ObservableCollection<MediaTagModel>();

            foreach(MediaTagType tag in inputFormat.Info.ReadableMediaTags)
                try
                {
                    byte[] data = inputFormat.ReadDiskTag(tag);

                    TagsList.Add(new MediaTagModel
                    {
                        Tag = tag, Data = data
                    });
                }
                catch
                {
                    //ignore
                }
        }

        public string                              Title    { get; }
        public ObservableCollection<MediaTagModel> TagsList { get; }

        public MediaTagModel SelectedTag
        {
            get => _selectedTag;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedTag, value);

                if(value is null)
                    return;

                // TODO: Decoders should be able to handle tags with/without length header
                HexViewText    = PrintHex.ByteArrayToHexArrayString(value.Data, HEX_COLUMNS);
                DecodedVisible = true;

                if(value.Decoded != null)
                {
                    DecodedText = value.Decoded;

                    return;
                }

                switch(value.Tag)
                {
                    case MediaTagType.CD_TOC:
                        DecodedText = TOC.Prettify(value.Data);

                        break;
                    case MediaTagType.CD_SessionInfo:
                        DecodedText = Session.Prettify(value.Data);

                        break;
                    case MediaTagType.CD_FullTOC:
                        DecodedText = FullTOC.Prettify(value.Data);

                        break;
                    case MediaTagType.CD_PMA:
                        DecodedText = PMA.Prettify(value.Data);

                        break;
                    case MediaTagType.CD_ATIP:
                        DecodedText = ATIP.Prettify(value.Data);

                        break;
                    case MediaTagType.CD_TEXT:
                        DecodedText = CDTextOnLeadIn.Prettify(value.Data);

                        break;
                    case MediaTagType.CD_MCN:
                        DecodedText = Encoding.ASCII.GetString(value.Data);

                        break;
                    case MediaTagType.DVD_PFI:
                        DecodedText = PFI.Prettify(value.Data);

                        break;
                    case MediaTagType.DVD_CMI:
                        DecodedText = CSS_CPRM.PrettifyLeadInCopyright(value.Data);

                        break;
                    case MediaTagType.DVDRAM_DDS:
                        DecodedText = DDS.Prettify(value.Data);

                        break;
                    case MediaTagType.DVDRAM_SpareArea:
                        DecodedText = Spare.Prettify(value.Data);

                        break;
                    case MediaTagType.DVDR_PFI:
                        DecodedText = PFI.Prettify(value.Data);

                        break;
                    case MediaTagType.HDDVD_MediumStatus:
                        DecodedText = PFI.Prettify(value.Data);

                        break;
                    case MediaTagType.BD_DI:
                        DecodedText = DI.Prettify(value.Data);

                        break;
                    case MediaTagType.BD_BCA:
                        DecodedText = BCA.Prettify(value.Data);

                        break;
                    case MediaTagType.BD_DDS:
                        DecodedText = Decoders.Bluray.DDS.Prettify(value.Data);

                        break;
                    case MediaTagType.BD_CartridgeStatus:
                        DecodedText = Cartridge.Prettify(value.Data);

                        break;
                    case MediaTagType.BD_SpareArea:
                        DecodedText = Decoders.Bluray.Spare.Prettify(value.Data);

                        break;
                    case MediaTagType.MMC_WriteProtection:
                        DecodedText = WriteProtect.PrettifyWriteProtectionStatus(value.Data);

                        break;
                    case MediaTagType.MMC_DiscInformation:
                        DecodedText = DiscInformation.Prettify(value.Data);

                        break;
                    case MediaTagType.SCSI_INQUIRY:
                        DecodedText = Inquiry.Prettify(value.Data);

                        break;
                    case MediaTagType.SCSI_MODEPAGE_2A:
                        DecodedText = Modes.PrettifyModePage_2A(value.Data);

                        break;
                    case MediaTagType.ATA_IDENTIFY:
                    case MediaTagType.ATAPI_IDENTIFY:
                        DecodedText = Identify.Prettify(value.Data);

                        break;
                    case MediaTagType.Xbox_SecuritySector:
                        DecodedText = SS.Prettify(value.Data);

                        break;
                    case MediaTagType.SCSI_MODESENSE_6:
                        DecodedText = Modes.PrettifyModeHeader6(value.Data, PeripheralDeviceTypes.DirectAccess);

                        break;
                    case MediaTagType.SCSI_MODESENSE_10:
                        DecodedText = Modes.PrettifyModeHeader10(value.Data, PeripheralDeviceTypes.DirectAccess);

                        break;
                    case MediaTagType.Xbox_DMI:
                        DecodedText = DMI.IsXbox360(value.Data) ? DMI.PrettifyXbox360(value.Data)
                                          : DMI.PrettifyXbox(value.Data);

                        break;
                    default:
                        DecodedVisible = false;

                        break;
                }

                if(DecodedText != null)
                    value.Decoded = DecodedText;
            }
        }

        public string HexViewText
        {
            get => _hexViewText;
            set => this.RaiseAndSetIfChanged(ref _hexViewText, value);
        }

        public bool DecodedVisible
        {
            get => _decodedVisible;
            set => this.RaiseAndSetIfChanged(ref _decodedVisible, value);
        }

        public string DecodedText
        {
            get => _decodedText;
            set => this.RaiseAndSetIfChanged(ref _decodedText, value);
        }
    }
}