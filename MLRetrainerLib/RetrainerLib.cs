﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MLRetrainerLib
{
    public class RetrainerLib
    {
        public string _mlretrainmodelurl { get; set; }
        public string _mlretrainerkey { get; set; }
        public string _endpointurl { get; set; }
        public string _endpointkey { get; set; }
        public string _mlstoragename { get; set; }
        public string _mlstoragekey { get; set; }
        public string _mlstoragecontainer { get; set; }

        private string _storgaeConnectionString;

        private CloudBlockBlob _trainingBlob;

        public Dictionary<string, double> lastScores { get; set; }
        public Dictionary<string, double> retrainedScores { get; set; }

        public RetrainerLib(params string[] configs)
        {
            _mlretrainmodelurl   = configs[0];
            _mlretrainerkey      = configs[1];
            _endpointurl         = configs[2];
            _endpointkey         = configs[3];
            _mlstoragename       = configs[4];
            _mlstoragekey        = configs[5];
            _mlstoragecontainer  = configs[6];

            _storgaeConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", _mlstoragename, _mlstoragekey);

            lastScores = GetRetrainedResults(); //Set this for retraining so we can compare updated model score
        }

        /// <summary>
        /// Used to setup the job retraining. You nust receive the jobID and submit to the start method
        /// </summary>
        /// <returns>This is a jobID used to then start the retraining job</returns>
        public async Task<string> UploadRetrainingAsync()
        {
            //TODO: need to set this up first
            //if (_trainingBlob == null) { return null; }

            string jobId = null;

            //CloudBlockBlob blob = _trainingBlob;

            BatchExecutionRequest request = new BatchExecutionRequest()
            {

                //Input = new AzureBlobDataReference()
                //{
                //    //ConnectionString = _storgaeConnectionString,
                //    //RelativeLocation = blob.Uri.LocalPath
                //},

                Outputs = new Dictionary<string, AzureBlobDataReference>()
                    {
                        {
                            "output2",
                            new AzureBlobDataReference()
                            {
                                ConnectionString = _storgaeConnectionString,
                                RelativeLocation = string.Format("/{0}/output2results.ilearner", _mlstoragecontainer)
                            }
                        },
                        {
                            "output1",
                            new AzureBlobDataReference()
                            {
                                ConnectionString = _storgaeConnectionString,
                                //RelativeLocation = string.Format("/{0}/output1results.csv", StorageContainerName)
                                RelativeLocation = string.Format("/{0}/output1results.csv", _mlstoragecontainer)
                            }
                        },
                    },
                GlobalParameters = new Dictionary<string, string>()
                {
                }
            };

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _mlretrainerkey);

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)

                // submit the job
                string uploadJobURL = _mlretrainmodelurl + "?api-version=2.0";
                HttpResponseMessage response = await client.PostAsJsonAsync(uploadJobURL, request);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                jobId = await response.Content.ReadAsAsync<string>(); //Used to reference the job for start and monitoring of completion
                //Console.WriteLine(string.Format("Job ID: {0}", jobId));
            }

            return jobId;
        }

        public async Task<BatchScoreStatusCode> CheckJobStatus(string jobId)
        {
            BatchScoreStatus status;

            using (HttpClient client = new HttpClient())
            {
                // Check the job
                string jobLocation = _mlretrainmodelurl + "/" + jobId + "?api-version=2.0";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _mlretrainerkey);

                HttpResponseMessage response = await client.GetAsync(jobLocation);
                if (!response.IsSuccessStatusCode)
                {
                    return BatchScoreStatusCode.Cancelled;
                }

                status = await response.Content.ReadAsAsync<BatchScoreStatus>();
            }

            return status.StatusCode;
        }

        //used to begin the retraining. This will need a callback in order to know when we're done and to get the precision results
        public async Task StartRetrainingJob(string jobId)
        {
            using (HttpClient client = new HttpClient())
            {
                // start the job
                string jobStartURL = _mlretrainmodelurl + "/" + jobId + "/start?api-version=2.0";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _mlretrainerkey);
                HttpResponseMessage response = await client.PostAsync(jobStartURL, null);
                if (!response.IsSuccessStatusCode)
                {
                    return;
                }
            }

        }

        /// <summary>
        /// This method puts in place the retrained iLearner for the published endpoint
        /// </summary>
        /// <param name="baseLoc"></param>
        /// <param name="relLoc"></param>
        /// <param name="sasBlobtoken"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public async Task<bool> UpdateRetrainedModel(string baseLoc, string relLoc, string sasBlobtoken, string connStr)
        {
            //string apiKey = "DUQquqfOk7Sk21g3K/YigqSdwM7Z4xbs2EYrEXDHNUjiZHLRtKUK72RgCfYIwiLYQJWSB5y7Lp0apfu0tIJnnQ=="; //Trained Model API
            //string apiKey = "Y5kjC3KiFTSn6eg08eCX4SpbgZd6X6Fv2zP5Oa0kIeAH4tKAMKLRBUvlcIqy+05I3DlL0vs2CqUK3NJwIfkn8A=="; //Endpoint API
            //string endPointURL = "https://management.azureml.net/workspaces/1e9859bf8e4d4861abf92463f2b0554a/webservices/779da01f4b3c4029bba5b5e2c7b9ed78/endpoints/rtep";

            var resourceLocations = new ResourceLocations()
            {
                Resources = new ResourceLocation[] {
                    new ResourceLocation()
                    {
                        Name = "Scenario 1 When will a customer return [trained model]",
                        Location = new AzureBlobDataReference()
                        {
                            BaseLocation = baseLoc,
                            RelativeLocation = relLoc,
                            SasBlobToken = sasBlobtoken
                        }
                    }
                }
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _endpointkey);
                using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), _endpointurl))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(resourceLocations), System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode) { return true;  }
                    else { return false; }
                }
            }
        }

        /// <summary>
        /// Used to update the 
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public async Task<Boolean> UpdateModel(string jobId)
        {
            BatchScoreStatus status;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Check the job
                    string jobLocation = _mlretrainmodelurl + "/" + jobId + "?api-version=2.0";
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _mlretrainerkey);

                    HttpResponseMessage response = await client.GetAsync(jobLocation);
                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    status = await response.Content.ReadAsAsync<BatchScoreStatus>();
                }

                AzureBlobDataReference res = status.Results["output2"];
                return await UpdateRetrainedModel(res.BaseLocation, res.RelativeLocation, res.SasBlobToken, _storgaeConnectionString);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// This method is primarily used to upload a new training set before retraining. 
        /// If you are retraining where the model trainer is fed from a cloud based storage location (SQL or Azure Storage for example) then
        /// you won't need this call
        /// </summary>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public bool GetRetrainingBlob(string blobName)
        {
            string rtBlobName = blobName;

            if (rtBlobName.EndsWith(".csv") || rtBlobName.EndsWith(".nh.csv") || rtBlobName.EndsWith(".tsv") || rtBlobName.EndsWith(".nh.tsv")) { }
            else
            {
                throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "File {0} is not a supported extension type (like csv or tsv)", rtBlobName));
            }

            try
            {
                CloudBlobClient blobClient = CloudStorageAccount.Parse(_storgaeConnectionString).CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_mlstoragecontainer);
                _trainingBlob = container.GetBlockBlobReference(rtBlobName);

                if (_trainingBlob.Exists())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "Blob {0} doesn't exist on local system.", rtBlobName));
            }
        }


        /// <summary>
        /// This method retrives the results of the last model retraining. If there are no existing results (it has never run) then it will return null
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, double> GetRetrainedResults()
        {
            string conn = _storgaeConnectionString;
            Dictionary<string, Double> vals = null;
            try
            {
                var blobClient = CloudStorageAccount.Parse(conn).CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(_mlstoragecontainer);
                //container.CreateIfNotExists();
                var blob = container.GetBlockBlobReference("output1results.csv");
                if (!blob.Exists()) { return null;  }
                //blob.DownloadToFile(@"c:\drops\results.csv", FileMode.OpenOrCreate);

                MemoryStream mStream = new MemoryStream();
                blob.DownloadToStream(mStream);

                string decoded = Encoding.UTF8.GetString(mStream.ToArray());

                vals = ExtractModelValues(decoded);
                retrainedScores = vals;

                return vals;
            }
            catch
            {
                return null;
            }
        }

        public Dictionary<string, Double> ExtractModelValues(string mvalues)
        {
            Dictionary<string, Double> vals = new Dictionary<string, double>();

            string[] entries = mvalues.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string[] headers = entries[0].Split(',');
            string[] values = entries[1].Split(',');

            for (int loop = 0; loop < headers.Length; loop++)
            {
                vals.Add(headers[loop], Convert.ToDouble(values[loop]));
            }

            return vals;
        }

        /// <summary>
        /// This method will tell you if you should deploy the retrained model. it compares the last known scores with the newest scores
        /// from retraining. Note that when this class is instantiated, it will attempt to retrieve last results from the configured blob. 
        /// Thus if there are no existing scores in the blob, it will return TRUE since it assumes no deployed endpoint yet.
        /// </summary>
        /// <param name="measuredValue"></param>
        /// <param name="pctImproveMin">This should be in it's decimal form such as 0.02f for 2% improvement target as an exmaple</param>
        /// <returns></returns>
        public bool isUdpateModel(string measuredValue, float pctImproveMin)
        {
            if (String.IsNullOrEmpty(measuredValue) || pctImproveMin == 0 || retrainedScores == null)
            {
                return false;
            }
            if (lastScores == null) { return true;
            }
            bool isImproved = false;

            var lastCompareScore = lastScores[measuredValue];
            var newCompareScore = retrainedScores[measuredValue];

            var improvement = lastCompareScore + (lastCompareScore * pctImproveMin);
            if (newCompareScore >= improvement) { isImproved = true; }

            return isImproved;
        }
    }

    public class AzureBlobDataReference
    {
        // Storage connection string used for regular blobs. It has the following format:
        // DefaultEndpointsProtocol=https;AccountName=ACCOUNT_NAME;AccountKey=ACCOUNT_KEY
        // It's not used for shared access signature blobs.
        public string ConnectionString { get; set; }

        // Relative uri for the blob, used for regular blobs as well as shared access 
        // signature blobs.
        public string RelativeLocation { get; set; }

        // Base url, only used for shared access signature blobs.
        public string BaseLocation { get; set; }

        // Shared access signature, only used for shared access signature blobs.
        public string SasBlobToken { get; set; }
    }

    public class ResourceLocations
    {
        public ResourceLocation[] Resources { get; set; }
    }

    public class ResourceLocation
    {
        public string Name { get; set; }
        public AzureBlobDataReference Location { get; set; }
    }

    public class BatchExecutionRequest
    {
        public AzureBlobDataReference Input { get; set; }
        public IDictionary<string, string> GlobalParameters { get; set; }

        // Locations for the potential multiple batch scoring outputs
        public IDictionary<string, AzureBlobDataReference> Outputs { get; set; }
    }

    public enum BatchScoreStatusCode
    {
        NotStarted,
        Running,
        Failed,
        Cancelled,
        Finished
    }

    public class BatchScoreStatus
    {
        // Status code for the batch scoring job
        public BatchScoreStatusCode StatusCode { get; set; }


        // Locations for the potential multiple batch scoring outputs
        public IDictionary<string, AzureBlobDataReference> Results { get; set; }

        // Error details, if any
        public string Details { get; set; }
    }
}
