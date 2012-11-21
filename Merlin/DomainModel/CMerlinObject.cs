using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Merlin.Mfc;
using System.ComponentModel;

namespace Merlin.DomainModel
{
    [MfcSerialisable]
    public class CMerlinObject : MfcObject
    {
        [Category("Identity")]
        public string Name { get; set; }

        public override void Deserialise(MfcDeserialiser archive)
        {
            Name = archive.DeserialiseString();
            archive.DeserialiseObjectNoHeader<TrailingBytes>();
        }

        public override void Serialise(MfcSerialiser archive)
        {
            archive.SerialiseString(Name);
            archive.SerialiseObjectNoHeader(new TrailingBytes());
        }
    }
}
