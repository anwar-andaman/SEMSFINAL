using System.ComponentModel.DataAnnotations;
namespace SEMS.Models
{
    public class RevisionModel
    {
        public int revisionNo { get; set; }
        public int revisionYear { get; set; }
        public DateTime revisionDate { get; set; }
        public DateTime ageAsOn { get; set; }
        public string supplementName { get; set; }
        public DateTime publicationDate { get; set; }
        public string revisionType { get; set; }
        public string erollHeader { get; set; }
        public string publicationPlace { get; set; }
        public DateTime qualifyingDate { get; set; }
        public string supplementDescription { get; set; }
        public string supplementType { get; set; }
        public int isActive { get; set; }
        public RevisionModel()
        {
            this.revisionYear = DateTime.Now.Year;
            this.revisionNo = 0;
            this.revisionDate = DateTime.Now;
            this.ageAsOn= new DateTime(DateTime.Now.Year, 1,1);
            this.qualifyingDate = new DateTime(DateTime.Now.Year, 1, 1);
            this.publicationDate = DateTime.Now.AddMonths(1);
        }
    }
}
