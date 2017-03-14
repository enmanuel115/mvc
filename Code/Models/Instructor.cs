using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Code.Models
{
    public class Instructor
    {
        public int ID { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; }

        

        [Required]
        [StringLength(50), Display(Name = "Apellido")]
        public string LastName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "El nombre no puede pasar de 50 caracteres"), Display(Name = "Nombre")]
        [Column("FirstName")]
        public string FirstName { get; set; }

        [Display(Name = "Nombre completo")]
        public string FullName
        {
            get
            {
                return LastName + ", " + FirstName;
            }
        }

        public  ICollection<Course> Courses { get; set; }
        public  OfficeAssignment OfficeAssignment { get; set; }
    }
}