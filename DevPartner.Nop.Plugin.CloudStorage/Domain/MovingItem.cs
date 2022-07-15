using System;
using System.Globalization;
using DevPartner.Nop.Plugin.CloudStorage.Models;
using Nop.Core;

namespace DevPartner.Nop.Plugin.CloudStorage.Domain
{
    public class MovingItem : BaseEntity 
    {
        /// <summary>
        /// Gets or sets the Old Storage Name
        /// </summary>
        public MovingItemTypes Types { get; set; }
        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        public MovingItemStatus Status { get; set; }
        /// <summary>
        /// Gets or sets the Created Date
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        /// <summary>
        /// Gets or sets the Change Date
        /// </summary>
        public DateTime ChangedOnUtc { get; set; }

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
        }

    }
}
