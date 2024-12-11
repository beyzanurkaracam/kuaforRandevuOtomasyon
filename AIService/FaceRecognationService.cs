using kuaforBerberOtomasyon.Enums;

namespace kuaforBerberOtomasyon.AIService
{
    public class FaceRecognationService
    {
        /*private readonly IFaceClient _faceClient;

        public FaceRecognitionService(string apiKey, string endpoint)
        {
            // Azure Face API'ye bağlanmak için gerekli istemciyi oluşturuyoruz
            _faceClient = new FaceClient(new ApiKeyServiceClientCredentials(apiKey)) { Endpoint = endpoint };
        }

        public async Task<FaceShape?> GetFaceShape(string imagePath)
        {
            try
            {
                var imageStream = File.OpenRead(imagePath);

                // Yüz analizi yapın
                var detectedFaces = await _faceClient.Face.DetectWithStreamAsync(imageStream, returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.FaceShape });

                if (detectedFaces.Count > 0)
                {
                    var faceAttributes = detectedFaces[0].FaceAttributes;

                    // Burada yüz şekline göre sınıflandırma yapıyoruz
                    return ConvertFaceShape(faceAttributes.FaceShape);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting face attributes: {ex.Message}");
                return null;
            }
        }

        private FaceShape? ConvertFaceShape(string faceShape)
        {
            return faceShape switch
            {
                "Round" => FaceShape.Round,
                "Square" => FaceShape.Square,
                "Oval" => FaceShape.Oval,
                "Triangle" => FaceShape.Triangle,
                _ => null
            };
        }*/
    }
}

