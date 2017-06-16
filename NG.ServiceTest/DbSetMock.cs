using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace NG.ServiceTest
{
    public static class DbSetMock
    {
        public static Mock<DbSet<T>> Create<T>(params T[] elements) where T : class
        {
            return new List<T>(elements).AsDbSetMock();
        }
    }
}