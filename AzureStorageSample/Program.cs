using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace AzureStorageSample
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        private class AzureConfiguration
        {
            public static string StorageAcccountName = "";
            public static string StorageAccountKey = "";
            public static string StorageContainerName = "";
            public static string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + StorageAcccountName + ";AccountKey=" + StorageAccountKey;
            
            public static CloudStorageAccount Account = CloudStorageAccount.Parse(StorageConnectionString);
        }

        #region " Azure Blob Storage "
        static void getBlobs(ref CloudStorageAccount account, string containerName)
        {
            var blobClient = account.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);

            foreach (var item in container.ListBlobs(null, false))
            {
                if (item.GetType() == typeof(Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob))
                {
                    var blob = (Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob)item;

                    using (MemoryStream stream = new MemoryStream())
                    {
                        blob.DownloadToStream(stream);

                        Console.WriteLine("Blob name: {1}\tContent: {0}", System.Text.Encoding.UTF8.GetString(stream.ToArray()), blob.Name);
                    }
                }
            }
        }
        static string getBlob(ref CloudStorageAccount account, string containerName, string blobName)
        {
            var blobResults = string.Empty;

            try
            {
                var blobClient = account.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(containerName);
                var blockBlob = container.GetBlockBlobReference(blobName);

                using (MemoryStream stream = new MemoryStream())
                {
                    blockBlob.DownloadToStream(stream);

                    using (StreamReader sr = new StreamReader(stream, System.Text.Encoding.GetEncoding(1252)))
                    {
                        blobResults = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ignore)
            {
                // return empty string - zero length string denotes download failed or invalid Block Blob
            }

            return blobResults;
        }

        static void storeBlobs(ref CloudStorageAccount acct, string containerName)
        {
            var files = Directory.GetFiles(@"C:\Projects\azureFiles\");

            foreach (var file in files)
            {
                var blobName = file.Substring(file.LastIndexOf("\\") + 1, file.Length - file.LastIndexOf("\\") - 5);

                var fileData = new MemoryStream(File.ReadAllBytes(file));

                Console.WriteLine(blobName + " | " + storeBlob(ref acct, containerName, blobName, fileData).ToString());
            }
        }

        static bool storeBlob(ref CloudStorageAccount account, string containerName, string blobName, MemoryStream stream)
        {
            try
            {
                var blobClient = account.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(containerName);
                var blockBlob = container.GetBlockBlobReference(blobName);
                blockBlob.UploadFromStream(stream);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}
