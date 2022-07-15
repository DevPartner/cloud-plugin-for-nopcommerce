using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Services
{
    public class MovingItemService
    {
        #region Consts
        private const int LOG_ITEMS_COUNT_MAX = 1000;
        private readonly string SqlScriptSavePictures = "~/Plugins/DevPartner.CloudStorage/SQL/Update_DP_CloudStorage_Queue.sql";

        #endregion

        #region Fields

        private readonly IRepository<ScheduleTask> _scheduleTaskRepository;
        private readonly IRepository<MovingItem> _movingItemRepository;
        private readonly IDbContext _dbContext;
        private readonly IDataProvider _dataProvider;
        private readonly INopFileProvider _fileProvider;

        #endregion

        #region Constr

        public MovingItemService(IRepository<ScheduleTask> scheduleTaskRepository,
            IRepository<MovingItem> movingItemRepository,
            IDbContext dbContext,
            IDataProvider dataProvider,
            INopFileProvider fileProvider)
        {
            _scheduleTaskRepository = scheduleTaskRepository;
            _movingItemRepository = movingItemRepository;
            _dbContext = dbContext;
            _dataProvider = dataProvider;
            _fileProvider = fileProvider;
        }

        #endregion

        #region Utils

        #endregion

        #region Methods

        public void SaveAllPictures(string oldProviderName)
        {
            DbParameter[] parameters = GetParams(oldProviderName);
            var sqlQuery = File.ReadAllText(_fileProvider.MapPath(SqlScriptSavePictures));
            var list = _dbContext.ExecuteSqlCommand(sqlQuery, false, 6000, parameters);
        }

        private DbParameter[] GetParams(string providerName)
        {
            var pProviderName = _dataProvider.GetParameter();
            pProviderName.ParameterName = "@providerName";
            pProviderName.Value = providerName;
            pProviderName.DbType = DbType.String;

            return new DbParameter[] { pProviderName };
        }

        public void Save(IEnumerable<int> ids, MovingItemTypes types, string oldProviderSystemName)
        {

            lock (_movingItemRepository)
            {
                var itemsToUpdate = _movingItemRepository.Table.Where(x => ids.Contains(x.EntityId.Value) && x.Types == types);
                foreach (var item in itemsToUpdate)
                {
                    item.OldProviderSystemName = oldProviderSystemName;
                    item.ChangedOnUtc = DateTime.UtcNow;
                    item.Status = MovingItemStatus.Pending;
                }
                _movingItemRepository.Update(itemsToUpdate);
                var updatedIds = itemsToUpdate.Select(x => x.EntityId);
                var itemIds = ids.Where(x => !updatedIds.Contains(x));
                var items = itemIds.Select(x => new MovingItem
                {
                    EntityId = x,
                    Types = types,
                    OldProviderSystemName = oldProviderSystemName,
                    Status = MovingItemStatus.Pending,
                    CreatedOnUtc = DateTime.UtcNow,
                    ChangedOnUtc = DateTime.UtcNow,
                });
                _movingItemRepository.Insert(items);
            }
        }

        public void UpdateStatus(MovingItem movingItem, MovingItemStatus newStatus)
        {
            movingItem.Status = newStatus;
            movingItem.ChangedOnUtc = DateTime.UtcNow;
            lock (_movingItemRepository)
            {
                _movingItemRepository.Update(movingItem);
            }
        }

        public MovingItem GetIdle(MovingItemTypes? types)
        {
            lock (_movingItemRepository)
            {
                var query = _movingItemRepository.Table;
                if (types != null)
                    query = query.Where(x => x.Types == types);
                var movingItem =
                    query.FirstOrDefault(mi1 =>
                        (mi1.Status == MovingItemStatus.Pending
                        && !query.Any(mi2 => mi2.Equals(mi1) && (mi2 != mi1)
                        && mi2.Status != MovingItemStatus.Succeed)))
                    ?? query.FirstOrDefault(mi1 => (mi1.Status == MovingItemStatus.Failed)
                        && !query.Any(mi2 => mi2.Equals(mi1) && (mi2 != mi1)
                        && mi2.Status != MovingItemStatus.Succeed));

                if (movingItem != null)
                {
                    UpdateStatus(movingItem, MovingItemStatus.Processing);
                }

                return movingItem;
            }
        }

        public List<MovingItem> GetLogItems(List<MovingItemStatus> status = null, List<MovingItemTypes> types = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _movingItemRepository.Table;

            if (status != null)
            {
                query = query.Where(mi => status.Contains(mi.Status));
            }
            if (types != null)
            {
                query = query.Where(mi => types.Contains(mi.Types));
            }
            query = query.OrderBy(e => e.CreatedOnUtc)
                .ThenBy(e => e.Id);
            var movingItems = new PagedList<MovingItem>(query, pageIndex, pageSize);

            return movingItems;
        }

        public void ClearSucceedRecords(MovingItemTypes? types = null)
        {
            lock (_movingItemRepository)
            {
                var query = _movingItemRepository.Table.Where(mi => mi.Status == MovingItemStatus.Succeed);
                if (types != null)
                    query = query.Where(mi => mi.Types == types);
                _movingItemRepository.Delete(query);
            }
        }

        public bool AllItemsMoved(MovingItemTypes? types = null)
        {
            lock (_movingItemRepository)
            {
                var query = _movingItemRepository.Table;
                if (types != null)
                    query = query.Where(mi => mi.Types == types);
                var result = !query.Any(mi => mi.Status != MovingItemStatus.Succeed);
                return result;
            }
        }

        public void ResetAbortedStatuses()
        {
            var movingItems = _movingItemRepository.Table.Where(mi => mi.Status == MovingItemStatus.Processing);
            foreach (var movingItem in movingItems)
            {
                movingItem.Status = MovingItemStatus.Pending;
                movingItem.CreatedOnUtc = DateTime.UtcNow;
            }
            lock (_movingItemRepository)
            {
                _movingItemRepository.Update(movingItems);
            }
        }

        public void StartWorklflow()
        {
            string taskType = CloudStoragePlugin.SCHEDULE_TASKS_TYPE;

            var scheduleTask = _scheduleTaskRepository.Table.FirstOrDefault(t => t.Type == taskType);

            if (scheduleTask != null)
            {
                scheduleTask.Enabled = true;
                var task = new Task(scheduleTask) { Enabled = true };
                task.Execute(true, false);
            }
        }

        #endregion

        public void Save(List<string> files, MovingItemTypes types, string oldProviderSystemName)
        {
            lock (_movingItemRepository)
            {
                var itemsToUpdate = _movingItemRepository.Table.Where(x => files.Contains(x.FilePath) && x.Types == types);
                foreach (var item in itemsToUpdate)
                {
                    item.OldProviderSystemName = oldProviderSystemName;
                    item.ChangedOnUtc = DateTime.UtcNow;
                    item.Status = MovingItemStatus.Pending;
                }
                _movingItemRepository.Update(itemsToUpdate);
                var updatedIds = itemsToUpdate.Select(x => x.FilePath);
                var itemIds = files.Where(x => !updatedIds.Contains(x));
                var items = itemIds.Select(x => new MovingItem
                {
                    FilePath = x,
                    Types = types,
                    OldProviderSystemName = oldProviderSystemName,
                    Status = MovingItemStatus.Pending,
                    CreatedOnUtc = DateTime.UtcNow,
                    ChangedOnUtc = DateTime.UtcNow,
                });
                _movingItemRepository.Insert(items);
            }
        }
    }
}
