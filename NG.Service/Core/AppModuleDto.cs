using System;
using NG.Service.Core;

namespace NG.Service.Core
{
    public class AppModuleDto : BaseDto
    {
        public Guid Id { get; set; }
        public string MenuText { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }

    }
}