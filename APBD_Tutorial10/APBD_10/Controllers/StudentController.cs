using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APBD_10.Entities;
using APBD_10.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBD_10.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly StudentContext _studentContext;
        public StudentController(StudentContext studentContext)
        {
            _studentContext = studentContext;
        }

        [HttpGet]
        public IActionResult GetStudent()
        {
            var students = _studentContext.Student
                                           .Select(s => new GetStudentsResponse
                                           {
                                               IndexNumber = s.StudentId,
                                               FirstName = s.FirstName,
                                               LastName = s.LastName,
                                               BirthDate = s.BirthDate,


                                           }).ToList();
            return Ok(students);
        }

        [HttpPost("AddStudent")]
        public IActionResult InsertStudent(Student st)
        {
            var newStudent = _studentContext.Student
                                            .Add(st);
            _studentContext.SaveChanges();
            return Ok(newStudent);
        }

        [HttpPost("ModifyStudent")]
        public IActionResult ModificationOnStudent(Student st)
        {
            var studentInfo = _studentContext.Student.Where(student => student.StudentId == st.StudentId).ToList().First();

            Student studentObject = studentInfo;

            studentObject.FirstName = st.FirstName;
            studentObject.LastName = st.LastName;
            studentObject.IdEnrollment = st.IdEnrollment;
            studentObject.BirthDate = st.BirthDate;

            _studentContext.Update(studentObject);
            _studentContext.SaveChanges();

            return Ok(studentObject);


        }

        [HttpDelete("{index}")]
        public IActionResult DeleteStudent(string index)
        {
            var to_remove = _studentContext.Student.Where(student => student.StudentId.Equals(index)).ToList().First();
            _studentContext.Student.Remove(to_remove);
            _studentContext.SaveChanges();

            return Ok("Student with Index Number: " + index + " has been removed");
        }



        [HttpPost("Enroll")]
        public IActionResult EnrollNewStudent(EnrollStudentRequest studentRequest)
        {
            var enrollmentId = -1;
            var res = (from study in _studentContext.Studies
                      where study.Name.Equals(studentRequest.StudyName)
                      select study.IdStudy).ToList().First();
            if (res == null)
            {
                return BadRequest();
            }
            var studyId = res;

            var enrollment = (from enroll in _studentContext.Enrollment
                             where enroll.IdStudy == studyId && enroll.Semester == 1
                             select enroll.IdEnrollment).ToList();

            if(enrollment.Count != 0) 
            {
                enrollmentId = _studentContext.Enrollment.Max(e => e.IdEnrollment);
            }
            var to_return = _studentContext.Student.Where(st => st.StudentId == studentRequest.IndexNumber);
            
            Student newStudent = new Student
            {
                StudentId = studentRequest.IndexNumber,
                FirstName = studentRequest.FirstName,
                LastName = studentRequest.LastName,
                BirthDate = studentRequest.BirthDate,
                IdEnrollment = enrollmentId
            };
            _studentContext.Add(newStudent);
            _studentContext.SaveChanges();
            return Ok("Student: " + newStudent.FirstName + " has been enrolled!");
        }

        [HttpPost("Promote")]
        public IActionResult PromoteStudent(PromoteStudentRequest promoteStudentRequest)
        {
            var newSemester = promoteStudentRequest.Semester + 1;
            var newEnrollId = _studentContext.Enrollment.Max(x => x.IdEnrollment) + 1;
            
            var studyName = _studentContext.Studies.Where(x => x.Name == promoteStudentRequest.StudyName).ToString();
            var newStudyId = (from study in _studentContext.Studies
                              where study.Name.Equals(studyName)
                              select study.IdStudy).ToList().FirstOrDefault();

            var newEnrollment = (_studentContext.Enrollment.Where(x => x.Semester == newSemester)).ToList();
            if (newEnrollment.Count == 0)
            {
                _studentContext.Enrollment.Add(new Enrollment
                {
                    IdEnrollment = newEnrollId,
                    IdStudy = newStudyId,
                    Semester = newSemester,
                    StartDate = DateTime.Now
                });
                _studentContext.SaveChanges();
            }
            var previousEnrollId = _studentContext.Enrollment.Where(x => x.Semester.Equals(promoteStudentRequest.Semester) 
                                                                && x.IdStudy.Equals(newStudyId))
                                                             .Select(x => x.IdEnrollment).FirstOrDefault();
            var enrollList = _studentContext.Student.Where(x => x.IdEnrollment.Equals(previousEnrollId)).ToList();
            foreach(Student student in enrollList)
            {
                student.IdEnrollment = newEnrollId;
                _studentContext.Update(student);
            }
            _studentContext.SaveChanges();
            PromoteStudentRequest newPromotion = new PromoteStudentRequest
            {
                    IdEnrollment = newEnrollId,
                    Semester = newSemester,
                    StudyName = studyName
            };
            
           return Ok("Student has been promoted" + newPromotion);
        }
    }
}