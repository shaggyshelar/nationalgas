namespace NG.Application
{
    public static class Permissions
    {
        #region Department
        public const string DepartmentRead = "DP" + "." + "R";
        public const string DepartmentCreate = "DP" + "." + "C";
        public const string DepartmentUpdate = "DP" + "." + "U";
        public const string DepartmentDelete = "DP" + "." + "D";
        #endregion Department

        #region Customer
        public const string CustomerRead = "CT" + "." + "R";
        public const string CustomerCreate = "CT" + "." + "C";
        public const string CustomerUpdate = "CT" + "." + "U";
        public const string CustomerDelete = "CT" + "." + "D";
        #endregion Customer

        #region Dashboard
        public const string DashboardDelete = "DB" + "." + "D";
        public const string DashboardRead = "DB" + "." + "R";
        public const string DashboardCreate = "DB" + "." + "C";
        public const string DashboardUpdate = "DB" + "." + "U";
        #endregion Dashboard
    }
}