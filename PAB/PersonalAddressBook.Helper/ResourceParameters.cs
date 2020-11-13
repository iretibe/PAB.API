namespace PAB.Helper
{
    public class ResourceParameters
    {
        const int MaxPageSize = 20;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string SearchQuery { get; set; }

        //public string OrderBy { get; set; } = "szDescription";

        //public string Fields { get; set; }
    }
}
