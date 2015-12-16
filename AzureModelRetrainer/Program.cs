using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureModelRetrainer
{
    class Program
    {
        static void Main(string[] args)
        {
            //_mlretrainmodelurl = configs[0];
            //_mlretrainerkey = configs[1];
            //_endpointurl = configs[2];
            //_endpointkey = configs[3];
            //_mlstoragename = configs[4];
            //_mlstoragekey = configs[5];
            //_mlstoragecontainer = configs[6];

            List<string> paramList = new List<string>();
            paramList.Add(Properties.Settings.Default.mlretrainermodelurl);
            paramList.Add(Properties.Settings.Default.mlretrainerkey);
            paramList.Add(Properties.Settings.Default.enpointurl);
            paramList.Add(Properties.Settings.Default.endpointkey);
            paramList.Add(Properties.Settings.Default.mlstoragename);
            paramList.Add(Properties.Settings.Default.mlstoragekey);
            paramList.Add(Properties.Settings.Default.mlstoragecontainer);

            MLRetrainerLib.RetrainerLib retrainer = new MLRetrainerLib.RetrainerLib(paramList.ToArray());

            Console.WriteLine("Results of last training: ");
            foreach(var val in retrainer.lastScores)
            {
                Console.WriteLine("Rating Name: {0} : Value: {1}", val.Key, val.Value.ToString());
            }

            string jobID = retrainer.UploadRetrainingAsync().Result;

            retrainer.StartRetrainingJob(jobID).Wait();

            MLRetrainerLib.BatchScoreStatusCode status;

            do
            {
                status = retrainer.CheckJobStatus(jobID).Result;
                Console.WriteLine(status.ToString());
            } while (!(status == MLRetrainerLib.BatchScoreStatusCode.Finished));

            Dictionary<string, double> scores = retrainer.GetRetrainedResults();

            if (!retrainer.isUdpateModel("AUC", 0.02f)) { return; }

            bool isUpdated = retrainer.UpdateModel(jobID).Result;
            if (isUpdated)
            {
                Console.WriteLine("Successful model retraining and endpoint deployment");
            }
            else
            {
                Console.WriteLine("Something went wrong updating the model");
            }
        }
    }
}
