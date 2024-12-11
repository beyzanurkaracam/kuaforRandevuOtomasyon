using kuaforBerberOtomasyon.Enums;
using kuaforBerberOtomasyon.Models;

namespace kuaforBerberOtomasyon.AIService
{
    public class HairCutRecommendationService
    {
        private List<HairCutSuggestion> _suggestions = new List<HairCutSuggestion>
        {
            new HairCutSuggestion
            {
                FaceShape = FaceShape.Round,
                Suggestion = "Long haircuts, straight styles are great for round faces.",
                ImageUrl = "path_to_image/round_face_long_hair.jpg"
            },
            new HairCutSuggestion
            {
                FaceShape = FaceShape.Square,
                Suggestion = "Soften the look with layered or long cuts.",
                ImageUrl = "path_to_image/square_face_long_layers.jpg"
            },
            new HairCutSuggestion
            {
                FaceShape = FaceShape.Oval,
                Suggestion = "You can go for various hair styles; oval faces suit most cuts.",
                ImageUrl = "path_to_image/oval_face_various_styles.jpg"
            },
            new HairCutSuggestion
            {
                FaceShape = FaceShape.Triangle,
                Suggestion = "For a triangular face, voluminous styles on top balance the look.",
                ImageUrl = "path_to_image/triangle_face_voluminous.jpg"
            }
        };

            public HairCutSuggestion GetSuggestion(FaceShape faceShape)
            {
                return _suggestions.FirstOrDefault(s => s.FaceShape == faceShape);
            }
        }
    }
