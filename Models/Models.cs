using System;

namespace PensionBase.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
    }

    public class PensionType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Pensioner
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string Snils { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int PensionTypeId { get; set; }
        public string PensionTypeName { get; set; }
        public decimal PensionAmount { get; set; }
        public DateTime AssignedDate { get; set; }

        public string FullName => (LastName + " " + FirstName + " " + MiddleName).Trim();

        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age))
                    age--;
                return age;
            }
        }
    }
}
