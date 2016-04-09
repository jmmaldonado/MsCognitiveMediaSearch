using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml;

namespace MediaSearch.Model
{
    public class Helper
    {
        /// <summary>
        /// JSON Serialization
        /// </summary>
        public static string JsonSerializer<T>(T t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }

        /// <summary>
        /// JSON Deserialization
        /// </summary>
        public static T JsonDeserialize<T>(string jsonString)
        {
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), settings);
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(ms);
            ms.Close();
            return obj;
        }

        public static List<AudioTranscript> ParseAudioTranscript(MemoryStream ms)
        {
            List<AudioTranscript> audioT = new List<AudioTranscript>();

            try
            {
                var xmlReader = XmlReader.Create(ms);
                var xDoc = XDocument.Load(xmlReader);

                if (null != xDoc)
                {
                    var xNamespace = xDoc.Root.Name.Namespace;
                    var xAudioSamples = xDoc.Descendants(xNamespace + "p");

                    foreach (XElement xas in xAudioSamples)
                    {
                        var atrans = new AudioTranscript();
                        atrans.audioText = xas.Value;
                        atrans.timeMarker = xas.Attribute("begin").Value;

                        audioT.Add(atrans);
                    }
                }
            }
            catch (Exception x)
            {
                throw x;
            }

            return audioT;
        }
    }
}