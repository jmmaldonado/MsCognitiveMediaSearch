using System;
using Microsoft.Azure.WebJobs;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;
using MediaSearch.Model;
using Microsoft.WindowsAzure.MediaServices.Client;
using IndexWebJob;

namespace IndexWebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var host = new JobHost();
            // The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            ProcessFiles().Wait();
        }

        private async static Task ProcessFiles()
        {
            //0. Constants
            const string AccountName = "jmmmediaservices";
            const string AccountKey = "KOs3A87L2UydtogQDjM1/FF0yuTaqsqBViFJG+WoKj8=";

            //1. Install Nuget packages
            //1.1 Nuget: Install-Package windowsazure.mediaservices

            ////2. Get AMS context
            //var context = new CloudMediaContext(AccountName, AccountKey);


            //foreach (var asset in context.Assets.ToList())
            //{
            //    Console.WriteLine(asset.Name);
            //}

            //Console.ReadLine();

            //1. From TTML to JSON
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=jmmmediaservicesstorage;AccountKey=VpmaM9avmqz7zYr/CCtMUndgZserSIE26sYRS/IqInzlPC03KW9BR+DHIOGV/AOPYxZMnTMSOtf1jOojs8OS/Q==");
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //clean collection
            DocumentDBRepository<AudioFile>.DeleteCollection();

           var containers = blobClient.ListContainers();

            foreach (var container in containers)
            {
                var blobs = blobClient.GetContainerReference(container.Name).ListBlobs().OfType<CloudBlob>().Where(b => b.Name.EndsWith(".ttml")).OrderByDescending(b => b.Properties.Length);

                foreach (var blob in blobs)
                {
                    var ttml = blobClient.GetBlobReferenceFromServer(blob.Uri);

                    using (var stream = new MemoryStream())
                    {
                        ttml.DownloadToStream(stream);
                        stream.Position = 0;
                        var json = Helper.ParseAudioTranscript(stream);

                        var audioFile = new AudioFile
                        {
                            Title = blob.Uri.Segments.Last().Replace(".ttml", ""),
                            Url = blob.Uri.ToString().Replace(".ttml", ""),                           
                            AudioTranscripts = json
                        };

                        var stringJson = Helper.JsonSerializer(json);

                        Console.WriteLine(stringJson);

                        Console.WriteLine();
                        Console.WriteLine("AudioFile object");
                        Console.WriteLine(Helper.JsonSerializer(audioFile));

                        //2. From JSON to DocumentDB
                        await DocumentDBRepository<AudioFile>.CreateItemAsync(audioFile);

                        var jsonContainer = blobClient.GetContainerReference(ttml.Container.Name);

                        var jsonFile = jsonContainer.GetBlockBlobReference(ttml.Name.Replace(".ttml", ".json"));

                        jsonFile.UploadText(stringJson);
                    }

                }
            }
            Console.WriteLine();
            Console.WriteLine("Done!");

            Console.ReadLine();
        }
    }
}
