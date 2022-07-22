#region Copyright
/* Copyright (C) 2016 Dev Partner LLC - All Rights Reserved. 
 *
 * This file is part of DevPartner.Core.
 * 
 * DevPartner.Core can not be copied and/or distributed without the express
 * permission of Dev Partner LLC
 *
 * Written by Kanstantsin Ivinki, July 2016
 * Email: info@dev-partner.biz
 * Website: http://dev-partner.biz
 */
#endregion

using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Infrastructure;
using Nop.Services.ScheduleTasks;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{
    public static class ScheduleTaskServiceExtensions
    {

        public static async Task InsertTaskIfDoesntExistAsync(this IScheduleTaskService scheduleTaskService, ScheduleTask task)
        {
            var dbTask = await scheduleTaskService.GetTaskByTypeAsync(task.Type);
            if (dbTask == null)
            {
                await scheduleTaskService.InsertTaskAsync(task);
            }
        }

        public static async Task InsertTasksIfDoesntExistAsync(this IScheduleTaskService scheduleTaskService, List<ScheduleTask> tasks)
        {
            foreach (var item in tasks)
            {
                await scheduleTaskService.InsertTaskIfDoesntExistAsync(item);
            }
        }

        public static async Task DeleteTaksByTypeAsync(this IScheduleTaskService scheduleTaskService, ScheduleTask task)
        {
            var dbTask = await scheduleTaskService.GetTaskByTypeAsync(task.Type);
            if (dbTask != null)
            {
                await scheduleTaskService.DeleteTaskAsync(dbTask);
            }
        }

        public static async Task DeleteTasksAsync(this IScheduleTaskService scheduleTaskService, List<ScheduleTask> tasks)
        {
            foreach (var item in tasks)
            {
                await scheduleTaskService.DeleteTaksByTypeAsync(item);
            }
        }

        public static async Task ExecuteAsync(this IScheduleTaskService scheduleTaskService, string taskType)
        {
            var scheduleTask = await scheduleTaskService.GetTaskByTypeAsync(taskType);

            if (scheduleTask != null)
            {
                scheduleTask.Enabled = true;
                var scheduleTaskRunner = EngineContext.Current.Resolve<IScheduleTaskRunner>();

                await scheduleTaskRunner.ExecuteAsync(scheduleTask);
            }
        }
    }
}
