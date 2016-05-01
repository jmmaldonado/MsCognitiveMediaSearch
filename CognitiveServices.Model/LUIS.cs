using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization.Json;

namespace CognitiveServices.Model
{
    public class LUIS
    {

        public string AppId { get; set; }
        public string AppKey { get; set; }
        public int MaxCharacters { get; set; }
        private string _LUISQuery;


        public LUIS (string appId, string appKey, int maxCharacters) {
            AppId = appId;
            AppKey = appKey;
            MaxCharacters = maxCharacters;

            _LUISQuery = "https://api.projectoxford.ai/luis/v1/application?id=" + this.AppId + "&subscription-key=" + this.AppKey + "&q=";
        }




        /// <summary>
        /// Invoca a LUIS para obtener el JSON Resultante con los intents y entities de la consulta
        /// Si el parámetro text tiene más de 500 caracteres se encarga de hacer el fragmentado y componer los resultados
        /// </summary>
        /// <param name="text">Texto plano para enviarle a LUIS</param>
        /// <returns>Objeto con el datacontract del JSON resultante de la llamada de Luis, sin duplicados en intents ni entities</returns>
        public LUISResponse makeLUISCallFromText(string text) {
            try {

                List<LUISResponse> LUISResultObjectList = new List<LUISResponse>();

                //Comprobar si el texto tiene más de [Settings Numero Caracteres] caracteres, en ese caso fragmentarlo en los chunks correspondientes
                //Si el texto es menor, sólo hace falta una llamada a LUIS
                //TODO: Ojo que al hacer el EscapeDataString en getLUISResults se meten muchos caracteres nuevos por el escapado de acentos, etc...
                if (text.Length <= MaxCharacters) {
                    //getLUISResults(text);
                    LUISResultObjectList.Add(getLUISResultsAsObject(text));

                } else {

                    //Necesitamos fragmentar el texto y recomponer los resultados
                    int fragments = (int)Math.Ceiling((decimal)text.Length / MaxCharacters);
                    int startIndex, numChars;
                    for (int fragment = 0; fragment < fragments; fragment++) {
                        startIndex = fragment * MaxCharacters;

                        //Corregimos el numero de caracteres por si en el ultimo fragmento nos pasamos de la longitud del texto
                        numChars = startIndex + MaxCharacters > text.Length ? text.Length - startIndex : MaxCharacters;

                        //getLUISResults(text.Substring(startIndex, numChars));
                        LUISResultObjectList.Add(getLUISResultsAsObject(text.Substring(startIndex, numChars)));
                    }

                }

                //Componer el resultado de varias llamadas a LUIS a una consolidada y eliminar duplicados en intents y entities
                LUISResponse response = composeFragmentedLUISResponses(LUISResultObjectList);
                Intents[] uniqueIntents = removeDuplicateIntentsFromArray(response.intents);
                Entities[] uniqueEntities = removeDuplicateEntitiesFromArray(response.entities);
                response.intents = uniqueIntents;
                response.entities = uniqueEntities;

                return response;

            } catch (Exception ex) { Trace.TraceError("[ERROR] makeLUISCallFromText exception: " + ex.Message); return null; }
        }




        #region Private methods


        private LUISResponse composeFragmentedLUISResponses(List<LUISResponse> responseList) {
            LUISResponse result = new LUISResponse();
            result.query = "";
            result.intents = new List<Intents>().ToArray();
            result.entities = new List<Entities>().ToArray();

            foreach (LUISResponse response in responseList) {
                result.query += response.query + " ";
                result.intents = result.intents.ToList().Concat(response.intents.ToList()).ToArray();
                result.entities = result.entities.ToList().Concat(response.entities.ToList()).ToArray();
            }

            return result;
        }

        private Intents[] removeDuplicateIntentsFromArray(Intents[] intents) {

            Hashtable table = new Hashtable();
            foreach (Intents intent in intents) {
                if (table.ContainsKey(intent.intent)) {
                    //De todos los intents iguales que encontremos nos quedamos con el de mayor score
                    if (((Intents)table[intent.intent]).score < intent.score) {
                        table[intent.intent] = intent;
                    }
                } else {
                    table.Add(intent.intent, intent);
                }
            }

            List<Intents> result = new List<Intents>();
            foreach (object resultIntent in table.Values)
                result.Add((Intents)resultIntent);

            return result.ToArray();
        }

        private Entities[] removeDuplicateEntitiesFromArray(Entities[] entities) {

            Hashtable table = new Hashtable();
            foreach (Entities entity in entities) {
                if (table.ContainsKey(entity.entity)) {
                    //De todos los entities iguales que encontremos nos quedamos con el de mayor score
                    if (((Entities)table[entity.entity]).score < entity.score) {
                        table[entity.entity] = entity;
                    }
                } else {
                    table.Add(entity.entity, entity);
                }
            }

            List<Entities> result = new List<Entities>();
            foreach (object resultEntity in table.Values)
                result.Add((Entities)resultEntity);

            return result.ToArray();
        }

        private LUISResponse getLUISResultsAsObject(string text) {

            LUISResponse result;

            string completeQuery = _LUISQuery + text;

            Trace.TraceInformation("[VERBOSE] getLUISResultsAsObject: LUIS URL: " + completeQuery);

            WebClient webClient = new WebClient();
            string LUISStringResponse = webClient.DownloadString(completeQuery);

            //object LUISResult = JsonConvert.DeserializeObject(LUISStringResponse);


            DataContractJsonSerializer myjsonhelper = new DataContractJsonSerializer(typeof(LUISResponse));
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(LUISStringResponse))) {
                result = (LUISResponse)myjsonhelper.ReadObject(stream);
            }

            Trace.TraceInformation("[VERBOSE] getLUISResultsAsObject: LUIS JSON response: " + LUISStringResponse);

            return result;

        }

        #endregion



    }
}
