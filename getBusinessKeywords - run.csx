using System.Net;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text.RegularExpressions;


public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log) {

    log.Verbose("getBusinessKeywords function invoked");

    // Get request body - where the conversation text is in just plain/text
    dynamic data = await req.Content.ReadAsStringAsync();

    //Prepare dictionaries
    List<string> businessDictionary = prepareDictionary("business_keywords.txt");
    List<string> uselessDictionary = prepareDictionary("useless_keywords.txt");

    //Process query against dictionaries
    string[] businessKeywords = getKeywordsFromQueryAsArray(data, businessDictionary, log);
    string filteredQuery = getFilteredQuery(data, uselessDictionary, log);

    //Compose result object
    returnObject result = new returnObject();
    result.businessKeywords = stringifyArray(businessKeywords);
    result.filteredQuery = filteredQuery;

    //return object in HTTP Response
    return req.CreateResponse(HttpStatusCode.OK, result);

}



//UTILITIES


public static string stringifyArray(string[] array) {
    string result = "";
    foreach (string item in array)
        result += item + " ";
    return result;
}



public static string stringifyList(List<string> array) {
    string result = "";
    foreach (var item in array)
        result += item.ToString() + " ";
    return result;
}







public static List<string> prepareDictionary(string dictionaryName) {
    List<string> result = new List<string>();

    string storageAccountName = Environment.GetEnvironmentVariable("APPSETTING_dictionariesStorageAccount");
    string dictionaryURI = "https://" + storageAccountName + ".blob.core.windows.net/dictionaries/" + dictionaryName;


    WebClient client = new WebClient();
    Stream stream = client.OpenRead(dictionaryURI);
    StreamReader reader = new StreamReader(stream);

    string line;

    while ((line = reader.ReadLine()) != null)
        result.Add(line);

    reader.Close();

    return result;
}








private static string[] getKeywordsFromQueryAsArray(string query, List<string> dictionary, TraceWriter log) {
    List<string> list = new List<string>();
    try {
        log.Verbose("Obtaining financial keywords from query as array");
        string[] temp = query.Split(' ');

        foreach (string word in temp)
            if (dictionary.Contains(word))
                list.Add(word);

        log.Verbose("Finished business keyword analysis: " + list.Count + " terms found");
        log.Verbose("[VERBOSE] Business keywords found: " + stringifyList(list));

        return list.ToArray();
    } catch (Exception ex) { log.Verbose("[ERROR] Error in getFinancialKeywordsFromQueryAsArray", ex.Message); return list.ToArray(); }
}







private static string getFilteredQuery(string query, List<string> uselessDictionary, TraceWriter log) {
    try {
        string result = query;

        result = result.Replace("...", " ");
        result = result.Replace(".", "");
        result = result.Replace(",", "");
        result = result.Replace(":", "");
        result = Regex.Replace(result, "\\b" + String.Join("\\b|\\b", uselessDictionary.ToArray()) + "\\b", "", RegexOptions.IgnoreCase);

        log.Verbose("[VERBOSE] Resulting query after removing useless words: " + result);

        return result;
    } catch (Exception ex) { log.Verbose("[ERROR] Exception in getFilteredQuery: " + ex.Message); return query; }
}






/* Object used to represent the results of the function */
public class returnObject {
    public string businessKeywords { get; set; }
    public string filteredQuery { get; set; }
}