using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Code.Models
{
    public class Student
    {
        public int ID { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha de ingreso")]
        public DateTime EnrollmentDate { get; set; }
        

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


        public  ICollection<Enrollment> Enrollements { get; set; }
    }
}