using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using MegaEngineeringSuite.Infrastructure.Cad;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.TubeSheet
{
    public class TubeSheetDrawingPipeline
    {
        private readonly ICadAdapter _cadAdapter;
        private readonly IGeometryGenerator _geometryGenerator;
        private readonly IAnnotationEngine _annotationEngine;
        private readonly ISynchronizationProvider _syncProvider;

        public TubeSheetDrawingPipeline(ICadAdapter cadAdapter, IGeometryGenerator geometryGenerator, IAnnotationEngine annotationEngine, ISynchronizationProvider syncProvider)
        {
            _cadAdapter = cadAdapter;
            _geometryGenerator = geometryGenerator;
            _annotationEngine = annotationEngine;
            _syncProvider = syncProvider;
        }

        public async Task<string> ExecuteAsync(PipelineContext context)
        {
            RuntimeTraceLogger.Log("## Pipeline Execution Timeline");
            try
            {
                await ExecutePhaseAsync(context, "ValidationPhase", () => { ValidatePhase(context); return Task.CompletedTask; });
                
                if (!context.WorkingCopyPrepared)
                {
                    await ExecutePhaseAsync(context, "WorkingCopyPhase", () => {
                        context.WorkingDrawingPath = WorkingCopyPhase(context);
                        context.CadAdapter.OpenDrawing(context.WorkingDrawingPath);
                        return Task.CompletedTask;
                    });
                }

                if (!context.GeometryAlreadyGenerated)
                {
                    await ExecutePhaseAsync(context, "GeometryGenerationPhase", () => GeometryGenerationPhaseAsync(context));
                    await ExecutePhaseAsync(context, "SynchronizationPhase", () => SynchronizationPhaseAsync(context));
                }
                
                await ExecutePhaseAsync(context, "DrawingNormalizationPhase", () => { DrawingNormalizationPhase(context); return Task.CompletedTask; });
                
                await ExecutePhaseAsync(context, "AnnotationPhase", () => { AnnotationPhase(context); return Task.CompletedTask; });
                
                await ExecutePhaseAsync(context, "TitleBlockPhase", () => { TitleBlockPhase(context); return Task.CompletedTask; });
                
                await ExecutePhaseAsync(context, "FinalizePhase", () => { FinalizePhase(context); return Task.CompletedTask; });

                return context.WorkingDrawingPath;
            }
            catch (Exception ex)
            {
                context.LastException = ex;
                ExecuteRollback(context, ex);
                throw;
            }
        }

        private async Task ExecutePhaseAsync(PipelineContext context, string phaseName, Func<Task> phaseAction)
        {
            context.CurrentPhase = phaseName;
            RuntimeTraceLogger.LogPhaseStart(phaseName);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await phaseAction();
                sw.Stop();
                RuntimeTraceLogger.LogPhaseEnd(phaseName, "PASS", sw.ElapsedMilliseconds);
            }
            catch (Exception)
            {
                sw.Stop();
                RuntimeTraceLogger.LogPhaseEnd(phaseName, "FAIL", sw.ElapsedMilliseconds);
                throw;
            }
        }

        private void ValidatePhase(PipelineContext context)
        {
            SimpleLogger.Log("TubeSheetPipeline", "Phase 0: Validating Inputs & Folders...");
        }

        private string WorkingCopyPhase(PipelineContext context)
        {
            SimpleLogger.Log("TubeSheetPipeline", "Phase 1: Creating Working Copy...");
            return "dummy_path.dwg";
        }

        private async Task GeometryGenerationPhaseAsync(PipelineContext context)
        {
            SimpleLogger.Log("TubeSheetPipeline", "Phase 2: Generating Geometry (LISP)...");
            context.SyncProvider.InitializeSynchronization();
            await _geometryGenerator.GenerateGeometryAsync(context.Data, context.CancellationToken);
        }

        private async Task SynchronizationPhaseAsync(PipelineContext context)
        {
            SimpleLogger.Log("TubeSheetPipeline", "Phase 3: Synchronizing with AutoLISP via SyncProvider...");
            
            bool success = await context.SyncProvider.WaitForCompletionAsync(TimeSpan.FromSeconds(60), context.CancellationToken);
            
            if (!success)
            {
                throw new Exception("Pipeline synchronization failed due to timeout or LISP error.");
            }
            
            SimpleLogger.Log("TubeSheetPipeline", "Synchronization complete.");
        }

        private void DrawingNormalizationPhase(PipelineContext context)
        {
            SimpleLogger.Log("TubeSheetPipeline", "Phase 3.5: Drawing Normalization...");
            context.CadAdapter.SendCommand("_REGEN ");
            context.CadAdapter.SendCommand("_ZOOM _E ");
        }

        private void AnnotationPhase(PipelineContext context)
        {
            SimpleLogger.Log("TubeSheetPipeline", "Phase 4: Annotations (META_ANNOTATIONS)...");
            
            // Phase A: Discovery
            var discoveryEngine = new AnnotationDiscoveryEngine();
            discoveryEngine.DiscoverAnnotations(context);

            var schema = new TubeSheetPlaceholderSchema();
            var activeProfile = MigrationProfile.Stage10_BOM;
            var activeDefinitions = schema.GetActiveProfileDefinitions(activeProfile).ToList();
            var activeNames = new HashSet<string>(activeDefinitions.Select(d => d.PlaceholderName), StringComparer.OrdinalIgnoreCase);

            // Filter context.PlaceholderIndex to active placeholders only
            var activeIndex = new PlaceholderIndex();
            foreach (var p in context.PlaceholderIndex.Enumerate())
            {
                if (activeNames.Contains(p.PlaceholderName))
                {
                    activeIndex.Add(p);
                }
            }
            SimpleLogger.Log("TubeSheetPipeline", $"Filtered {context.PlaceholderIndex.Count} discovered down to {activeIndex.Count} active placeholders.");

            // Phase B: Validation
            var structureValidator = new PlaceholderStructureValidator();
            var structureReport = structureValidator.ValidateStructure(activeIndex, activeDefinitions);
            
            if (!structureReport.Success)
            {
                throw new Exception($"Structure Validation Failed: {string.Join(", ", structureReport.Errors)}");
            }

            var schemaValidator = new PlaceholderSchemaValidator();
            var schemaReport = schemaValidator.ValidateSchema(activeIndex, activeDefinitions);

            if (!schemaReport.Success)
            {
                throw new Exception($"Schema Validation Failed: {string.Join(", ", schemaReport.Errors)}");
            }

            context.AnnotationValidationResult = schemaReport;
            SimpleLogger.Log("TubeSheetPipeline", "Phase 4 Validation Passed.");

            // Phase C: Resolution & Replacement
            // --- PHASE 3: Replacement Planning ---
            var data = context.Data;
            var info = context.Info;
            var provider = new TubeSheetPlaceholderProvider(data, info);
            
            // Generate profile-specific dictionary
            var replacementDictionary = provider.GetValues(activeProfile);
            
            var resolutionEngine = new PlaceholderResolutionEngine();
            var replacementPlan = resolutionEngine.Resolve(activeIndex, activeDefinitions, replacementDictionary);

            var planValidator = new ReplacementPlanValidator();
            var planReport = planValidator.ValidatePlan(replacementPlan, activeDefinitions);

            if (!planReport.Success)
            {
                // Log replacement plan validation failure
                throw new Exception($"Replacement Plan Validation Failed: {string.Join(", ", planReport.Errors)}");
            }
            

            var replacementEngine = new ReplacementEngine();
            replacementEngine.ExecutePlan(replacementPlan, context);

            var verificationEngine = new ReplacementVerificationEngine();
            verificationEngine.VerifyReplacements(replacementPlan, context);

            SimpleLogger.Log("TubeSheetPipeline", "Phase 4 Replacement & Verification Complete.");
        }

        private void TitleBlockPhase(PipelineContext context)
        {
            SimpleLogger.Log("TubeSheetPipeline", "Phase 6: Replacing Title Block...");
            var titleBlockService = new TitleBlockService();
            titleBlockService.Execute(context);
        }

        private void FinalizePhase(PipelineContext context)
        {
            SimpleLogger.Log("TubeSheetPipeline", "Phase 7: Finalizing & Saving...");

            context.CadAdapter.Save();
            

            switch (context.ExecutionMode)
            {
                case PipelineExecutionMode.Automation:
                    context.CadAdapter.CloseDrawing();
                    break;
                case PipelineExecutionMode.Interactive:
                    context.CadAdapter.ReleaseDocumentReference();
                    SimpleLogger.Log("TubeSheetPipeline", "Interactive Mode: Leaving drawing open for user review.");
                    break;
            }
        }


        private void ExecuteRollback(PipelineContext context, Exception ex)
        {
            SimpleLogger.Log("TubeSheetPipeline", $"ROLLBACK TRIGGERED. Exception: {ex.Message}");
            // We intentionally do NOT release the CAD Application here. Orchestrator handles drawing close.
        }
    }
}
