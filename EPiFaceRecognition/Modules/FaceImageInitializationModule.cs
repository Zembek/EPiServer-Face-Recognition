using EPiFaceRecognition.Attributes;
using EPiFaceRecognition.Contracts;
using EPiFaceRecognition.Models;
using EPiFaceRecognition.Services;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EPiFaceRecognition.Modules
{

    [InitializableModule]
    public class FaceImageInitializationModule : IInitializableModule
    {
        private Injected<IContentEvents> _contentEventsInj;
        private Injected<IContentLoader> _contentLoaderInj;
        private Injected<IContentRepository> _contentRepositoryInj;
        private Injected<IBlobFactory> _blobFactoryInj;
        private Injected<ContentAssetHelper> _contentAssetHelperInj;

        public void Initialize(InitializationEngine context)
        {
            _contentEventsInj.Service.SavingContent += FaceImagePublishCreateEvent;
        }

        public void Uninitialize(InitializationEngine context)
        {
            _contentEventsInj.Service.SavingContent -= FaceImagePublishCreateEvent;
        }

        private void FaceImagePublishCreateEvent(object sender, ContentEventArgs e)
        {

            ContentData content = e.Content as ContentData;
            SaveContentEventArgs savingEvent = e as SaveContentEventArgs;
            if ((savingEvent.Action & EPiServer.DataAccess.SaveAction.CheckOut) != EPiServer.DataAccess.SaveAction.Default || content == null || ContentReference.IsNullOrEmpty(e.ContentLink))
                return;
            Dictionary<string, EPiFaceImageAttribute> faceImageProperties = ListFaceImageProperties(e);

            if (faceImageProperties.Count == 0)
                return;

            ContentAssetFolder contentAssetFolder = _contentAssetHelperInj.Service.GetOrCreateAssetFolder(e.ContentLink);
            if (contentAssetFolder == null)
                return;
            foreach (var faceImagePropertyDef in faceImageProperties)
            {
                ContentReference imageReference = content.GetPropertyValue<ContentReference>(faceImagePropertyDef.Key);
                if (ContentReference.IsNullOrEmpty(imageReference))
                    continue;

                ImageData image;
                if (!_contentLoaderInj.Service.TryGet(imageReference, out image) || image.GetOriginalType() == typeof(FaceImageData) || image.BinaryData == null)
                    continue;

                Image imageDetails = Image.FromStream(new MemoryStream(image.BinaryData.ReadAllBytes()));
                if (imageDetails == null)
                    continue;

                byte[] faceResizedImage;

                if (!TryToDetectFaceAndResizeImage(faceImagePropertyDef, image, imageDetails, out faceResizedImage))
                    continue;

                ContentReference faceImageId = CreateFaceMediaFile(contentAssetFolder, image, faceResizedImage);
                content[faceImagePropertyDef.Key] = faceImageId;
            }
        }

        private static Dictionary<string, EPiFaceImageAttribute> ListFaceImageProperties(ContentEventArgs e)
        {
            Dictionary<string, EPiFaceImageAttribute> faceImageProperties = new Dictionary<string, EPiFaceImageAttribute>();
            Type originalContentType = e.Content.GetOriginalType();
            foreach (var propertyInfo in originalContentType.GetProperties().Where(q => q.CustomAttributes.Any(a => a.AttributeType == typeof(EPiFaceImageAttribute))))
            {
                EPiFaceImageAttribute faceAttribute = propertyInfo.GetCustomAttribute<EPiFaceImageAttribute>();
                if (faceAttribute == null)
                    continue;
                faceImageProperties.Add(propertyInfo.Name, faceAttribute);
            }

            return faceImageProperties;
        }

        private bool TryToDetectFaceAndResizeImage(KeyValuePair<string, EPiFaceImageAttribute> faceImagePropertyDef, ImageData image, Image imageDetails, out byte[] faceResizedImage)
        {
            faceResizedImage = null;
            IList<DetectedFace> detectedFaces = FaceRecognitionService.DetectFace(image.BinaryData);

            if (detectedFaces?.Count == 0)
                return false;

            DetectedFace firstFace = detectedFaces.FirstOrDefault();
            FaceImageDimensions faceImageDimensions = new FaceImageDimensions(firstFace, faceImagePropertyDef.Value.FacePercentage, imageDetails);

            faceResizedImage = ImageResizeService.CutAndResizeImage(image.BinaryData.ReadAllBytes(), faceImageDimensions, faceImagePropertyDef.Value.Width, faceImagePropertyDef.Value.Height);
            return true;
        }

        private ContentReference CreateFaceMediaFile(ContentAssetFolder contentAssetFolder, ImageData image, byte[] faceResizedImage)
        {
            FaceImageData faceImage = _contentRepositoryInj.Service.GetDefault<FaceImageData>(contentAssetFolder.ContentLink);
            faceImage.Name = $"autogenerated_{image.Name}";

            Blob imageBlob = _blobFactoryInj.Service.CreateBlob(faceImage.BinaryDataContainer, ".png");
            using (Stream blobWriter = imageBlob.OpenWrite())
            using (MemoryStream memoryStream = new MemoryStream(faceResizedImage))
            {
                memoryStream.CopyTo(blobWriter);
            }
            faceImage.BinaryData = imageBlob;
            ContentReference faceImageId = _contentRepositoryInj.Service.Save(faceImage, EPiServer.DataAccess.SaveAction.Publish);
            return faceImageId;
        }
    }
}
