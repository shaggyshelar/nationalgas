using NG.Common.Helpers;

namespace NG.Application.Core
{
    public class AppRolesResourceParameters : BaseResourceParameters
    {
        public override string OrderBy { get; set; } = "Name";
        private new int _pageSize = 0;
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