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

                var user2 = new UserEntity
                {
                    CreatedBy = "carl",
                    CreatedTime = DateTime.UtcNow,
                    Email = "dc0106@126.com",
                    FirstName = "dyh",
                    LastName = "dai",
                    Language = "zh-cn",
                    Password = "123456",
                    UserName = "dennis"
                };

                if (reponsitory.FindAll(x => x.UserName == "admin").Any())
                {
                    var us = reponsitory.FindAll(x => x.UserName == "admin").FirstOrDefault();
                    reponsitory.Delete(us);
                }

                reponsitory.Insert(user);
                reponsitory.Insert(user2);
                
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

                Assert.AreEqual("carl 11", user.FirstName);
                Debug.WriteLine(user.FirstName);
            }
        }

        [TestMethod]
        public void QueryAllTest()
        {
            using (var uof = new UnitOfWork("DbConnStr"))
            {
                var repository = new Repository<UserEntity, int>(uof);
                var list = repository.FindAll(x => true);
                foreach (var item in list)
                {
                    Debug.WriteLine(item.UserName);
                }
            }
        }
    }
}
