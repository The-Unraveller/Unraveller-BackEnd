-- Migration: Create Feedbacks table in Supabase PostgreSQL
CREATE TABLE IF NOT EXISTS "Feedbacks" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INT NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Rating" INT NOT NULL DEFAULT 5,
    "Category" VARCHAR(255) NOT NULL DEFAULT 'Trải nghiệm UI/UX',
    "Comment" TEXT NOT NULL DEFAULT '',
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "IX_Feedbacks_UserId" ON "Feedbacks" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_Feedbacks_CreatedAt" ON "Feedbacks" ("CreatedAt");
