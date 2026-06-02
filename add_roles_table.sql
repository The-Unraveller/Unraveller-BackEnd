-- =============================================================================
-- THE UNRAVELLER - ADD ROLES LOOKUP TABLE MIGRATION SCRIPT
-- =============================================================================

-- 1. Create Roles lookup table
CREATE TABLE IF NOT EXISTS "Roles" (
    "Id" INTEGER PRIMARY KEY,
    "RoleName" VARCHAR(50) NOT NULL UNIQUE
);

-- 2. Seed default roles based on UserRole enum (0 = User, 1 = Moderator, 2 = Admin)
INSERT INTO "Roles" ("Id", "RoleName") VALUES
(0, 'User'),
(1, 'Moderator'),
(2, 'Admin')
ON CONFLICT ("Id") DO NOTHING;

-- 3. Add Role column to Users table if it does not exist
-- Default role is set to 0 (User)
ALTER TABLE "Users" 
ADD COLUMN IF NOT EXISTS "Role" INTEGER DEFAULT 0 REFERENCES "Roles"("Id");

-- 4. Map existing Admin user (if present) to Admin role (2)
UPDATE "Users" 
SET "Role" = 2 
WHERE "Username" = 'Admin' OR "Email" = 'admin@example.com';
