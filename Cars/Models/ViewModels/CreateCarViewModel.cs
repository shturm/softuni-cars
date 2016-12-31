using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cars.Models.ViewModels
{
    public class CreateCarViewModel
    {
        public List<SelectListItem> Makes{ get; set; }
        public List<SelectListItem> Users{ get; set; }
        public int SelectedMakeID { get; set; }
        public string CarModel { get; set; }
        public string SelectedOwnerID { get; internal set; }

        public CreateCarViewModel()
        {
            Makes = new List<SelectListItem>();
            Users = new List<SelectListItem>();
        }
        public CreateCarViewModel(IEnumerable<Manufacturer> makes, IEnumerable<ApplicationUser> users)
        {
            Makes = makes.Select(m => new SelectListItem() { Text = m.Name, Value = m.ManufacturerID.ToString() }).ToList();
            Users = users.Select(u => new SelectListItem() { Text = u.UserName, Value = u.Id }).ToList();
        }
    }
}