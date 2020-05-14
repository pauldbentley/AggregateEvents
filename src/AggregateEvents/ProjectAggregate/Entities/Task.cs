using System;

namespace AggregateEvents.Model
{
    public class Task : Entity
    {
        public Task(string name, 
            int hoursRemaining,
            Guid projectId)
        {
            Name = name;
            HoursRemaining = hoursRemaining;
            ProjectId = projectId;
        }
        private Task()
        {
        }

        public event EventHandler<TaskCompletedEvent> Completed;

        public event EventHandler<TaskHoursUpdatedEvent> HoursUpdated;

        public Guid ProjectId { get; }
        public string Name { get; private set; }
        public bool IsComplete { get; private set; }
        public int HoursRemaining { get; private set; }
        
        public void MarkComplete()
        {
            if (IsComplete) return;
            IsComplete = true;
            HoursRemaining = 0;
            Completed?.Invoke(this, new TaskCompletedEvent(this));
        }

        public void UpdateHoursRemaining(int hours)
        {
            if (hours < 0) return;
            int currentHoursRemaining = HoursRemaining;

            HoursRemaining = hours;
            if (HoursRemaining == 0)
            {
                MarkComplete();
                return;
            }
            IsComplete = false;

            var eventArgs = new TaskHoursUpdatedEvent(this);
            HoursUpdated?.Invoke(this, eventArgs);

            if (eventArgs.CancelRequested)
            {
                HoursRemaining = currentHoursRemaining;
            }
        }
    }
}