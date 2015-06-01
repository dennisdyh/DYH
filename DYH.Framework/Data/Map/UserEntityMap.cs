using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DYH.Framework.Models;

namespace DYH.Framework.Data.Map
{
    class UserEntityMap : EntityTypeConfiguration<UserEntity>
    {
        public UserEntityMap()
        {
            ToTable("Users");
            HasKey(x => x.Id);
            Property(x => x.UserName).HasMaxLength(50).IsRequired();
            Property(x => x.Password).HasMaxLength(50).IsRequired();
            Property(x => x.Email).HasMaxLength(200).IsRequired();
            Property(x => x.ChangedBy).HasMaxLength(50);
            Property(x => x.CreatedBy).HasMaxLength(50);
            Property(x => x.FirstName).HasMaxLength(50);
            Property(x => x.LastName).HasMaxLength(50);
            Property(x => x.Language).HasMaxLength(10);
        }
    }
}
