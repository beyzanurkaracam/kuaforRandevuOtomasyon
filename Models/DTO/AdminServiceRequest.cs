namespace kuaforBerberOtomasyon.Models.DTO
{
    public class AdminServiceRequest
    {
        public String Name { get; set; } // Enum kullanımı
        public int Duration { get; set; } // Dakika cinsinden
        public decimal Price { get; set; }
    }
}
