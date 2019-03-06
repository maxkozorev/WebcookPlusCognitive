using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Recognition_face
{
    class MyFace
    {
        const string friend1ImageDir = @"C:\Users\mako\Desktop\Fotoware\PartnerSummit\MainAzureFuntion\MainSolution\Main\TrainImages\Max";
        const string friend1ImageDir1 = @"C:\Users\mako\Desktop\Fotoware\PartnerSummit\MainAzureFuntion\MainSolution\Main\TrainImages\Anne";
        const string friend1ImageDir2 = @"C:\Users\mako\Desktop\Fotoware\PartnerSummit\MainAzureFuntion\MainSolution\Main\TrainImages\Florian";

        public async Task<IList<PersonGroup>> GetAllPersonGroupsAsync(FaceServiceClient faceServiceClient)
        {
            return await faceServiceClient.GetPersonGroupsAsync();
        }
        //Initial creation
        public async Task<bool> Create_Person(FaceServiceClient faceServiceClient, string personGroupId, TraceWriter log, string person_group_name)
        {
            await faceServiceClient.CreatePersonGroupAsync(personGroupId, person_group_name);

            // Define MAX
            CreatePersonResult friend1 = await faceServiceClient.CreatePersonAsync(
                // Id of the PersonGroup that the person belonged to
                personGroupId,
                // Name of the person
                "Max"
            );
            foreach (string imagePath in Directory.GetFiles(friend1ImageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    // Detect faces in the image and add to Anna
                    await faceServiceClient.AddPersonFaceAsync(
                        personGroupId, friend1.PersonId, s);
                }
            }

            // Define Anne
            CreatePersonResult friend2 = await faceServiceClient.CreatePersonAsync(
                // Id of the PersonGroup that the person belonged to
                personGroupId,
                // Name of the person
                "Anne"
            );
            foreach (string imagePath in Directory.GetFiles(friend1ImageDir1, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    // Detect faces in the image and add to Anna
                    await faceServiceClient.AddPersonFaceAsync(
                        personGroupId, friend2.PersonId, s);
                }
            }

            // Define Florian
            CreatePersonResult friend3 = await faceServiceClient.CreatePersonAsync(
                // Id of the PersonGroup that the person belonged to
                personGroupId,
                // Name of the person
                "Florian"
            );
            foreach (string imagePath in Directory.GetFiles(friend1ImageDir2, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    // Detect faces in the image and add to Anna
                    await faceServiceClient.AddPersonFaceAsync(
                        personGroupId, friend3.PersonId, s);
                }
            }


            // Train
            await faceServiceClient.TrainPersonGroupAsync(personGroupId);

            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                if (trainingStatus.Status != Status.Running)
                {
                    return true;
                }

                await Task.Delay(1000);
            }
        }

        //Predict face
        public async Task<string> Predict_face(byte [] image, FaceServiceClient faceServiceClient, string personGroupId)
        {
            string error = "";
            try
            {
                using (Stream s = new MemoryStream(image))
                {
                    var faces = await faceServiceClient.DetectAsync(s);
                    var faceIds = faces.Select(q => q.FaceId).ToArray();

                    var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);

                    foreach (var identifyResult in results)
                    {
                        if (identifyResult.Candidates.Length != 0)
                        {
                            // Get top 1 among all candidates returned
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                            if (person.Name != null)
                                return person.Name.ToString();     
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return error;
            }
            return error;
        }
    }
}
