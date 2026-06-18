-- =====================================================
-- ROLLBACK: Writing Coach Schema
-- Database: PostgreSQL (Supabase)
-- Created: 2026-06-18
-- =====================================================
-- This script removes all tables and indexes added by
-- the Writing Coach feature migration.
--
-- WARNING: This will delete ALL Writing Coach data including:
-- - WritingSkillSnapshots
-- - Corrections
-- - UserBadges
-- - Badges (if they were added by this migration only)
-- =====================================================

BEGIN;

-- Drop indexes first (if they exist)
DROP INDEX IF EXISTS "IX_WritingSkillSnapshots_UserId_CompletedAt";
DROP INDEX IF EXISTS "IX_WritingSkillSnapshots_MissionId";
DROP INDEX IF EXISTS "IX_WritingSkillSnapshots_UserId";
DROP INDEX IF EXISTS "IX_Corrections_DialogueId";
DROP INDEX IF EXISTS "IX_UserBadges_UserId_BadgeId";
DROP INDEX IF EXISTS "IX_UserBadges_UserId";

-- Drop tables (with cascade to handle foreign keys)
DROP TABLE IF EXISTS "WritingSkillSnapshots" CASCADE;
DROP TABLE IF EXISTS "Corrections" CASCADE;
DROP TABLE IF EXISTS "UserBadges" CASCADE;
DROP TABLE IF EXISTS "Badges" CASCADE;

-- Remove added columns from Dialogues (if they exist)
-- Only run these if you're sure these columns were added by the Writing Coach migration
ALTER TABLE "Dialogues" DROP COLUMN IF EXISTS "GrammarScore";
ALTER TABLE "Dialogues" DROP COLUMN IF EXISTS "VocabularyScore";
ALTER TABLE "Dialogues" DROP COLUMN IF EXISTS "ToneScore";
ALTER TABLE "Dialogues" DROP COLUMN IF EXISTS "NaturalnessScore";
ALTER TABLE "Dialogues" DROP COLUMN IF EXISTS "ClarityScore";
ALTER TABLE "Dialogues" DROP COLUMN IF EXISTS "StructureScore";

COMMIT;

-- =====================================================
-- POST-ROLLBACK VERIFICATION
-- =====================================================
-- Verify tables are dropped:
-- \d "WritingSkillSnapshots"  -- should not exist
-- \d "Corrections"            -- should not exist
-- \d "Badges"                 -- should not exist
-- \d "UserBadges"             -- should not exist
--
-- Verify columns removed from Dialogues:
-- \d "Dialogues"
