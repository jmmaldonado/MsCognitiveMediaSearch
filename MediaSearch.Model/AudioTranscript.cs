using System.Runtime.Serialization;

namespace MediaSearch.Model
{
    [DataContract]
    public class AudioTranscript
    {
        [DataMember]
        public string timeMarker;
        [DataMember]
        public string audioText;
    }
}
