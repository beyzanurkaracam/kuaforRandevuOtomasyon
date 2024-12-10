using kuaforBerberOtomasyon.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace kuaforBerberOtomasyon.Models.DTO
{
    public class EmployeeUpdateDto
    {
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Ad alanı gereklidir.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "En az bir hizmet seçmelisiniz.")]
        public List<int> SelectedServiceIds { get; set; }

        public List<Services> AllServices { get; set; }
    }
}
