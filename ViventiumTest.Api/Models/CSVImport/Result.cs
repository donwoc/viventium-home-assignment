namespace ViventiumTest.Api.Models.CSVImport
{

    public class Result
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
