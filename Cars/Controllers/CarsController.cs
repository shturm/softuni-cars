using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Cars.Models;
using Cars.Models.ViewModels;
using System.Diagnostics;
using Microsoft.AspNet.Identity;
using System.Data.Entity.Validation;

namespace Cars.Controllers
{
    public class CarsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Cars
        public ActionResult Index(int page = 1, string sort = "created")
        {
            int pageSize = 5;
            int skip = page == 1 ? 0 : (page - 1)*pageSize;
            var query = db.Cars.Include(c => c.Likes);
            if (sort == "created")
            {
                query = query.OrderByDescending(c => c.Created);
            } else if (sort == "likes")
            {
                query = query.OrderByDescending(c => c.Likes.Count());
            } else
            {
                query = query.OrderByDescending(c => c.Likes.Count());            
            }

            var cars = query.Skip(skip).Take(pageSize).ToList();
            return View(cars);
        }

        public ActionResult My(int page = 1, string sort = "created")
        {
            int pageSize = 5;
            int skip = page == 1 ? 0 : (page - 1) * pageSize;
            var userId = User.Identity.GetUserId();
            
            
            var query = db.Cars.Include(c => c.Likes).Where(c=>c.OwnerID==userId);
            if (sort == "created")
            {
                query = query.OrderByDescending(c => c.Created);
            }
            else if (sort == "likes")
            {
                query = query.OrderByDescending(c => c.Likes.Count());
            }
            else
            {
                query = query.OrderByDescending(c => c.Likes.Count());
            }

            var cars = query.Skip(skip).Take(pageSize).ToList();
            return View(cars);
        }

        // GET: Cars/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = db.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }

            var likesByMonth = db.Likes
                .Where(l => l.CarID == id)
                .GroupBy(l => new { Month = l.Created.Month, Year = l.Created.Year })
                .Select(l => new {
                    Count = l.Count(),
                    Date = l.Key.Year +"-"+l.Key.Month
                }).ToList();

            int[] chartData = likesByMonth.Select(l => l.Count).ToArray();
            string[] chartLabels = likesByMonth.Select(l => "\""+l.Date+"\"").ToArray();

            ViewData["chartData"] = string.Join(",",chartData);
            ViewData["chartLabels"] = string.Join(",",chartLabels);

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                var myLikesThisMonth = db.Likes
                    .Where(l => l.AuthorID == userId && l.CarID == id) // my likes for this car
                    .Where(l => l.Created.Month == DateTime.UtcNow.Month && l.Created.Year == DateTime.UtcNow.Year) // one like per month
                    .ToList()
                    .Count();
                if (myLikesThisMonth == 0)
                {
                    ViewData["canLike"] = true;
                } else
                {
                    ViewData["canLike"] = false;
                }
            } else
            {
                ViewData["canLike"] = false;
            }

            return View(car);
        }

        [Authorize]
        public ActionResult Like(int carId)
        {
            var userId = User.Identity.GetUserId();
            var myLikesThisMonth = db.Likes
                .Where(l => l.AuthorID == userId && l.CarID == carId) // my likes for this car
                .Where(l => l.Created.Month != DateTime.UtcNow.Month && l.Created.Year != DateTime.UtcNow.Year) // one like per month
                .Count();
            if (myLikesThisMonth > 0) return RedirectToAction("Index");

            var like = new Like() { CarID = carId, AuthorID = userId, Created = DateTime.UtcNow };
            db.Likes.Add(like);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = carId });
        }

        public ActionResult Reviews(int carId, int reviewsPage = 1)
        {
            int pageSize = 5;
            int skip = reviewsPage == 1 ? 0 : (reviewsPage - 1) * pageSize;
            var reviews = db.Review
                .Where(r => r.CarID == carId)
                .OrderByDescending(c => c.ReviewID)
                .Skip(skip).Take(pageSize).ToList();

            return View(reviews);
        }

        // GET: Cars/Create
        [Authorize]
        public ActionResult Create()
        {
            var makes = db.Manufacturers.ToList();
            var users = db.Users.ToList();
            var vm = new CreateCarViewModel(makes,users);

            return View(vm);
        }

        // POST: Cars/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "CarModel,SelectedMakeId")] CreateCarViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = db.Users.Find(User.Identity.GetUserId());
                try
                {
                    var car = new Car()
                    {
                        Make = db.Manufacturers.Find(model.SelectedMakeID),
                        Model = model.CarModel,
                        Owner =User.IsInRole("Admin") ? db.Users.Find(model.SelectedOwnerID) : currentUser,
                        Created = DateTime.UtcNow
                    };
                    db.Cars.Add(car);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
                return RedirectToAction("Index");
            } else
            {
                foreach (ModelState modelState in ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        Debug.WriteLine(error.ErrorMessage);
                    }
                }
            }

            return View();
        }

        // GET: Cars/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = db.Cars.Find(id);
            if (car == null || car.OwnerID != User.Identity.GetUserId() && !User.IsInRole("Admin"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            if (car == null)
            {
                return HttpNotFound();
            }

            var users = db.Users.ToList();
            var makes = db.Manufacturers.ToList();
            var model = new EditCarViewModel(makes, users) {
                CarID =  car.CarID,
                Model = car.Model,
                MakeID = car.Make.ManufacturerID,
                OwnerID = car.OwnerID
            };
            return View(model);
        }

        // POST: Cars/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(EditCarViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var car = db.Cars.Find(vm.CarID);
                if(car == null || car.OwnerID != User.Identity.GetUserId() && !User.IsInRole("Admin"))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                if (User.IsInRole("Admin"))
                {
                    car.OwnerID = vm.OwnerID;
                }
                car.ManufacturerID = vm.MakeID;
                car.Model = vm.Model;
                //db.Entry(model).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
                return RedirectToAction("Index");
            }
            
            return View(vm);
        }

        // GET: Cars/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Car car = db.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Car car = db.Cars.Find(id);
            db.Cars.Remove(car);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
