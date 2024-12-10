using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace kuaforBerberOtomasyon.Models.DTO
{
    public class AdminEmployeeRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "At least one service must be selected")]
        public List<int> ServiceId { get; set; }  // List<int> olmalı, çünkü birden fazla hizmet seçilebilir

        public List<SelectListItem> ServiceList { get; set; }
    }


}

