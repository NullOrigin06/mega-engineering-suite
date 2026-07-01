using System;

namespace MegaEngineeringSuite
{
    public class DrawingInformation
    {
        public string CustomerName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ProjectNo { get; set; } = string.Empty;
        public string DrawingNo { get; set; } = string.Empty;
        public string Revision { get; set; } = "0";
        public string PreparedBy { get; set; } = "NSS";
        public string CheckedBy { get; set; } = "ASK";
        public string ApprovedBy { get; set; } = "ASK";
        public DateTime Date { get; set; } = DateTime.Today;
    }
}
