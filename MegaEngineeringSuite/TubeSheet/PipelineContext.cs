using System;
using System.Threading;
using MegaEngineeringSuite.Infrastructure.Cad;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.TubeSheet
{
    public class PipelineContext
    {
        public TubeSheetData Data { get; set; } = new TubeSheetData();
        public DrawingInformation Info { get; set; } = new DrawingInformation();
        public string WorkingDrawingPath { get; set; } = string.Empty;
        public ICadAdapter CadAdapter { get; set; }
        public ISynchronizationProvider SyncProvider { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public PlaceholderIndex PlaceholderIndex { get; set; } = new PlaceholderIndex();
        
        public bool WorkingCopyPrepared { get; set; } = false;
        public bool GeometryAlreadyGenerated { get; set; } = false;

        // These will be defined in later stages
        public object? GeometryValidationResult { get; set; }
        public object? AnnotationValidationResult { get; set; }
        
        // Diagnostic properties
        public string CurrentPhase { get; set; } = "Initializing";
        public Exception? LastException { get; set; }
        
        public DateTime GenerationStartTime { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string OutputFolder { get; set; } = string.Empty;

        public PipelineContext(ICadAdapter cadAdapter, ISynchronizationProvider syncProvider, CancellationToken cancellationToken)
        {
            CadAdapter = cadAdapter;
            SyncProvider = syncProvider;
            CancellationToken = cancellationToken;
            GenerationStartTime = DateTime.Now;
        }
    }
}
