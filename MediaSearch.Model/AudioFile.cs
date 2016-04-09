using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaSearch.Model
{
    [DataContract]
    public class AudioFile
    {
        public AudioFile()
        {
            AudioTranscripts = new List<AudioTranscript>();
        }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public List<AudioTranscript> AudioTranscripts { get; set; }
    }
}
