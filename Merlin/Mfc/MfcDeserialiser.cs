using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Merlin.Mfc
{
    public class MfcDeserialiser
    {
        const UInt16 NullTag = 0;
        const UInt16 NewClassTag = 0xffff;
        const UInt16 ClassTag = 0x8000;
        const UInt32 BigClassTag = 0x80000000;
        const UInt16 BigObjectTag = 0x7fff;
        const UInt32 MaxMapCountTag = 0x3ffffffe;

        private BinaryReader _stream;
        private MfcClassRegistry _classRegistry;
        private List<MfcClass> _loadedClasses;
        private List<MfcObject> _loadedObjects;

        public MfcDeserialiser(Stream input, MfcClassRegistry classRegistry)
        {
            this._stream = new BinaryReader(input);
            this._classRegistry = classRegistry;
            this._loadedClasses = new List<MfcClass>();
            this._loadedObjects = new List<MfcObject>();

            // Class index zero isn't used/represents an error
            _loadedClasses.Add(null);

            // Object index zero isn't used/represents a null pointer
            _loadedObjects.Add(null);
        }

        public BinaryReader Stream { get { return _stream; } }

        public string PeekNextObjectType()
        {
            string nextObjectType;
            var savedPosition = _stream.BaseStream.Position;

            uint objectTag;
            MfcClass mfcClass = ReadClass(out objectTag);
            if (mfcClass == null)
            {
                if (objectTag > _loadedObjects.Count)
                {
                    throw new InvalidDataException("Got an object tag larger than the count of loaded objects: " + objectTag);
                }
                nextObjectType = _loadedObjects[(int)objectTag].GetType().Name;
            }
            else
            {
                nextObjectType = mfcClass.Name;
            }

            _stream.BaseStream.Position = savedPosition;
            return nextObjectType;
        }

        /// <summary>
        /// Deserialises an object of type T without reading in the header.
        /// This implies it must be a new object and not one that has already
        /// been loaded since there is no object tag to reference the loaded list.
        /// </summary>
        public T DeserialiseObjectNoHeader<T>() where T : MfcObject
        {
            MfcClass mfcClass = _classRegistry.GetMfcClass(typeof(T));
            return DeserialiseNewObject<T>(mfcClass);
        }

        public T DeserialiseObject<T>() where T : MfcObject
        {
            uint objectTag;
            MfcClass mfcClass = ReadClass(out objectTag);

            if (mfcClass == null)
            {
                // An object we've already loaded
                if (objectTag > _loadedObjects.Count)
                {
                    throw new InvalidDataException("Got an object tag larger than the count of loaded objects: " + objectTag);
                }

                return (T)_loadedObjects[(int)objectTag];
            }

            // An object we haven't yet loaded.  Create a new instance and deserialise it.
            // Make sure to add it to the list of loaded objects before deserialising in
            // case it has (possibly indirect) references to itself.
            return DeserialiseNewObject<T>(mfcClass);
        }

        private T DeserialiseNewObject<T>(MfcClass mfcClass) where T : MfcObject
        {
            MfcObject newObject = mfcClass.CreateNewObject<MfcObject>();
            _loadedObjects.Add(newObject);
            newObject.Deserialise(this);
            return (T)newObject;
        }

        /// <summary>
        /// Tries to read in and return the MfcClass that is next in the stream.
        /// If the next object has already been loaded then null is returned and
        /// alreadyLoadedTag is set to the object ID of the object.
        /// </summary>
        private MfcClass ReadClass(out uint alreadyLoadedTag)
        {
            ushort tag = _stream.ReadUInt16();
            uint objectTag = ((uint)(tag & ClassTag) << 16) | (uint)(tag & ~ClassTag);
            if (tag == BigObjectTag)
            {
                objectTag = _stream.ReadUInt32();
            }

            // If it is an object tag and not a class tag then bail out -- caller will handle it
            alreadyLoadedTag = 0;
            if ((objectTag & BigClassTag) == 0)
            {
                alreadyLoadedTag = objectTag;
                return null;
            }

            MfcClass mfcClass;
            if (tag == NewClassTag)
            {
                // Not a class we've seen before; read it in
                mfcClass = ReadNewClass();
                _loadedClasses.Add(mfcClass);
                return mfcClass;
            }

            // A class we've seen before, look it up by index
            uint classIndex = objectTag & ~BigClassTag;
            if (classIndex == 0)
            {
                throw new InvalidDataException("Got a invalid class index: 0");
            }
            if (classIndex > _loadedClasses.Count)
            {
                throw new InvalidDataException("Got a class index larger than the currently loaded count: " + classIndex);
            }

            return _loadedClasses[(int)classIndex];
        }

        private MfcClass ReadNewClass()
        {
            ushort schemaVersion = _stream.ReadUInt16();
            ushort classNameLength = _stream.ReadUInt16();
            string className = _stream.ReadAsciiString(classNameLength);

            MfcClass mfcClass = _classRegistry.GetMfcClass(className);
            
            if (mfcClass == null)
            {
                throw new InvalidDataException("No registered class for MfcObject " + className);
            }
            if (mfcClass.SchemaVersion != schemaVersion)
            {
                throw new InvalidDataException("Schema mismatch: file = " + schemaVersion + ", registered = " + mfcClass.SchemaVersion);
            }

            return mfcClass;
        }
    }

    public class MfcClass
    {
        public string Name { get; set; }
        public int SchemaVersion { get; set; }
        public Type RealType { get; set; }

        internal T CreateNewObject<T>() where T : IMfcSerialisable<T>
        {
            if (RealType.IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException("Cannot assign " + RealType.Name + " from " + typeof(T).Name);
            }
            return (T)RealType.Assembly.CreateInstance(RealType.FullName);
        }
    }

    internal static class BinaryReaderExtensions
    {
        public static string ReadAsciiString(this BinaryReader stream, int length)
        {
            var bytes = stream.ReadBytes(length);
            return Encoding.ASCII.GetString(bytes);
        }
    }

    public static class MfcDeserialiserExtensions
    {
        public static uint DeserialiseUInt32(this MfcDeserialiser archive)
        {
            return archive.Stream.ReadUInt32();
        }

        public static ushort DeserialiseUInt16(this MfcDeserialiser archive)
        {
            return archive.Stream.ReadUInt16();
        }

        public static byte DeserialiseByte(this MfcDeserialiser archive)
        {
            return archive.Stream.ReadByte();
        }

        public static byte[] DeserialiseBytes(this MfcDeserialiser archive, int length)
        {
            return archive.Stream.ReadBytes(length);
        }

        /// <summary>
        /// Deserialise an MFC-format ASCII CString
        /// </summary>
        public static string DeserialiseString(this MfcDeserialiser archive)
        {
            int length = ReadStringLength(archive.Stream);
            return archive.Stream.ReadAsciiString(length);
        }

        private static int ReadStringLength(BinaryReader stream)
        {
            byte bLength = stream.ReadByte();
            if (bLength < 0xff)
            {
                return bLength;
            }

            ushort wLength = stream.ReadUInt16();
            if (wLength < 0xfffe)
            {
                return wLength;
            }
            if (wLength == 0xfffe)
            {
                // Unicode string prefix -- not currently handled.  See 
                // CArchive::operator>>(CArchive& ar, CString& string)
                // for details on how to implement this when needed.
            }

            return stream.ReadInt32();
        }

        public static List<T> DeserialiseBuggyList<T>(this MfcDeserialiser archive) where T : MfcObject
        {
            List<T> result = new List<T>();
            ushort listLength = archive.DeserialiseUInt16();

            if (listLength >= 1)
            {
                // First object has a valid runtime class header
                result.Add(archive.DeserialiseObject<T>());
            }
            for (int i = 1; i < listLength; i++)
            {
                // Subsequent objects are missing the runtime class header but have a word preceding them
                // that looks like an invalid runtime classs header.
                uint test = archive.DeserialiseUInt16();
                result.Add(archive.DeserialiseObjectNoHeader<T>());
            }

            return result;
        }
    }
}
