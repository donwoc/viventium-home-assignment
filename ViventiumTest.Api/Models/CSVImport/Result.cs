namespace ViventiumTest.Api.Models.CSVImport
{

    public class Result
    {
        public bool Success { get; set; }
        public List<Company> Companies { get; set; } = new List<Company>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
