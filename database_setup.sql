-- =============================================================================
-- THE UNRAVELLER - DATABASE SCHEMA FOR SUPABASE (POSTGRESQL)
-- =============================================================================

-- 1. Table: Roles
CREATE TABLE IF NOT EXISTS "Roles" (
    "Id" INTEGER PRIMARY KEY,
    "RoleName" VARCHAR(50) NOT NULL UNIQUE
);

-- 2. Table: Users
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" TEXT NOT NULL,
    "Email" TEXT UNIQUE NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "Energy" INTEGER DEFAULT 100,
    "MaxEnergy" INTEGER DEFAULT 100,
    "LastEnergyRechargedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "StreakCount" INTEGER DEFAULT 0,
    "LastActiveDate" TIMESTAMP WITH TIME ZONE,
    "XpBalance" INTEGER DEFAULT 0,
    "IsPremium" BOOLEAN DEFAULT FALSE,
    "Role" INTEGER DEFAULT 0 REFERENCES "Roles"("Id")
);

-- 2. Table: Npcs
CREATE TABLE IF NOT EXISTS "Npcs" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Role" TEXT,
    "Personality" TEXT,
    "BasePrompt" TEXT
);

-- 3. Table: Missions
CREATE TABLE IF NOT EXISTS "Missions" (
    "Id" SERIAL PRIMARY KEY,
    "Title" TEXT NOT NULL,
    "Description" TEXT,
    "Difficulty" TEXT,
    "TargetSuspicion" INTEGER,
    "RewardXp" INTEGER,
    "GrammarTarget" TEXT
);

-- 4. Table: Dialogues
CREATE TABLE IF NOT EXISTS "Dialogues" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER REFERENCES "Users"("Id") ON DELETE CASCADE,
    "MissionId" INTEGER REFERENCES "Missions"("Id"),
    "Message" TEXT NOT NULL,
    "Sender" TEXT NOT NULL, -- 'Player' or 'NPC'
    "Timestamp" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "SuspicionDelta" INTEGER DEFAULT 0
);

-- 5. Table: UserProgress
CREATE TABLE IF NOT EXISTS "UserProgress" (
    "UserId" INTEGER REFERENCES "Users"("Id") ON DELETE CASCADE,
    "MissionId" INTEGER REFERENCES "Missions"("Id") ON DELETE CASCADE,
    "Status" TEXT DEFAULT 'InProgress', -- 'InProgress', 'Completed', 'Failed'
    "CurrentTurn" INTEGER DEFAULT 0,
    "CurrentSuspicion" INTEGER DEFAULT 0,
    "CompletedAt" TIMESTAMP WITH TIME ZONE,
    PRIMARY KEY ("UserId", "MissionId")
);

-- 6. Table: ShopItems
CREATE TABLE IF NOT EXISTS "ShopItems" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT,
    "Type" TEXT NOT NULL, -- 'InGameHint', 'BribeNpc', 'Cosmetic'
    "PriceXp" INTEGER NOT NULL,
    "Emoji" TEXT
);

-- 7. Table: UserInventory
CREATE TABLE IF NOT EXISTS "UserInventory" (
    "UserId" INTEGER REFERENCES "Users"("Id") ON DELETE CASCADE,
    "ItemId" INTEGER REFERENCES "ShopItems"("Id") ON DELETE CASCADE,
    "Quantity" INTEGER DEFAULT 0,
    PRIMARY KEY ("UserId", "ItemId")
);

-- 8. Table: Payments
CREATE TABLE IF NOT EXISTS "Payments" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER REFERENCES "Users"("Id") ON DELETE CASCADE,
    "PlanId" TEXT NOT NULL,
    "Amount" DECIMAL(18, 2) NOT NULL,
    "Status" TEXT NOT NULL, -- 'Pending', 'Completed', 'Failed'
    "OrderId" TEXT,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 9. Table: SubscriptionPlans
CREATE TABLE IF NOT EXISTS "SubscriptionPlans" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Tier" INTEGER NOT NULL,
    "Price" DECIMAL(18, 2) NOT NULL,
    "DurationDays" INTEGER NOT NULL,
    "Description" TEXT,
    "Features" TEXT[]
);

-- 10. Table: UserSubscriptions
CREATE TABLE IF NOT EXISTS "UserSubscriptions" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER REFERENCES "Users"("Id") ON DELETE CASCADE,
    "PlanId" INTEGER REFERENCES "SubscriptionPlans"("Id"),
    "StartDate" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "EndDate" TIMESTAMP WITH TIME ZONE,
    "IsActive" BOOLEAN DEFAULT TRUE,
    "TransactionId" TEXT
);

-- =============================================================================
-- SEED DATA (Dữ liệu mẫu ban đầu)
-- =============================================================================

INSERT INTO "Roles" ("Id", "RoleName") VALUES
(0, 'User'),
(1, 'Moderator'),
(2, 'Admin')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Users" ("Username", "Email", "PasswordHash", "IsPremium", "Role")
VALUES ('Admin', 'admin@example.com', 'admin123', true, 2)
ON CONFLICT ("Email") DO NOTHING;

INSERT INTO "Npcs" ("Name", "Role", "Personality", "BasePrompt")
VALUES ('Detective Lee', 'Police Officer', 'Strict and observant', 'You are Detective Lee. You are suspicious of the player...')
ON CONFLICT DO NOTHING;

INSERT INTO "Missions" ("Title", "Description", "Difficulty", "TargetSuspicion", "RewardXp")
VALUES ('The Secret Agent', 'Convince the guard you belong here', 'Easy', 50, 100)
ON CONFLICT DO NOTHING;

INSERT INTO "ShopItems" ("Name", "Description", "Type", "PriceXp", "Emoji")
VALUES ('Golden Tongue', 'Reduce suspicion by 20', 'BribeNpc', 500, '✨')
ON CONFLICT DO NOTHING;
