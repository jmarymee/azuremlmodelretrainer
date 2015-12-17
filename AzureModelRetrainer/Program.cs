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
            List<string> paramList = new List<string>();
            paramList.Add(Properties.Settings.Default.mlretrainermodelurl);
            paramList.Add(Properties.Settings.Default.mlretrainerkey);
            paramList.Add(Properties.Settings.Default.enpointurl);
            paramList.Add(Properties.Settings.Default.endpointkey);
            paramList.Add(Properties.Settings.Default.mlstoragename);
            paramList.Add(Properties.Settings.Default.mlstoragekey);
            paramList.Add(Properties.Settings.Default.mlstoragecontainer);
            paramList.Add(Properties.Settings.Default.endpointname);


            MLRetrainerLib.RetrainerLib retrainer = new MLRetrainerLib.RetrainerLib(paramList.ToArray());


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
            Dictionary<string, double> scores = retrainer.GetRetrainedResults();
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
