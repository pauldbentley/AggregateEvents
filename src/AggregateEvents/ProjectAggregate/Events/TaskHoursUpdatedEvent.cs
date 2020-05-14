﻿namespace AggregateEvents.Model
{
    public class TaskHoursUpdatedEvent : AggregateEvent
    {
        public Task Task { get; set; }
        public bool CancelRequested { get; set; }

        public TaskHoursUpdatedEvent(Task task)
        {
            Task = task;
        }
    }
}