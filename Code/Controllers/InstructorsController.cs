using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Code.DAL;
using Code.Models;
using Code.ViewModels;
using System.Data.Entity.Infrastructure;
using System;
using System.Collections.Generic;

namespace Code.Controllers
{
    public class InstructorsController : Controller
    {
        private dbcontext db = new dbcontext();

        // GET: Instructors
        public ActionResult Index(int? id, int? courseID)
        {
            var viewModel = new InstructorIndexData();
            viewModel.Instructors = db.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.Courses.Select(c => c.Department))
                .OrderBy(i => i.LastName);
            if (id != null)
            {
                ViewBag.InstructorID = id.Value;
                viewModel.Courses = viewModel.Instructors.Where(
                    i => i.ID == id.Value).Single().Courses;
            }
            if (courseID != null)
            {
                ViewBag.CourseID = id.Value;
                //viewModel.Enrollments = viewModel.Courses.Where(
                //    i => i.CourseID == courseID).Single().Enrollments;
                var selectedCourse = viewModel.Courses.Where(x => x.CourseID == courseID).Single();
                db.Entry(selectedCourse).Collection(x => x.Enrollments).Load();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    db.Entry(enrollment).Reference(x => x.Student).Load();
                }

                viewModel.Enrollments = selectedCourse.Enrollments;

            }
            return View(viewModel);
        }

        // GET: Instructors/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = db.Instructors.Find(id);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }

        // GET: Instructors/Create
        public ActionResult Create()
        {
            var instructor = new Instructor();
            instructor.Courses = new List<Course>();
            PopulateAssignedCourseData(instructor);
            return View();
        }

        // POST: Instructors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,LastName,FirstName,HireDate,Location")] Instructor instructor, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                instructor.Courses = new List<Course>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = db.Courses.Find(int.Parse(course));
                    instructor.Courses.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                db.Instructors.Add(instructor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: Instructors/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = db.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.Courses)
                .Where(i => i.ID == id)
                .Single();
            PopulateAssignedCourseData(instructor);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            ViewBag.officeID = new SelectList(db.OfficeAssignments, "InstructorID", "Location", instructor.ID);
            return View(instructor);
        }
        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourse = db.Courses;
            var instructorCourses = new HashSet<int>(instructor.Courses.Select(c => c.CourseID));
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourse)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewBag.Courses = viewModel;
        }

        // POST: Instructors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Instructor instructor, string[] selectedCourses)
        {
            if (instructor == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var instructorToUpdate = db.Instructors
               .Include(i => i.OfficeAssignment)
               .Include(i => i.Courses)
               .Where(i => i.ID == instructor.ID)
               .Single();
            if (ModelState.IsValid)
            {
                if (TryUpdateModel(instructorToUpdate, "",
               new string[] { "LastName", "FirstName", "HireDate", "OfficeAssignment" }))
                {
                 //   db.Entry(instructor).State = EntityState.Modified;
                    try
                    {
                        if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment.Location))
                        {
                            instructor.OfficeAssignment = null;
                        }
                        UpdateInstructorCourses(selectedCourses, instructorToUpdate);

                        //else
                        //{
                        //    OfficeAssignment _of = db.OfficeAssignments.Where(x => x.InstructorID == instructor.ID).FirstOrDefault();
                        //    if (_of == null)
                        //    {
                        //        _of = new OfficeAssignment();

                        //        _of.InstructorID = instructor.ID;
                        //        _of.Location = instructor.OfficeAssignment.Location;
                        //        db.OfficeAssignments.Add(_of);
                        //    }
                        //    else
                        //        db.Entry(_of).State = EntityState.Modified;
                        //}
                    }

                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                }
                UpdateInstructorCourses(selectedCourses, instructorToUpdate);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            return View(instructor);
        }
        private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.Courses = new List<Course>();
                return;
            }
                var selectedCoursesHS = new HashSet<string>(selectedCourses);
                var instructorCourses = new HashSet<int>
                    (instructorToUpdate.Courses.Select(c => c.CourseID));
                foreach (var course in db.Courses)
                {
                    if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                    {
                        if (!instructorCourses.Contains(course.CourseID))
                        {
                            instructorToUpdate.Courses.Add(course);
                        }
                    }
                    else
                    {
                        if (instructorCourses.Contains(course.CourseID))
                        {
                            instructorToUpdate.Courses.Remove(course);
                        }
                    }
                }
            }
        


        // GET: Instructors/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = db.Instructors.Find(id);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }

        // POST: Instructors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Instructor instructor = db.Instructors
                .Include(i => i.OfficeAssignment)
                .Where(i => i.ID == id)
                .Single();
                
            db.Instructors.Remove(instructor);
            var department = db.Departments
                .Where(d => d.InstructorID == id)
                .SingleOrDefault();
            if(department != null)
            {
                department.InstructorID = null;
            }
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
