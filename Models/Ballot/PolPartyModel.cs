namespace SEMS.Models.Ballot
{
    public class PolPartyModel
    {
        public string partyName { get; set; }
        public string partyNameV1 { get; set; }
        public string shortName { get; set;}
        public string shortNameV1 { get; set; }
        public int sid { get; set; }
        public byte[] symbol { get; set; }
        public string formStatus { get; set; } = "new";
    }
}
