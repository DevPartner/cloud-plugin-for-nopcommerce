using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Nop.Core;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Services
{
    public class MovingItemService
    {
        #region Fields
        private readonly IRepository<MovingItem> _movingItemRepository;
        #endregion

        #region Constr

        public MovingItemService(IRepository<MovingItem> movingItemRepository)
        {
            _movingItemRepository = movingItemRepository;
        }

        #endregion

        #region Utils

        #endregion

        #region Methods

        public async Task SaveAsync(IEnumerable<int> ids, MovingItemTypes types, string oldProviderSystemName)
        {
            var itemsToUpdate = await _movingItemRepository.Table.Where(x => ids.Contains(x.EntityId.Value) && x.TypeId == (int)types).ToListAsync();
            foreach (var item in itemsToUpdate)
            {
                item.OldProviderSystemName = oldProviderSystemName;
                item.UpdatedOnUtc = DateTime.UtcNow;
                item.StatusId = (int)MovingItemStatus.Pending;
            }
            await _movingItemRepository.UpdateAsync(itemsToUpdate);
            var updatedIds = itemsToUpdate.Select(x => x.EntityId);
            var itemIds = ids.Where(x => !updatedIds.Contains(x));
            var items = await itemIds.Select(x => new MovingItem
            {
                EntityId = x,
                TypeId = (int)types,
                OldProviderSystemName = oldProviderSystemName,
                StatusId = (int)MovingItemStatus.Pending,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            }).ToListAsync();
            await _movingItemRepository.InsertAsync(items);
        }

        public async Task UpdateStatusAsync(MovingItem movingItem, MovingItemStatus newStatus)
        {
            movingItem.StatusId = (int)newStatus;
            movingItem.UpdatedOnUtc = DateTime.UtcNow;
            await _movingItemRepository.UpdateAsync(movingItem);
        }

        public async Task<MovingItem> GetIdle(MovingItemTypes? types)
        {
            var query = _movingItemRepository.Table;
            if (types != null)
                query = query.Where(x => x.TypeId == (int)types.Value);
            var movingItem =
                query.FirstOrDefault(mi1 =>
                    (mi1.StatusId == (int)MovingItemStatus.Pending
                    && !query.Any(mi2 => mi2.Equals(mi1) && (mi2 != mi1)
                    && mi2.StatusId != (int)MovingItemStatus.Succeed)))
                ?? query.FirstOrDefault(mi1 => (mi1.StatusId == (int)MovingItemStatus.Failed)
                    && !query.Any(mi2 => mi2.Equals(mi1) && (mi2 != mi1)
                    && mi2.StatusId != (int)MovingItemStatus.Succeed));
            if (movingItem != null)
                await UpdateStatusAsync(movingItem, MovingItemStatus.Processing);

            return movingItem;
        }
        /// <summary>
        ///? deprecated
        /// </summary>
        /// <param name="status"></param>
        /// <param name="types"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<IPagedList<MovingItem>> GetLogItemsAsync(List<MovingItemStatus> status = null, List<MovingItemTypes> types = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _movingItemRepository.Table;

            if (status != null)
            {
                query = query.Where(mi => status.Contains((MovingItemStatus)mi.StatusId));
            }
            if (types != null)
            {
                query = query.Where(mi => types.Contains((MovingItemTypes)mi.TypeId));
            }
            query = query.OrderBy(e => e.CreatedOnUtc)
                .ThenBy(e => e.Id);

            var movingItems = await query.ToPagedListAsync(pageIndex, pageSize);

            return movingItems;
        }

        public async Task ClearSucceedRecordsAsync(MovingItemTypes? types = null)
        {
            await _movingItemRepository.DeleteAsync(mi => mi.StatusId == (int)MovingItemStatus.Succeed
                    && types != null && mi.TypeId == (int)types.Value);
        }

        public bool AllItemsMoved(MovingItemTypes? types = null)
        {
            lock (_movingItemRepository)
            {
                var query = _movingItemRepository.Table;
                if (types != null)
                    query = query.Where(mi => mi.TypeId == (int)types.Value);
                var result = !query.Any(mi => mi.StatusId != (int)MovingItemStatus.Succeed);
                return result;
            }
        }

        public async Task ResetAbortedStatusesAsync()
        {
            var movingItems = _movingItemRepository.Table.Where(mi => mi.StatusId == (int)MovingItemStatus.Processing).ToList();
            movingItems.ForEach(x =>
            {
                x.StatusId = (int)MovingItemStatus.Pending;
                x.CreatedOnUtc = DateTime.UtcNow;
            });

            await _movingItemRepository.UpdateAsync(movingItems);
        }

        public async Task SaveAsync(List<string> files, MovingItemTypes types, string oldProviderSystemName)
        {
            var itemsToUpdate = _movingItemRepository.Table.Where(x => files.Contains(x.FilePath) && x.TypeId == (int)types).ToList();
            foreach (var item in itemsToUpdate)
            {
                item.OldProviderSystemName = oldProviderSystemName;
                item.UpdatedOnUtc = DateTime.UtcNow;
                item.StatusId = (int)MovingItemStatus.Pending;
            }
            await _movingItemRepository.UpdateAsync(itemsToUpdate);
            var updatedIds = itemsToUpdate.Select(x => x.FilePath);
            var itemIds = files.Where(x => !updatedIds.Contains(x));
            var items = itemIds.Select(x => new MovingItem
            {
                FilePath = x,
                TypeId = (int)types,
                OldProviderSystemName = oldProviderSystemName,
                StatusId = (int)MovingItemStatus.Pending,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            }).ToList();
            await _movingItemRepository.InsertAsync(items);
        }
        #endregion

    }
}
