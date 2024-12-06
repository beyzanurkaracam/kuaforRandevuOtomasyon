using System.ComponentModel.DataAnnotations;

namespace kuaforBerberOtomasyon.Models.Entities
{
    public class EmployeeService
    {
        [Key]
        public int EmployeeServiceID { get; set; } // Birincil anahtar
        public int EmployeeID { get; set; } // Yabancı anahtar
        public int ServiceID { get; set; } // Yabancı anahtar

        // Diğer özellikler
        public virtual Employee Employee { get; set; }
        public virtual Services Service { get; set; }
    }
}
