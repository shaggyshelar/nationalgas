using System;

namespace NG.Domain.Common
{
    public interface IEntity
    {
        DateTime CreatedOn { get; set; }
        Guid? CreatedBy { get; set; }
        DateTime? UpdatedOn { get; set; }
        Guid? UpdatedBy { get; set; }
        bool IsDelete { get; set; }
    }
}