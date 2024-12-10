using System.ComponentModel.DataAnnotations;

namespace kuaforBerberOtomasyon.Models.Entities
{
    public class WorkingHours
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public DateTime baslangicWorkingHour { get; set; }
        public DateTime bitisWorkingHour { get; set; }

        public string EmployeeName { get; set; }
    }
}
