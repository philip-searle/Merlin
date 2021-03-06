﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Merlin.Mfc;

namespace Merlin.DomainModel
{
    [MfcSerialisable("CMerlinBSP")]
    public class CMerlinBsp : CMerlinLine
    {
        public ushort unknown01 { get; set; } // CMerlinBsp ID
        public ushort unknown02 { get; set; } // Index of CMerlinStatic to check position against
        public ushort unknown03 { get; set; } // ID of CMerlinBsp on the left of the line (0xffff if none)
        public ushort unknown04 { get; set; } // ID of CMerlinBsp on the right of the line (0xffff if none)

        public override void Deserialise(MfcDeserialiser archive)
        {
            base.Deserialise(archive);

            unknown01 = archive.DeserialiseUInt16();
            unknown02 = archive.DeserialiseUInt16();
            unknown03 = archive.DeserialiseUInt16();
            unknown04 = archive.DeserialiseUInt16();

            int unknownCount01 = archive.DeserialiseUInt16();

            byte[] unknown64BitValue01 = archive.DeserialiseBytes(8);
            byte[] unknown64BitValue02 = archive.DeserialiseBytes(8);

            archive.DeserialiseObjectNoHeader<TrailingBytes>();

            // Ensure unknown values are zero
            if (unknownCount01 != 0) throw new InvalidOperationException("unknownCount01 != 0");
            if (unknown64BitValue01.Any(b => b != 0)) throw new InvalidOperationException("unknown64BitValid01 non-zero");
            if (unknown64BitValue02.Any(b => b != 0)) throw new InvalidOperationException("unknown64BitValid02 non-zero");
        }

        public override void Serialise(MfcSerialiser archive)
        {
            base.Serialise(archive);

            archive.SerialiseUInt16(unknown01);
            archive.SerialiseUInt16(unknown02);
            archive.SerialiseUInt16(unknown03);
            archive.SerialiseUInt16(unknown04);

            archive.SerialiseUInt16(0);

            archive.SerialiseBytes(new byte[8]);
            archive.SerialiseBytes(new byte[8]);

            archive.SerialiseObjectNoHeader(new TrailingBytes());
        }
    }
}
