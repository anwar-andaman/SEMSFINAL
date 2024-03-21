﻿namespace SEMS.Models.Ballot
{
    public class CandidateModel
    {
        public int cid { get; set; }
        public string cName { get; set; }
        public string cNameV1 { get; set; }
        public string pstCode { get; set; }
        public string constCode { get; set; }
        public int slNo { get; set; }
        public string panchayat { get; set; }
        public char gender { get; set; }
        public string formStatus { get; set; }
        public bool independent { get; set; }
        public string pacode { get; set; }  
        public string postCause { get; set;}
        public string sid { get; set; }
    }
}
