using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using System.Threading;

namespace Learning
{
    class Program
    {
        const string personGroupId = "family3";

        static void Main(string[] args)
        {
            var d = DateTime.Now;
            var r = DoIt();
            r.Wait();
            Console.WriteLine("Time spent {0} sec.", (DateTime.Now - d).TotalSeconds);
            Console.ReadKey();
        }

        private static async Task DoIt()
        {
            Console.Write("Connect to server");

            IFaceClient faceClient = new FaceClient(
           new ApiKeyServiceClientCredentials("1a0dbec9939347118f891fa5ab6fcdf2"),  // 1a0dbec9939347118f891fa5ab6fcdf2   392f60c535434c0dba36b2c1b7753e8b
           new System.Net.Http.DelegatingHandler[] { })
            { Endpoint = "https://westcentralus.api.cognitive.microsoft.com/" };

            Console.WriteLine("..OK");

            try
            {
                Console.WriteLine("PersonGroup Create");
                await faceClient.PersonGroup.CreateAsync(personGroupId, "Fam 3");
            }
            catch(Exception )
            {

            }

            Console.WriteLine("Clean group");
            var ll = await faceClient.PersonGroupPerson.ListAsync(personGroupId);
            foreach(var l in ll)
            {
                Console.Write(l.Name);
                await faceClient.PersonGroupPerson.DeleteAsync(personGroupId, l.PersonId);
                Console.WriteLine("-");                
            }

            Console.WriteLine("Begin Learning");

            var directories = Directory.GetDirectories("People");

            foreach(var dir in directories)
            {
                var di = new DirectoryInfo(dir);
                Console.Write(di.Name);

                var friend1 = await faceClient.PersonGroupPerson.CreateAsync(
                // Id of the PersonGroup that the person belonged to
                personGroupId,
                // Name of the person
                di.Name
            );

                foreach (string imagePath in Directory.GetFiles(dir, "*.jpg"))
                {
                    using (Stream s = File.OpenRead(imagePath))
                    {
                        // Detect faces in the image and add to Anna
                        await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                            personGroupId, friend1.PersonId, s);
                    }
                    Console.Write(".");
                }

                await faceClient.PersonGroup.TrainAsync(personGroupId);
                Console.WriteLine("*");
                Thread.Sleep(61 * 1000);
            }


            Console.WriteLine("Done Learning");
          
        }
    }
}
