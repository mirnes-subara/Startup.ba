using Startupba.Model;
using Startupba.Services.Database;
using System;
using System.Collections.Generic;

namespace Startupba.Services.Helpers
{
    /// <summary>
    /// Allowed startup status transitions. Every path (admin, founder, funding complete)
    /// must go through <see cref="EnsureCanTransition"/> before changing StatusId.
    /// </summary>
    public static class StartupStatusMachine
    {
        private static readonly HashSet<(int From, int To)> Allowed = new()
        {
            (StartupStatuses.Draft, StartupStatuses.Pending),
            (StartupStatuses.Pending, StartupStatuses.Approved),
            (StartupStatuses.Pending, StartupStatuses.Rejected),
            (StartupStatuses.Pending, StartupStatuses.Deleted),
            (StartupStatuses.Rejected, StartupStatuses.Pending),
            (StartupStatuses.Rejected, StartupStatuses.Deleted),
            (StartupStatuses.Approved, StartupStatuses.Paused),
            (StartupStatuses.Approved, StartupStatuses.Completed),
            (StartupStatuses.Approved, StartupStatuses.Deleted),
            (StartupStatuses.Paused, StartupStatuses.Approved),
            (StartupStatuses.Paused, StartupStatuses.Deleted),
            (StartupStatuses.Completed, StartupStatuses.Deleted),
        };

        public static bool CanTransition(int fromStatusId, int toStatusId)
            => Allowed.Contains((fromStatusId, toStatusId));

        public static void EnsureCanTransition(int fromStatusId, int toStatusId)
        {
            if (CanTransition(fromStatusId, toStatusId))
                return;

            throw new UserException(
                $"Cannot change startup status from {Name(fromStatusId)} to {Name(toStatusId)}.");
        }

        /// <summary>
        /// Validates the transition, updates snapshot audit fields, and returns a history row
        /// (caller must add it to the context).
        /// </summary>
        public static StartupStatusHistory Transition(
            Startup entity,
            int toStatusId,
            int? actorUserId,
            string? reason = null)
        {
            var fromStatusId = entity.StatusId;
            EnsureCanTransition(fromStatusId, toStatusId);

            var now = DateTime.UtcNow;
            entity.StatusId = toStatusId;
            entity.UpdatedAt = now;

            if (toStatusId == StartupStatuses.Approved)
            {
                entity.ApprovedAt = now;
                entity.ApprovedByUserId = actorUserId;
                entity.RejectionReason = null;
                entity.RejectedByUserId = null;
            }
            else if (toStatusId == StartupStatuses.Rejected)
            {
                entity.RejectedByUserId = actorUserId;
                entity.RejectionReason = reason;
            }
            else if (toStatusId == StartupStatuses.Paused)
            {
                entity.PausedAt = now;
                entity.PausedByUserId = actorUserId;
            }
            else if (toStatusId == StartupStatuses.Completed)
            {
                entity.CompletedAt = now;
            }
            else if (toStatusId == StartupStatuses.Pending && fromStatusId == StartupStatuses.Rejected)
            {
                entity.RejectionReason = null;
                entity.RejectedByUserId = null;
            }
            else if (toStatusId == StartupStatuses.Deleted)
            {
                entity.IsActive = false;
            }

            return new StartupStatusHistory
            {
                StartupId = entity.Id,
                FromStatusId = fromStatusId,
                ToStatusId = toStatusId,
                ActorUserId = actorUserId,
                Reason = reason,
                CreatedAt = now
            };
        }

        public static string Name(int statusId) => statusId switch
        {
            StartupStatuses.Draft => "Draft",
            StartupStatuses.Pending => "Pending",
            StartupStatuses.Approved => "Approved",
            StartupStatuses.Rejected => "Rejected",
            StartupStatuses.Paused => "Paused",
            StartupStatuses.Completed => "Completed",
            StartupStatuses.Deleted => "Deleted",
            _ => statusId.ToString()
        };
    }
}
