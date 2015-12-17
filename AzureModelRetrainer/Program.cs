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

            //You can change the date run of the SQL query in order to get a daily trained model
            //retrainer.UpdateSQLQueryForNewDate("2015-10-01");

            //This is for display. it allows a person to view the results of the last model training.
            //This List is stored initially in the Library upon object instatiation
            Console.WriteLine("Results of last training: ");
            foreach(var val in retrainer.lastScores)
            {
                Console.WriteLine("Rating Name: {0} : Value: {1}", val.Key, val.Value.ToString());
            }

            //Retraining a model takes two steps; queue up the job then start the job. One must save the jobID in order to start the job
            string jobID = retrainer.QueueRetrainingAsync().Result;

            //This is how you start the retraining job
            retrainer.StartRetrainingJob(jobID).Wait();

            //We use this to watch the retraining so that we can decide if we want to deploy to the endpoint or not. 
            //We spin here on the token until we show complete
            MLRetrainerLib.BatchScoreStatusCode status;

            //Here is when we spin lock until we show that the retraining job is Finished
            do
            {
                status = retrainer.CheckJobStatus(jobID).Result;
                Console.WriteLine(status.ToString());
            } while (!(status == MLRetrainerLib.BatchScoreStatusCode.Finished));

            //Now we look at the new (latest) results. 
            Console.WriteLine("New Scores for retraining...");
            Dictionary<string, double> scores = retrainer.GetLatestRetrainedResults();
            foreach (var val in scores)
            {
                Console.WriteLine("Rating Name: {0} : Value: {1}", val.Key, val.Value.ToString());
            }

            //Here is where we compare the current result to the last result. In this scenario, we compare AUC and if we haven't at 
            //least seen a 20% improvement then we don't deploy the retrained model
            if (!retrainer.isUdpateModel("AUC", 0.02f))
            {
                Console.WriteLine("No need to update endpoint. Accuracy has not improved. Press a key to end");
                Console.ReadLine();
                //return;
            }

            //Here is where we deploy the model to the published endpoint IF the accuracy has met our hurdle
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
