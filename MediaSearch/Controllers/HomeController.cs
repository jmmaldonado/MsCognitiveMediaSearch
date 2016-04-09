using MediaSearch.Model;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace MediaSearch.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Media Search - BBVA PoC";
 
            return View();
        }

        public ActionResult Search(string query)
        {
            ViewBag.Query = query;

            var searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
            var queryApiKey = ConfigurationManager.AppSettings["SearchServiceQueryApiKey"];

            var indexClient = new SearchIndexClient(searchServiceName, "temp", new SearchCredentials(queryApiKey));

            DocumentSearchResult results = indexClient.Documents.Search(query);

            var audioResults = new List<AudioFile>();

            foreach (var item in results.Results)
            {
                var audioFile = new AudioFile();

                audioFile.Title = item.Document["Title"].ToString();

                var audiotranscripts = item.Document["AudioTranscripts"] as string[];

                foreach (var at in audiotranscripts)
                {
                    if (at.Contains(query))
                        audioFile.AudioTranscripts.Add(Helper.JsonDeserialize<AudioTranscript>(at));
                }

                audioResults.Add(audioFile);
            }

            return View(audioResults);
        }
    }
}
