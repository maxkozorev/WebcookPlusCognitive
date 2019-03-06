using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Main.Recognition_face;
using Main.Recognition_image;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Main.Models.MetadataExampleModel;

namespace Main
{
    public static class Funtion1
    {
        //Some variables
        private const string subscriptionKey = "0549802ea2b44503aa2830f462754ce7";
        private const string face_endpoint = "https://centralus.api.cognitive.microsoft.com/face/v1.0/";
        const string api_key = "YotvkXJ8PSZ0OQbwmlgPHLnUwTrNDIWF";
        const string content_type = "application/vnd.fotoware.assetupdate+json";
        const string accept = "application/vnd.fotoware.asset+json";
        const string person_group_name = "supername123";
        const string personGroupId = "superid123";

        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            Image Image = new Image();
            MyFace face = new MyFace();

            #region Image Analysis
            ComputerVisionClient computerVision = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(subscriptionKey),
                new System.Net.Http.DelegatingHandler[] { });
            computerVision.Endpoint = "https://centralus.api.cognitive.microsoft.com/";

            //Read Request
            object data = await req.Content.ReadAsAsync<object>();
            string string_data = data.ToString();
            JObject jObject = (JObject)JsonConvert.DeserializeObject(string_data);

            // Image Url's
            string url_image = jObject["preview-href"].Value<string>();
            string picture_url = jObject["href"].Value<string>();

            // Downloading Image from webhook url
            var webClient = new WebClient();
            byte[] image = webClient.DownloadData(url_image);

            // Analyze image 
            var ImageAnalysis = await Image.AnalyzeLocalAsync(computerVision, image);
            var Image_Description = ImageAnalysis.Description.Captions.Select(q => q.Text).FirstOrDefault();

            // List of Tags
            List<string> list_tags = new List<string>();
            foreach (var a in ImageAnalysis.Tags)
                list_tags.Add(a.Name.ToString());
            #endregion
            #region Face
            var faceServiceClient = new FaceServiceClient(subscriptionKey, face_endpoint);
            //Check for existing groups
            var all_existing_groups = await face.GetAllPersonGroupsAsync(faceServiceClient);
            var group_exists = all_existing_groups.Where(q => q.PersonGroupId == personGroupId).Select(w => w.Name == person_group_name).FirstOrDefault();
            bool success;
            string face_string = "";
            if (!group_exists)
            {
                success = await face.Create_Person(faceServiceClient, personGroupId, log, person_group_name);
                if(success)
                    face_string = await face.Predict_face(image, faceServiceClient, personGroupId);
            }
            else
                face_string = await face.Predict_face(image, faceServiceClient, personGroupId);
            #endregion
            // Populate Metadata 
            var send_metadata = new Send_Metadata()
            {
                metadata = new Metadata()
                {
                    description = new Description()
                    {
                        description = Image_Description
                    },
                    facename = new Models.MetadataExampleModel.Face()
                    {
                        value = face_string
                    },
                    tagsname = new Tags()
                    {
                        value = list_tags
                    }
                }
            };
            //Prepare json
            var json_serialized = JsonConvert.SerializeObject(send_metadata);
            var URI = new Uri(picture_url);

            //Send metadata request
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("FWAPIToken", api_key);
                wc.Headers.Add("Content-Type", content_type);
                wc.Headers.Add("Accept", accept);
                await wc.UploadStringTaskAsync(URI, "PATCH", json_serialized);
            }
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
