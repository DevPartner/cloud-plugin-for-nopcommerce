using System;
using Nop.Core;

namespace DevPartner.Nop.Plugin.CloudStorage.Domain
{
    public class MovingItem : BaseEntity 
    {
        #region Properties
        /// <summary>
        /// Gets or sets the Old Storage Name
        /// </summary>
        public int TypeId { get; set; }
        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        public int StatusId { get; set; }
        /// <summary>
        /// Gets or sets the Created Date
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        /// <summary>
        /// Gets or sets the Change Date
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the EntiyId
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the FilePath
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the OldProviderSystemName
        /// </summary>
        public string OldProviderSystemName { get; set; }
        #endregion

        #region Custom properties
        /// <summary>
        /// Gets or sets the Old Storage Name
        /// </summary>
        public MovingItemTypes Types
        {
            get => (MovingItemTypes)TypeId;
            set => TypeId = (int)value;
        }

        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        public MovingItemStatus Status
        {
            get => (MovingItemStatus)StatusId;
            set => StatusId = (int)value;
        }

        #endregion
        /*
        public MovingItemModel ToModel()
        {
            return new MovingItemModel()
            {
                Id = Id,
                Item = $"Entity Id: {EntityId}",
                StoreType = Types.ToString(),
                Status = Status.ToString(),
                CreatedOnUtc = CreatedOnUtc.ToString(CultureInfo.InvariantCulture),
                ChangedOnUtc = CreatedOnUtc.ToString(CultureInfo.InvariantCulture),
            };
        }*/

    }
}
