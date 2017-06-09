using NG.Common.Helpers;

namespace NG.Common.Helpers
{
    public class ExportResourceParameters : BaseResourceParameters
    {
        public override int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value < 0) ? 0 : value;
            }
        }
    }
}