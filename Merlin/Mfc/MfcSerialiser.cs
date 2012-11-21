using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Merlin.Mfc
{
    public class MfcSerialiser
    {
        const UInt16 NullTag = 0;
        const UInt16 NewClassTag = 0xffff;
        const UInt16 ClassTag = 0x8000;
        const UInt32 BigClassTag = 0x80000000;
        const UInt16 BigObjectTag = 0x7fff;
        const UInt32 MaxMapCountTag = 0x3ffffffe;

        private BinaryWriter _stream;
        private MfcClassRegistry _classRegistry;
        private List<MfcClass> _loadedClasses;
        private List<MfcObject> _loadedObjects;

        public MfcSerialiser(Stream output, MfcClassRegistry classRegistry)
        {
            this._stream = new BinaryWriter(output);
            this._classRegistry = classRegistry;
            this._loadedClasses = new List<MfcClass>();
            this._loadedObjects = new List<MfcObject>();

            // Class index zero isn't used/represents an error
            _loadedClasses.Add(null);

            // Object index zero isn't used/represents a null pointer
            _loadedObjects.Add(null);
        }

        public BinaryWriter Stream { get { return _stream; } }

        /// <summary>
        /// Serialises an object without writing out the header.
        /// </summary>
        public void SerialiseObjectNoHeader(MfcObject mfcObject)
        {
            _loadedObjects.Add(mfcObject);
            mfcObject.Serialise(this);
        }

        public void SerialiseObject(MfcObject mfcObject)
        {
            if (mfcObject == null)
            {
                _stream.Write((ushort)NullTag);
                return;
            }

            int prevObjectIndex = _loadedObjects.IndexOf(mfcObject);
            if (prevObjectIndex >= 0)
            {
                // We have already serialised this object and do not need to do so again
                if (prevObjectIndex >= BigObjectTag)
                {
                    throw new NotImplementedException("Object count >= 0x7fff not yet supported");
                }
                _stream.Write((ushort)prevObjectIndex);
                return;
            }

            // A new object, possibly with a new class
            _loadedObjects.Add(mfcObject);
            MfcClass mfcClass = _classRegistry.GetMfcClass(mfcObject.GetType());
            int prevClassIndex = _loadedClasses.IndexOf(mfcClass);
            if (prevClassIndex <= 0)
            {
                // A new class
                _loadedClasses.Add(mfcClass);
                _stream.Write((ushort)NewClassTag);
                _stream.Write((ushort)mfcClass.SchemaVersion);
                _stream.Write((ushort)mfcClass.Name.Length);
                _stream.WriteAsciiString(mfcClass.Name);
            }
            else
            {
                // A previously written class
                _stream.Write((ushort)(prevClassIndex | ClassTag));
            }

            mfcObject.Serialise(this);
        }
    }

    internal static class BinaryWriterExtensions
    {
        public static void WriteAsciiString(this BinaryWriter stream, string value)
        {
            stream.Write(Encoding.ASCII.GetBytes(value));
        }
    }

    public static class MfcSerialiserExtensions
    {
        public static void SerialiseUInt32(this MfcSerialiser archive, uint value)
        {
            archive.Stream.Write((uint)value);
        }

        public static void SerialiseUInt16(this MfcSerialiser archive, ushort value)
        {
            archive.Stream.Write((ushort)value);
        }

        public static void SerialiseByte(this MfcSerialiser archive, byte value)
        {
            archive.Stream.Write((byte)value);
        }

        public static void SerialiseBytes(this MfcSerialiser archive, byte[] value)
        {
            archive.Stream.Write(value);
        }

        /// <summary>
        /// Serialise an MFC-format ASCII CString
        /// </summary>
        public static void SerialiseString(this MfcSerialiser archive, string value)
        {
            WriteStringLength(archive.Stream, value);
            archive.Stream.WriteAsciiString(value);
        }

        private static void WriteStringLength(BinaryWriter stream, string value)
        {
            var length = value.Length;

            stream.Write((byte)Math.Min(length, 0xff));
            if (length < 0xff)
            {
                return;
            }

            stream.Write((ushort)Math.Min(length, 0xffff));
            if (length < 0xffff)
            {
                return;
            }

            stream.Write((uint)length);
        }

        public static void SerialiseBuggyList<T>(this MfcSerialiser archive, List<T> list) where T : MfcObject
        {
            if (list.Count > ushort.MaxValue)
            {
                throw new NotImplementedException("List length > 0xffff not supported");
            }

            archive.SerialiseUInt16((ushort)list.Count);

            foreach (var item in list)
            {
                archive.SerialiseObject(item);
            }
        }
    }
}
