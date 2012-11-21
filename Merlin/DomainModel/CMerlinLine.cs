using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Merlin.Mfc;
using System.ComponentModel;

namespace Merlin.DomainModel
{
    [MfcSerialisable("CMerlinLine")]
    public class CMerlinLine : CMerlinObject
    {
        [Category("Position")]
        public ushort X1 { get; set; }

        [Category("Position")]
        public ushort Y1 { get; set; }

        [Category("Position")]
        public ushort X2 { get; set; }

        [Category("Position")]
        public ushort Y2 { get; set; }

        public override void Deserialise(MfcDeserialiser archive)
        {
            base.Deserialise(archive);

            X1 = archive.DeserialiseUInt16();
            Y1 = archive.DeserialiseUInt16();
            X2 = archive.DeserialiseUInt16();
            Y2 = archive.DeserialiseUInt16();

            archive.DeserialiseObjectNoHeader<TrailingBytes>();
        }
    }
}
