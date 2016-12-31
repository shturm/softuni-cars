using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cars.Models.ViewModels
{
    public class EditCarViewModel
    {
        [Required]
        public int CarID { get; set; }
        [Required]
        [StringLength(255)]
        public string Model { get; set; }
        [Required]
        public int MakeID { get; set; }
        public string OwnerID { get; set; }

        public IEnumerable<SelectListItem> Makes { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }

        public EditCarViewModel()
        {
            Makes = new List<SelectListItem>();
            Users = new List<SelectListItem>();
        }

        public EditCarViewModel(IEnumerable<Manufacturer> makes, IEnumerable<ApplicationUser> users)
        {
            Makes = makes.Select(m => new SelectListItem() {
                Text = m.Name,
                Value = m.ManufacturerID.ToString(),
                Selected = m.ManufacturerID == MakeID }).ToList();
            Users = users.Select(u => new SelectListItem() {
                Text = u.UserName,
                Value = u.Id,
                Selected = u.Id == OwnerID}).ToList();
        }
    }
}