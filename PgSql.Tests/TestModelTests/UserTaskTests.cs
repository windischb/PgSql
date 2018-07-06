using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using doob.PgSql;
using doob.PgSql.Tables;
using PgSql.Tests.TestModels;
using Xunit;
using Xunit.Abstractions;

namespace PgSql.Tests.TestModelTests
{
    public class UserTaskTests
    {
        private readonly ITestOutputHelper _output;

        internal Schema _Schema;

        public UserTaskTests(ITestOutputHelper output)
        {
            _output = output;

            _Schema = Static.GetSchema("UserTaskTests");
        }

        [Fact]
        public async Task Test1()
        {

            _Schema.TableDrop("UserTasks");
            var table = _Schema.CreateTable<UserTaskDbModel>("UserTasks");

           // var dt = DateTime.Parse("2018-06-28 20:20:53.194411");
            var dt = DateTime.Now;
            var usertasks1 = new List<UserTaskDbModel>();
            for (int i = 0; i < 100; i++)
            {
                var usertask = new UserTaskDbModel();
                usertask.MetaData.Add("dt", dt);
                usertask.AssignedTo = "user1";
                usertask.UpdatedAt = dt;
                usertask.UserTaskStatus = i % 2 != 0 ? UserTaskStatus.New : UserTaskStatus.InProgress;
                usertasks1.Add(usertask);
            }
            table.Insert(usertasks1);



            var dt1 = dt.AddDays(7);
            var usertasks2 = new List<UserTaskDbModel>();
            for (int i = 0; i < 20; i++)
            {
                var usertask = new UserTaskDbModel();
                usertask.MetaData.Add("dt", dt);
                usertask.AssignedTo = "user2";
                usertask.UpdatedAt = dt1;
                usertask.UserTaskStatus = i % 2 != 0 ? UserTaskStatus.New : UserTaskStatus.Completed;
                usertasks2.Add(usertask);
            }
            table.Insert(usertasks2);

            var dt2 = dt.AddDays(14);
            var usertasks3 = new List<UserTaskDbModel>();
            for (int i = 0; i < 30; i++)
            {
                var usertask = new UserTaskDbModel();
                usertask.MetaData.Add("dt", dt);
                usertask.AssignedTo = $"user{i + 5}";
                usertask.UpdatedAt = dt2;
                usertask.UserTaskStatus = UserTaskStatus.Canceled;
                usertasks3.Add(usertask);
            }
            table.Insert(usertasks3);


            var q1 = table.Queryable().Where(r => r.AssignedTo == "user1").ToList();
            var q1count = q1.Count;

            Assert.Equal(100, q1count);

            var q2 = table.Queryable().Where(r => r.AssignedTo == "user2").ToList();
            var q2count = q2.Count;

            Assert.Equal(20, q2count);


            var q3 = table.Queryable().Where(r => r.AssignedTo != "user2").ToList();
            var q3count = q3.Count;
            Assert.Equal(130, q3count);


            var q4 = table.Queryable().Count(r => r.UpdatedAt == dt2);
            Assert.Equal(30, q4);



            var q5 = table.Queryable().Count(r =>
                r.UserTaskStatus == UserTaskStatus.Canceled || r.UserTaskStatus == UserTaskStatus.Completed);
            Assert.Equal(40, q5);

        }

        [Fact]
        public async Task GeneratedData()
        {

            _Schema.TableDrop("GeneratedUserTasks");
            var table = _Schema.CreateTable<UserTaskDbModel>("GeneratedUserTasks");

            table.Insert(createModel(5));

            //var q5 = table.Queryable().Count(r =>
            //    r.UserTaskStatus == UserTaskStatus.Canceled || r.UserTaskStatus == UserTaskStatus.Completed);

            //_output.WriteLine($"q5 = {q5}");

            //Assert.True(q5 > 1);


            var q6 = table.Queryable().Where(r => r.Permissions.Any(p => p.Value == "test")).ToList();

            _output.WriteLine($"q6 = {q6.Count()}");

        }

        private List<UserTaskDbModel> createModel(int count)
        {

            var fakePerms = new Faker<UserTaskPermissionRule>()
                .RuleFor(p => p.Value, f => f.Person.Email)
                .RuleFor(p => p.AccessRights, f => f.PickRandom<UserTaskPermissionRights>())
                .RuleFor(p => p.SourceType, f => f.PickRandom<PermissionRuleSourceType>());

            var fakeUserTasks = new Faker<UserTaskDbModel>()
                .RuleFor(u => u.AssignedTo, f => f.Person.Email)
                .RuleFor(u => u.UpdatedAt, f => f.Date.Soon(7))
                .RuleFor(u => u.UserTaskStatus, f => f.PickRandom<UserTaskStatus>())
                .RuleFor(u => u.Permissions, f => fakePerms.Generate(5).ToList());

            return fakeUserTasks.Generate(count);
        }
    }
}
