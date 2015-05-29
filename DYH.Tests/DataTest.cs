using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using DYH.Framework.Data;
using DYH.Framework.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DYH.Tests
{
    [TestClass]
    public class DataTest
    {
        [TestMethod]
        public void InsertTest()
        {
            using (var uof = new UnitOfWork("DbConnStr"))
            {
                var reponsitory = new Repository<UserEntity, int>(uof);

                var user = new UserEntity
                {
                    CreatedBy = "carl",
                    CreatedTime = DateTime.UtcNow,
                    Email = "CarlDai@bcsint.com",
                    FirstName = "carl",
                    LastName = "dai",
                    Language = "zh-cn",
                    Password = "123456",
                    UserName = "admin"
                };

                reponsitory.Insert(user);

                uof.Commit();
            }
        }

        [TestMethod]
        public void QueryTest()
        {
            using (var uof = new UnitOfWork("DbConnStr"))
            {
                var repository = new Repository<UserEntity, int>(uof);

                var info = repository.FindAll(x => x.UserName == "admin").FirstOrDefault();
                Debug.WriteLine(info.FirstName);
            }
        }

        [TestMethod]
        public void UpdateTest()
        {
            using (var uof = new UnitOfWork("DbConnStr"))
            {
                var repository = new Repository<UserEntity, int>(uof);

                var info = repository.FindAll(x => x.UserName == "admin").FirstOrDefault();
                info.FirstName = "carl 11";
                repository.Update(info);

                uof.Commit();

                var user = repository.FindAll(x => x.UserName == "admin").FirstOrDefault();
                Debug.WriteLine(user.FirstName);
            }
        }


    }
}
