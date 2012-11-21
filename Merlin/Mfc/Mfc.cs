
namespace Merlin.Mfc
{
    public interface IMfcSerialisable<T> where T : IMfcSerialisable<T>
    {
        void Deserialise(MfcDeserialiser archive);
        void Serialise(MfcSerialiser archive);
    }

    public abstract class MfcObject : IMfcSerialisable<MfcObject>
    {
        public abstract void Deserialise(MfcDeserialiser archive);
        public abstract void Serialise(MfcSerialiser archive);
    }
}
