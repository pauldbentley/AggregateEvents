using AggregateEvents.Model;
using System;
using System.Linq;
using Xunit;

namespace AggregateEvents.Tests
{
    public class ProjectNew
    {
        [Fact]
        public void HasStatusNew()
        {
            var project = new Project();

            Assert.Equal("New", project.Status);
        }

        [Fact]
        public void HasZeroTasks()
        {
            var project = new Project();

            Assert.Empty(project.Tasks);
        }

        [Fact]
        public void HandleOwnTaskCompletedEventOnly()
        {
            var project = new Project();
            string taskName = Guid.NewGuid().ToString();
            project.AddTask(taskName, 1);

            // project 2 has no tasks assigned to it.
            var project2 = new Project();

            project.Tasks.First().MarkComplete();

            Assert.Contains(taskName, project.ToString());
            Assert.DoesNotContain(taskName, project2.ToString());
        }

        [Fact]
        public void HandleOwnTaskHoursUpdatedEventOnly()
        {
            var project = new Project();
            string taskName = Guid.NewGuid().ToString();
            project.AddTask(taskName, 1);

            // project 2 has no tasks assigned to it.
            var project2 = new Project();

            project.Tasks.First().UpdateHoursRemaining(2);

            Assert.Contains(taskName, project.ToString());
            Assert.DoesNotContain(taskName, project2.ToString());
        }
    }
}
