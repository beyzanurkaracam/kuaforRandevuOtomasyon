using kuaforBerberOtomasyon.Enums;

namespace kuaforBerberOtomasyon.Models
{
    public class HairCutSuggestion
    {
        public FaceShape FaceShape { get; set; }
        public string Suggestion { get; set; }
        public string ImageUrl { get; set; }
    }
}
