namespace Zbyrach.Api.Admin.Dto
{
    public class StatisticResponse
    {
        public long UsersCount { get; set; }
        public long ArticlesCount { get; set; }
        public long TagsCount { get; set; }
        public long DbTotalRowsCount { get; set; }
        public long DbTotalSizeInBytes { get; set; }
        public long PdfCashItemsCount { get; set; }
        public long PdfCashDataSize { get; set; }
    }
}