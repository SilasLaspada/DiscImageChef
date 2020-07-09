﻿// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : NTFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef unit testing.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Filesystems;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class NtfsGpt
    {
        readonly string[] testfiles =
        {
            "haiku_hrev51259.aif"
        };

        readonly ulong[] sectors =
        {
            2097152
        };

        readonly uint[] sectorsize =
        {
            512
        };

        readonly long[] clusters =
        {
            261887
        };

        readonly int[] clustersize =
        {
            4096
        };

        readonly string[] volumename =
        {
            null, null, null, null, null, null, null, null
        };

        readonly string[] volumeserial =
        {
            "106DA7693F7F6B3F"
        };

        readonly string[] oemid =
        {
            null
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "New Technology File System (GPT)",
                                               testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new NTFS();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Microsoft Basic data")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual("NTFS", fs.XmlFsType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFsType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class NtfsMbr
    {
        readonly string[] testfiles =
        {
            "win10.aif", "win2000.aif", "winnt_3.10.aif", "winnt_3.50.aif", "winnt_3.51.aif", "winnt_4.00.aif",
            "winvista.aif", "linux.aif", "haiku_hrev51259.aif", "linux_4.19_ntfs3g_flashdrive.aif"
        };

        readonly ulong[] sectors =
        {
            524288, 2097152, 1024000, 524288, 524288, 524288, 524288, 262144, 2097152, 1024000
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            65263, 1046511, 1023057, 524256, 524256, 524096, 64767, 32511, 261887, 127743
        };

        readonly int[] clustersize =
        {
            4096, 1024, 512, 512, 512, 512, 4096, 4096, 4096, 4096
        };

        readonly string[] volumename =
        {
            null, null, null, null, null, null, null, null, null, null
        };

        readonly string[] volumeserial =
        {
            "C46C1B3C6C1B28A6", "8070C8EC70C8E9CC", "10CC6AC6CC6AA5A6", "7A14F50014F4BFE5", "24884447884419A6",
            "822C288D2C287E73", "E20AF54B0AF51D6B", "065BB96B7C1BCFDA", "46EC796749C6FA66", "1FC3802B52F9611C"
        };

        readonly string[] oemid =
        {
            null, null, null, null, null, null, null, null, null, null
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "New Technology File System (MBR)",
                                               testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new NTFS();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x07" ||

                       // Value incorrectly set by Haiku
                       partitions[j].Type == "0x86")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual("NTFS", fs.XmlFsType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFsType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}