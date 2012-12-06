using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Merlin.Mfc;
using System.ComponentModel;

namespace Merlin.DomainModel
{
    [MfcSerialisable("CMerlinLocation")]
    public class CMerlinLocation : CMerlinObject
    {
        public enum Rotation {
            East = 0,
            South = 128,
            West = 256,
            North = 384
        };

        [Category("Position")]
        public ushort X { get; set; }

        [Category("Position")]
        public ushort Y { get; set; }

        [Category("Location")]
        public Rotation FacingDirection { get; set; }

        [Category("Position")]
        public ushort Z { get; set; }

        public override void Deserialise(MfcDeserialiser archive)
        {
            base.Deserialise(archive);

            X = archive.DeserialiseUInt16();
            Y = archive.DeserialiseUInt16();
            FacingDirection = (Rotation)archive.DeserialiseUInt16();
            Z = archive.DeserialiseUInt16();

            archive.DeserialiseObjectNoHeader<TrailingBytes>();
        }

        public override void Serialise(MfcSerialiser archive)
        {
            base.Serialise(archive);

            archive.SerialiseUInt16(X);
            archive.SerialiseUInt16(Y);
            archive.SerialiseUInt16((ushort)FacingDirection);
            archive.SerialiseUInt16(Z);

            archive.SerialiseObjectNoHeader(new TrailingBytes());
        }
    }
}
