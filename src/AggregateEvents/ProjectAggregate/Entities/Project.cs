using AggregateEvents.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace AggregateEvents.Model
{
    /// <summary>
    /// Project has a number of tasks
    /// Project state depends on task state
    /// Project invariant: cannot have total hours more than _hoursLimit
    /// </summary>
    public class Project : Entity, IAggregateRoot
    {
        public string Name { get; set; }
        public string Status { get; private set; } = "New";

        private ObservableCollection<Task> _tasks = new ObservableCollection<Task>();
        private List<string> _activityLog = new List<string>();
        private int _hoursLimit = 10; // total hours cannot exceed this amount

        public IEnumerable<Task> Tasks
        {
            get { return _tasks; }
        }

        public Project()
        {
            _tasks.CollectionChanged += Tasks_CollectionChanged;
        }

        private void Tasks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var task in e.NewItems.OfType<Task>())
                {
                    task.Completed += HandleTaskCompleted;
                    task.HoursUpdated += HandleTaskHoursUpdated;
                }

                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var task in e.OldItems.OfType<Task>())
                {
                    task.Completed -= HandleTaskCompleted;
                    task.HoursUpdated -= HandleTaskHoursUpdated;
                }

                return;
            }
        }

        private void HandleTaskHoursUpdated(object sender, TaskHoursUpdatedEvent taskHoursUpdatedEvent)
        {
            if (taskHoursUpdatedEvent.Task.ProjectId != Id) return;
            if(!VerifyHoursWithinLimit())
            {
                Log("Update would exceed project hour limit.");
                taskHoursUpdatedEvent.CancelRequested = true;
                return;
            }
            UpdateStatus();
        }

        private bool VerifyHoursWithinLimit(int newTaskHours = 0)
        {
            return _tasks.Sum(t => t.HoursRemaining) + newTaskHours <= _hoursLimit;
        }

        private void HandleTaskCompleted(object sender, TaskCompletedEvent taskCompletedEvent)
        {
            if (taskCompletedEvent.Task.ProjectId != Id) return;
            UpdateStatus();
            Log($"{taskCompletedEvent.Task.Name} completed.");
        }

        private void UpdateStatus()
        {
            if (!Tasks.Any())
            {
                Status = "New";
                return;
            }
            if (Tasks.Any(t => t.IsComplete))
            {
                Status = "Making Progress";
            }
            if (Tasks.All(t => t.IsComplete))
            {
                Status = "Done!";
            }
            if (Tasks.All(t => !t.IsComplete))
            {
                Status = "Not Started";
            }
        }

        private void Log(string message)
        {
            _activityLog.Add(message);
        }

        public void AddTask(string name, int hoursRemaining)
        {
            var task = new Task(name, hoursRemaining, Id);
            if (hoursRemaining < 0)
            {
                Log("Can't add a task with negative hours remaining.");
                return;
            }
            if (!VerifyHoursWithinLimit(hoursRemaining))
            {
                Log("Can't add a task that will exceed project hours limit.");
                return;
            }
            _tasks.Add(task);
            UpdateStatus();
            Log($"{task.Name} added.");
        }

        public void DeleteTask(Guid id)
        {
            var taskToDelete = _tasks.SingleOrDefault(t => t.Id == id);
            if (taskToDelete == null) return;
            _tasks.Remove(taskToDelete);

            // why raise this event here?
            // TODO: Replace with Domain Event?
            AggregateEvents.Raise(new TaskDeletedEvent(taskToDelete));
            Log($"{taskToDelete.Name} deleted.");
        }

        public override string ToString()
        {
            int hoursRemaining = _tasks.Sum(t => t.HoursRemaining);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Project: {Name} ({Id})");
            sb.AppendLine($"Status: {Status} {hoursRemaining} hours");
            sb.AppendLine("Tasks:");
            sb.AppendLine("--------------");
            foreach (var task in _tasks)
            {
                sb.AppendLine($"Task: {task.Name} {task.HoursRemaining} hours; Complete? {task.IsComplete}");
            }
            sb.AppendLine("Activity Log:");
            sb.AppendLine("--------------");
            foreach (string item in _activityLog)
            {
                sb.AppendLine(item);
            }
            return sb.ToString();
        }
    }
}