using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Merlin.Mfc;

namespace Merlin.DomainModel
{
    /// <summary>
    /// Most Hover objects, after deserialising the object data, include code
    /// // for loading a two byte count followed by a block of arbitrary bytes.
    /// As far as I can tell these bytes blocks aren't used by the program and
    /// are always of zero length in the data files.  This class handles
    /// deserialising them and throws an exception if it comes across a block
    /// of non-zero length (so it can be investigated).
    /// </summary>
    [MfcSerialisable]
    public class TrailingBytes : MfcObject
    {
        private byte[] trailingBytes;

        public override void Deserialise(MfcDeserialiser archive)
        {
            ushort byteCount = archive.DeserialiseUInt16();
            trailingBytes = archive.DeserialiseBytes(byteCount);

            if (trailingBytes.Length > 0)
            {
                throw new InvalidDataException("Trailing byte block with non-zero length; investigate!");
            }
        }

        public override void Serialise(MfcSerialiser archive)
        {
            archive.SerialiseUInt16(0);
        }
    }
}
