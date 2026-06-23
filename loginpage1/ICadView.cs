using System.Collections.Generic;
using System.Drawing;

namespace loginpage1
{
    public interface ICadView
    {
        IEnumerable<ICadEntity> Render(GeometryModel geometry, EngineeringDataModel data, PointF origin);
    }
}
