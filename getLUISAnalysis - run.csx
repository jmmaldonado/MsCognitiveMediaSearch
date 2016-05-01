#load "..\datacontracts.csx"

using System.Net;
using System.Collections;
using Newtonsoft.Json;


public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log) {
    log.Verbose("getLUISAnalysis function invoked");

    // Get request body - where the conversation text is in just plain/text
    dynamic data = await req.Content.ReadAsStringAsync();



    LUISResponse result = makeLUISCallFromText(data, log);



    //return object in HTTP Response
    return req.CreateResponse(HttpStatusCode.OK, result);
}






//////////////////////////////////////////7
//              LUIS STUFF
///////////////////////////////////////////


/// <summary>
/// Invoca a LUIS para obtener el JSON Resultante con los intents y entities de la consulta
/// Si el parámetro text tiene más de 500 caracteres se encarga de hacer el fragmentado y componer los resultados
/// </summary>
/// <param name="text">Texto plano para enviarle a LUIS</param>
/// <returns>Objeto con el datacontract del JSON resultante de la llamada de Luis, sin duplicados en intents ni entities</returns>
private static LUISResponse makeLUISCallFromText(string text, TraceWriter log) {
    try {

        int _LUISMaxCharacters = Int32.Parse(Environment.GetEnvironmentVariable("APPSETTING_LUISMaxCharacters"));
        List<LUISResponse> LUISResultObjectList = new List<LUISResponse>();

        //Comprobar si el texto tiene más de [Settings Numero Caracteres] caracteres, en ese caso fragmentarlo en los chunks correspondientes
        //Si el texto es menor, sólo hace falta una llamada a LUIS
        //TODO: Ojo que al hacer el EscapeDataString en getLUISResults se meten muchos caracteres nuevos por el escapado de acentos, etc...
        if (text.Length <= _LUISMaxCharacters) {
            //getLUISResults(text);
            LUISResultObjectList.Add(getLUISResultsAsObject(text, log));

        } else {

            //Necesitamos fragmentar el texto y recomponer los resultados
            int fragments = (int)Math.Ceiling((decimal)text.Length / _LUISMaxCharacters);
            int startIndex, numChars;
            for (int fragment = 0; fragment < fragments; fragment++) {
                startIndex = fragment * _LUISMaxCharacters;

                //Corregimos el numero de caracteres por si en el ultimo fragmento nos pasamos de la longitud del texto
                numChars = startIndex + _LUISMaxCharacters > text.Length ? text.Length - startIndex : _LUISMaxCharacters;

                //getLUISResults(text.Substring(startIndex, numChars));
                LUISResultObjectList.Add(getLUISResultsAsObject(text.Substring(startIndex, numChars), log));
            }

        }

        //Componer el resultado de varias llamadas a LUIS a una consolidada y eliminar duplicados en intents y entities
        LUISResponse response = composeFragmentedLUISResponses(LUISResultObjectList);
        Intents[] uniqueIntents = removeDuplicateIntentsFromArray(response.intents);
        Entities[] uniqueEntities = removeDuplicateEntitiesFromArray(response.entities);
        response.intents = uniqueIntents;
        response.entities = uniqueEntities;

        return response;

    } catch (Exception ex) { log.Error("[ERROR] makeLUISCallFromText - Exception: " + ex.Message); return null; }
}




//[DONE]
private static LUISResponse getLUISResultsAsObject(string text, TraceWriter log) {

    LUISResponse result;

    string _LUISQuery = Environment.GetEnvironmentVariable("APPSETTING_LUISBaseURI") + Environment.GetEnvironmentVariable("APPSETTING_LUISAppID") + "&subscription-key=" + Environment.GetEnvironmentVariable("APPSETTING_LUISKey") + "&q=";
    string completeQuery = _LUISQuery + text;

    log.Verbose("[VERBOSE] getLUISResultsAsObject: Full URL to invoke LUIS: " + completeQuery);

    WebClient webClient = new WebClient();
    string LUISStringResponse = webClient.DownloadString(completeQuery);

    //object LUISResult = JsonConvert.DeserializeObject(LUISStringResponse);


    result = JsonConvert.DeserializeObject<LUISResponse>(LUISStringResponse);

    /*
    DataContractJsonSerializer myjsonhelper = new DataContractJsonSerializer(typeof(LUISResponse));
    using (System.IO.MemoryStream stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(LUISStringResponse))) {
        result = (LUISResponse)myjsonhelper.ReadObject(stream);
    }*/

    log.Verbose("[VERBOSE] getLUISResultsAsObject: LUIS JSON response: ", LUISStringResponse);

    return result;
}



//[DONE]
//If we have duplicated Intents in the array we will keep the one with the highest score
//We may get these duplicates as we are calling LUIS multiple times to get around the LUISMaxCharacters limitation
private static Intents[] removeDuplicateIntentsFromArray(Intents[] intents) {

    Hashtable table = new Hashtable();
    foreach (Intents intent in intents) {
        if (table.ContainsKey(intent.intent)) {
            //Keep the highest scored intent
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


//[DONE]
//If we have duplicated Entities in the array we will keep the one with the highest score
//We may get these duplicates as we are calling LUIS multiple times to get around the LUISMaxCharacters limitation
private static Entities[] removeDuplicateEntitiesFromArray(Entities[] entities) {

    Hashtable table = new Hashtable();
    foreach (Entities entity in entities) {
        if (table.ContainsKey(entity.entity)) {
            //Keep the highest scored Entity
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



//[DONE]
private static LUISResponse composeFragmentedLUISResponses(List<LUISResponse> responseList) {
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

