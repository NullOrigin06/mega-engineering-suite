using System;
using System.Collections.Generic;
using MegaEngineeringSuite;

namespace MegaEngineeringSuite.TubeSheet
{
    public class TitleBlockMappingProfile
    {
        public Dictionary<string, Func<DrawingInformation, string>> TagMappings { get; } = new Dictionary<string, Func<DrawingInformation, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "TITLE",      info => info.Title },
            { "CUSTOMER",   info => info.CustomerName },
            { "PROJECTNO",  info => info.ProjectNo },
            { "DWG",        info => info.DrawingNo },
            { "REV",        info => info.Revision },
            { "DRAWN",      info => info.PreparedBy },
            { "CHECKED",    info => info.CheckedBy },
            { "APPROVED",   info => info.ApprovedBy },
            { "DATE",       info => info.Date.ToString("dd-MM-yyyy") }
        };
    }
}

