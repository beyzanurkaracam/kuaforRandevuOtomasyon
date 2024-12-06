using kuaforBerberOtomasyon.Models.Entities;

namespace kuaforBerberOtomasyon.Models.DTO
{
    public class ServicesRespond
    {
        public int serviceId { get; set; }
        public String Name { get; set; } // Enum kullanımı
        public int Duration { get; set; } // Dakika cinsinden
        public decimal Price { get; set; }
        
    }
}
