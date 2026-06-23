using System;

namespace loginpage1
{
    public static class DraftingScaleManager
    {
        // Paper Space Target Sizes (SheetLayoutEngine will convert these into model-space heights)
        // Values as per Phase 5F hierarchy requirements

        public static float GetPaperSpaceMainTitleHeight() 
        { 
            return 7.0f; // Reduced from 10.0f
        }

        public static float GetPaperSpaceDetailTitleHeight() 
        { 
            return 5.0f; // Reduced from 8.0f
        }

        public static float GetPaperSpaceDimensionHeight() 
        { 
            return 5.5f; // 55% 
        }

        public static float GetPaperSpaceStandardNotesHeight() 
        { 
            return 3.5f; // Standardized with RowCountHeight 
        }

        public static float GetPaperSpaceRowCountHeight() 
        { 
            return 3.5f; // 35% 
        }

        // Keep table cell and title block fields stable
        public static float GetPaperSpaceSpecHeaderHeight() { return 4.0f; }
        public static float GetPaperSpaceTableCellHeight() { return 3.0f; }
        public static float GetPaperSpaceTitleBlockFieldHeight() { return 3.0f; }
        
        // These are kept for backward compatibility if any view still calls them
        public static float GetMainTitleTextHeight(float outerDiameter) { return GetPaperSpaceMainTitleHeight(); }
        public static float GetDetailTitleTextHeight(float outerDiameter) { return GetPaperSpaceDetailTitleHeight(); }
        public static float GetStandardDraftingTextHeight(float outerDiameter) { return GetPaperSpaceStandardNotesHeight(); }
        public static float GetPaperSpaceTextHeight() { return GetPaperSpaceStandardNotesHeight(); }
    }
}
