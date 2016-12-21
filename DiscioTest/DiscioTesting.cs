
using Discio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiscioTest {
    [TestClass]
    public class DiscioTesting {

        private const string USER_DATA = "users";
        private const string DATA_STORAGE = @"C:\Temp\Data";
        private const string DATA_SOURCE = "main";

        [TestMethod]
        public void CreateUserRepo() {
            var main = new Source(DATA_STORAGE);

            if (!main.Exists(USER_DATA)) {
                main.Create(USER_DATA);
            }

            Assert.IsTrue(main.Exists(USER_DATA));
        }

        private static void CleanUpDeleteUser(User user) {
            var man = new Manager<User>(USER_DATA);
            man.Delete(user);
            man.Commit();
        }

        private static User PrepareAddUser(User user) {
            var man = new Manager<User>(USER_DATA);
            man.Insert(user);
            man.Commit();
            return man.First();
        }

        public void AddSource() {
            if (SiteSources.Sources.Count == 0) {
                SiteSources.Sources.Add(DATA_SOURCE, new Source(DATA_STORAGE));
            }
            Assert.IsTrue(SiteSources.Sources.Count > 0);
        }

        [TestMethod]
        public void AddUser() {
            AddSource();
            var user = new User {
                FirstName = "AddUser",
                LastName = "Sample",
                Age = 30
            };
            var man = new Manager<User>(USER_DATA);
            man.Insert(user);
            man.Commit();
            var r = man.First();
            Assert.IsNotNull(r);
            Assert.IsNotNull(r.ID);
            Assert.IsTrue(r.ID.Length > 0);
            CleanUpDeleteUser(r);
        }

        [TestMethod]
        public void DeleteUser() {
            AddSource();

            var user = new User {
                FirstName = "DeleteUser",
                LastName = "Sample",
                Age = 30
            };

            var saved = PrepareAddUser(user);

            var man = new Manager<User>(USER_DATA);

            man.Delete(m => m.ID == saved.ID);
            man.Commit();

            var total = man.Count();

            Assert.AreEqual(total, 0);
        }

    }


    public class User : IDiscio {
        public string ID { set; get; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public int Age { set; get; }
    }
}
