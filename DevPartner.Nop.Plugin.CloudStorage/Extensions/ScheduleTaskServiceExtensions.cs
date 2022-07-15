#region Copyright
/* Copyright (C) 2016 Fine Support L.P. - All Rights Reserved. 
 *
 * This file is part of DevPartner.Core.
 * 
 * DevPartner.Core can not be copied and/or distributed without the express
 * permission of Fine Support L.P.
 *
 * Written by Kanstantsin Ivinki, July 2016
 * Email: info@dev-partner.biz
 * Website: http://dev-partner.biz
 */
#endregion

using System.Collections.Generic;
using Nop.Core.Domain.Tasks;
using Nop.Services.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{
    public static class ScheduleTaskServiceExtensions
    {

        public static void InsertTaskIfDoesntExist(this IScheduleTaskService scheduleTaskService, ScheduleTask task)
        {
            var dbTask = scheduleTaskService.GetTaskByType(task.Type);
            if (dbTask == null)
            {
                scheduleTaskService.InsertTask(task);
            }
        }

        public static void InsertTasksIfDoesntExist(this IScheduleTaskService scheduleTaskService, List<ScheduleTask> tasks)
        {
            tasks.ForEach(scheduleTaskService.InsertTaskIfDoesntExist);
        }

        public static void DeleteTaksByType(this IScheduleTaskService scheduleTaskService, ScheduleTask task)
        {
            var dbTask = scheduleTaskService.GetTaskByType(task.Type);
            if (dbTask != null)
            {
                scheduleTaskService.DeleteTask(dbTask);
            }
        }

        public static void DeleteTasks(this IScheduleTaskService scheduleTaskService, List<ScheduleTask> tasks)
        {
            tasks.ForEach(scheduleTaskService.DeleteTaksByType);
        }

        public static void Execute(this IScheduleTaskService scheduleTaskService, string taskType)
        {
            var scheduleTask = scheduleTaskService.GetTaskByType(taskType);

            if (scheduleTask != null)
            {
                scheduleTask.Enabled = true;
                var task = new Task(scheduleTask) { Enabled = true };
                task.Execute(true, false);
            }
        }
    }
}
