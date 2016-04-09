using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MediaSearch.Controllers
{
    public class LuisController : Controller
    {
        // GET: Luis
        public ActionResult Index()
        {
            ViewBag.LUISAppId = ConfigurationManager.AppSettings["LUISAppId"];
            ViewBag.BlobList = getTrainingBlobsFromStorageAccount();
            return View();
        }

        [HttpPost]
        public string ProcessIndex()
        {
            Thread.Sleep(5000);

            return "Guardia quiere comer ya!";
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


    }
}