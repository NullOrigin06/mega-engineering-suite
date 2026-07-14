using System.Threading;
using System.Threading.Tasks;

namespace MegaEngineeringSuite.TubeSheet
{
    public interface IGeometryGenerator
    {
        Task GenerateGeometryAsync(TubeSheetData data, CancellationToken cancellationToken);
    }
}
