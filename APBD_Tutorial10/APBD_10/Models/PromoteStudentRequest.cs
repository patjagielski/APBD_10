using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APBD_10.Models
{
    public class PromoteStudentRequest
    {
        public int IdEnrollment { get; set; }
        public string StudyName { get; set; }
        public int Semester { get; set; }
    }
}
