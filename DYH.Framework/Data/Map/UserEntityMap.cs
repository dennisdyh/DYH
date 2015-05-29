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
            this.ToTable("Users");
            this.HasKey(x => x.Id);
            this.Property(x => x.UserName).HasMaxLength(50).IsRequired();
            this.Property(x => x.Password).HasMaxLength(50).IsRequired();
            this.Property(x => x.Email).HasMaxLength(200).IsRequired();

        }
    }
}
