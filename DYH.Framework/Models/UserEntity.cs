using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DYH.Core.Data;

namespace DYH.Framework.Models
{
    public class UserEntity : BaseEntity<int>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Language { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string ChangedBy { get; set; }
        public DateTime? ChangedTime { get; set; }
    }
}
