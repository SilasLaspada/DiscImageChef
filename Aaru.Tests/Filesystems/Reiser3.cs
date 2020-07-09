﻿// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Reiser3.cs
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
    public class Reiser3
    {
        readonly string[] testfiles =
        {
            "linux_r3.5.aif", "linux_r3.6.aif", "linux_4.19_reiser_3.5_flashdrive.aif",
            "linux_4.19_reiser_3.6_flashdrive.aif"
        };

        readonly ulong[] sectors =
        {
            262144, 262144, 1024000, 1024000
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            32512, 32512, 127744, 127744
        };

        readonly int[] clustersize =
        {
            4096, 4096, 4096, 4096
        };

        readonly string[] reiserversion =
        {
            "Reiser 3.5 filesystem", "Reiser 3.6 filesystem", "Reiser 3.5 filesystem", "Reiser 3.6 filesystem"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "Reiser filesystem v3",
                                               testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new Reiser();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x83")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual(reiserversion[i], fs.XmlFsType.Type, testfiles[i]);
            }
        }
    }
}