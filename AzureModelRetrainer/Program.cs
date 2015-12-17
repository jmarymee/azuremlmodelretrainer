using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AzureModelRetrainer
{
    class Program
    {
        static void Main(string[] args)
        {
            MLRetrainerLib.RetrainerLib.MLRetrainConfig configobj = new MLRetrainerLib.RetrainerLib.MLRetrainConfig();
            configobj.mlretrainerurl = Properties.Settings.Default.mlretrainermodelurl;
            configobj.mlretrainerkey = Properties.Settings.Default.mlretrainerkey;
            configobj.publishendpointurl = Properties.Settings.Default.enpointurl;
            configobj.publishendpointkey = Properties.Settings.Default.endpointkey;
            configobj.mlretrainerstoragename = Properties.Settings.Default.mlstoragename;
            configobj.mlretrainerstoragekey = Properties.Settings.Default.mlstoragekey;
            configobj.mlretrainercontainername = Properties.Settings.Default.mlstoragecontainer;
            configobj.publishendpointname = Properties.Settings.Default.endpointname;

            MLRetrainerLib.RetrainerLib retrainer = new MLRetrainerLib.RetrainerLib(configobj);

            var resList = retrainer.GetAllStoredResults();


            retrainer.UpdateSQLQueryForNewDate("2015-10-01");

            Console.WriteLine("Results of last training: ");
            foreach(var val in retrainer.lastScores)
            {
                Console.WriteLine("Rating Name: {0} : Value: {1}", val.Key, val.Value.ToString());
            }

            string jobID = retrainer.QueueRetrainingAsync().Result;

            retrainer.StartRetrainingJob(jobID).Wait();

            MLRetrainerLib.BatchScoreStatusCode status;

            do
            {
                status = retrainer.CheckJobStatus(jobID).Result;
                Console.WriteLine(status.ToString());
            } while (!(status == MLRetrainerLib.BatchScoreStatusCode.Finished));

            Console.WriteLine("New Scores for retraining...");
            Dictionary<string, double> scores = retrainer.GetLatestRetrainedResults();
            foreach (var val in scores)
            {
                Console.WriteLine("Rating Name: {0} : Value: {1}", val.Key, val.Value.ToString());
            }

            if (!retrainer.isUdpateModel("AUC", 0.02f))
            {
                Console.WriteLine("No need to update endpoint. Accuracy has not improved. Press a key to end");
                Console.ReadLine();
                //return;
            }

            bool isUpdated = retrainer.UpdateModel(jobID).Result;
            if (isUpdated)
            {
                Console.WriteLine("Successful model retraining and endpoint deployment");
            }
            else
            {
                Console.WriteLine("Something went wrong updating the model");
            }

            Console.WriteLine("Process has completed. Press a key to end");
            Console.ReadLine();
        }
    }
}
