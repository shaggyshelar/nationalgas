using NG.Common.Enums;

namespace NG.Service.Core
{
    public class AddPermissionToRoleDto
    {
        public string AppModuleName { get; set; }

        public PermissionType PermissionType { get; set; }
    }
}