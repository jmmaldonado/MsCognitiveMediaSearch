using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using CognitiveServices.Model;
using System.Diagnostics;
using System.IO;

namespace MediaSearch.Controllers
{
    public class LuisController : Controller
    {
        // GET: Luis
        public ActionResult Index()
        {

            string LUISAppId = ConfigurationManager.AppSettings["LUISAppId"];
            string LUISAppKey = ConfigurationManager.AppSettings["LUISAppKey"];
            string LUISMaxCharacters = ConfigurationManager.AppSettings["LUISMaxCharacters"];

            Trace.TraceInformation("LUISAppID: " + LUISAppId);
            Trace.TraceInformation("LUISAppKey: " + LUISAppKey);
            Trace.TraceInformation("LUISMaxCharacters: " + LUISMaxCharacters);



            ViewBag.LUISAppId = LUISAppId;
            ViewBag.BlobList = getTrainingBlobsFromStorageAccount();

            //Recoger los valores de las variables y crear la clase
            LUIS _luis = new LUIS(LUISAppId, LUISAppKey, int.Parse(LUISMaxCharacters));


            return View();
        }



        [HttpPost]
        public string ProcessIndex()
        {
            Thread.Sleep(5000);

            return "Guardia quiere comer ya!";
        }




        [HttpPost]
        public string trainLUIS() {
            List<LUISResponse> result = new List<LUISResponse>();

            string LUISAppId = ConfigurationManager.AppSettings["LUISAppId"];
            string LUISAppKey = ConfigurationManager.AppSettings["LUISAppKey"];
            string LUISMaxCharacters = ConfigurationManager.AppSettings["LUISMaxCharacters"];

            Trace.TraceInformation("LUISAppID: " + LUISAppId);
            Trace.TraceInformation("LUISAppKey: " + LUISAppKey);
            Trace.TraceInformation("LUISMaxCharacters: " + LUISMaxCharacters);


            //Recoger los valores de las variables y crear la clase
            LUIS _luis = new LUIS(LUISAppId, LUISAppKey, int.Parse(LUISMaxCharacters));

            //Connect to Media Assets storage account
            string MediaAssetsStorageAccountName = ConfigurationManager.AppSettings["MediaAssetsStorageAccountName"];
            string MediaAssetsStorageAccountKey = ConfigurationManager.AppSettings["MediaAssetsStorageAccountKey"];
            string containerName = ConfigurationManager.AppSettings["TrainingTextsStorageContainer"];

            string storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + MediaAssetsStorageAccountName + ";AccountKey=" + MediaAssetsStorageAccountKey;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);


            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            string text = "";
            foreach (Uri blob in getTrainingBlobsFromStorageAccount()) {
                text = readBlobFileAsText(container, blob);
                result.Add(_luis.makeLUISCallFromText(text));
            }

            ViewData["LUISResponses"] = result;

            //return RedirectToAction("Index");
            return "Controller says: Entrenado";
        }




        [HttpPost]
        public ActionResult uploadTrainingTextFilesToStorageAccount(IEnumerable<HttpPostedFileBase> files) {
            string result = "";

            string containerName = ConfigurationManager.AppSettings["TrainingTextsStorageContainer"];
            //if (containerName.Equals("")) return "[ERROR] uploadTrainingTextFilesToStorageAccount - application setting (TrainingTextsStorageContainer) is not set";
            Console.WriteLine("Hola desde uploadTrainingTextFilesToStorageAccount!");

            UploadFileToBlobStorage(containerName, files);


            return RedirectToAction("Index");
        }




        public void UploadFileToBlobStorage(string containerName, IEnumerable<HttpPostedFileBase> files) {

            //Connect to Media Assets storage account
            string MediaAssetsStorageAccountName = ConfigurationManager.AppSettings["MediaAssetsStorageAccountName"];  
            string MediaAssetsStorageAccountKey = ConfigurationManager.AppSettings["MediaAssetsStorageAccountKey"];
            string storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + MediaAssetsStorageAccountName + ";AccountKey=" + MediaAssetsStorageAccountKey;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);

            
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);


            foreach (var file in files) {
                string blobName = file.FileName.Split('\\').Last();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
                blockBlob.UploadFromStream(file.InputStream);
            }
                
        }






        public List<Uri> getTrainingBlobsFromStorageAccount() {
            List<Uri> result = new List<Uri>();

            string storageAccountName = "storagecallcenterdemo";
            string storageAccountKey = "5r0hNiuYnAx8Dn3GLAm3mpxVNNPMUfOobecQ+teghHKh5ryJN6Xyc2itasjaGSrR2LjIDyedArFC2GngpG3IzQ==";

            string storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + storageAccountName + ";AccountKey=" + storageAccountKey;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            IEnumerable<CloudBlobContainer> containerList = blobClient.ListContainers();


            //Listado de todos los contenedores
            foreach (CloudBlobContainer container in containerList) {

                //Listado de todos los blobs dentro del contenedor actual
                IEnumerable<IListBlobItem> blobList = container.ListBlobs();

                foreach (IListBlobItem blob in blobList) {

                    string filename = blob.Uri.Segments.Last();
                    string[] filenameArray = filename.Split('.');

                    result.Add(blob.Uri);

                }

            }


            return result;
        }




        private string readBlobFileAsText(CloudBlobContainer container, Uri blob) {
            try {
                string result = "";
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob.Segments.Last());
                using (var memoryStream = new MemoryStream()) {
                    blockBlob.DownloadToStream(memoryStream);
                    result = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                }
                return result;
            } catch (Exception ex) { Trace.TraceError("readBlobFileAsText exception: " + ex.Message); return ""; }
        }





    }
}