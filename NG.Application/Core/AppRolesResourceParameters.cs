using NG.Common.Helpers;

namespace NG.Application.Core
{
    public class AppRolesResourceParameters : BaseResourceParameters
    {
        public string OrderBy { get; set; } = "Name";
        private int _pageSize = 0;
        public override int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value;
            }
        }

    }
}