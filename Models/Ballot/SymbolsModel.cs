namespace SEMS.Models.Ballot
{
    public class SymbolsModel
    {
        public string sid { get; set; }
        public byte[] symbol { get; set; }
        public string symbolName { get; set; }
       
        public IFormFile symbolfile { get; set; }
        public IFormFile symbolfile1 { get; set; }
        public string formStatus { get; set; } = "new";

    }
}
