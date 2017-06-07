using System;

namespace NG.Domain.Common
{
    public class BaseEntity : IEntity
    {
        public BaseEntity()
        {
            this.CreatedOn = DateTime.Now;
            this.IsDelete = false;
            this.IsAssigned = false;
        }
        public DateTime CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDelete { get; set; }
        public bool IsAssigned { get; set; }
    }
}