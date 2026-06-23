using System.Collections.Generic;
using System.Drawing;

namespace MegaEngineeringSuite
{
    public interface ICadView
    {
        IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin);
    }
}
