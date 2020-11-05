using EPiFaceRecognition.Consts;
using EPiServer.Framework.Blobs;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace EPiFaceRecognition.Services
{
    internal class FaceRecognitionService
    {
        private static string SubscriptionKey => ConfigurationManager.AppSettings[SettingKeyConsts.AzureFaceAPISubscriptionKey];
        private static string Endpoint => ConfigurationManager.AppSettings[SettingKeyConsts.AzureFaceAPIEndpoint];

        private static IFaceClient _faceClient;
        private static IFaceClient FaceClientObj
        {
            get
            {
                if (_faceClient == null)
                    _faceClient = new FaceClient(new ApiKeyServiceClientCredentials(SubscriptionKey)) { Endpoint = Endpoint };
                return _faceClient;
            }
        }

        internal static IList<DetectedFace> DetectFace(Blob imageBlob)
        {
            var task = Task.Run(async () => await DetectFaces(imageBlob.OpenRead()));
            return task.Result;
        }

        private static async Task<IList<DetectedFace>> DetectFaces(Stream image)
        {
            return await FaceClientObj.Face.DetectWithStreamAsync(image,
                recognitionModel: RecognitionModel.Recognition02,
                returnFaceLandmarks: false);
        }
    }
}
