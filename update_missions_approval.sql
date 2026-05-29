-- =============================================================================
-- THE UNRAVELLER - DATABASE SCHEMA UPDATE FOR MISSION APPROVALS (SUPABASE)
-- =============================================================================

-- Add ApprovalStatus column: 0 = Approved, 1 = Pending, 2 = Rejected
ALTER TABLE "Missions" 
ADD COLUMN IF NOT EXISTS "ApprovalStatus" INTEGER DEFAULT 0;

-- Add RejectionReason column to hold admin feedback
ALTER TABLE "Missions" 
ADD COLUMN IF NOT EXISTS "RejectionReason" TEXT DEFAULT NULL;

-- Add CreatedByUserId column to track which Moderator created it
ALTER TABLE "Missions" 
ADD COLUMN IF NOT EXISTS "CreatedByUserId" INTEGER REFERENCES "Users"("Id") ON DELETE SET NULL;
