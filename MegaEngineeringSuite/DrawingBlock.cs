#pragma warning disable CS8618
using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public enum SheetZone
    {
        TopEngineeringLeft,
        TopEngineeringRight,
        MiddleEngineeringLeft,
        MiddleEngineeringRight,
        BottomEngineeringLeft,
        BottomEngineeringMid,
        BottomEngineeringRight,
        DocColumnTop,
        DocColumnMid,
        DocColumnBottom
    }

    public class DrawingBlock
    {
        public string Name { get; set; }
        public List<ICadEntity> Entities { get; set; } = new List<ICadEntity>();
        public DrawingBounds Bounds { get; set; }
        public float PreferredScale { get; set; } = 1.0f;
        public bool AllowScaling { get; set; } = true;
        public bool AllowTranslation { get; set; } = true;
        public SheetZone Zone { get; set; }
        public int Priority { get; set; }
    }
}
