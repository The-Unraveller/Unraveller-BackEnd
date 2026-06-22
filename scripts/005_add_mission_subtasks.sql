-- =============================================================================
-- MIGRATION 005: Add MissionSubTasks and UserSubTaskProgress tables
-- Database: PostgreSQL (Supabase)
-- =============================================================================

-- Tạo bảng MissionSubTasks
CREATE TABLE IF NOT EXISTS "MissionSubTasks" (
    "Id"              SERIAL PRIMARY KEY,
    "MissionId"       INTEGER NOT NULL REFERENCES "Missions"("Id") ON DELETE CASCADE,
    "OrderIndex"      INTEGER NOT NULL DEFAULT 0,
    "Label"           TEXT NOT NULL,                     -- VD: "Hỏi chỗ ngồi"
    "LabelEn"         TEXT NOT NULL DEFAULT '',          -- VD: "Ask for a seat"
    "HintPhrase"      TEXT NOT NULL DEFAULT '',          -- Gợi ý câu tiếng Anh
    "TriggerKeywords" TEXT[] NOT NULL DEFAULT '{}',      -- Keywords để AI detect
    "IsOptional"      BOOLEAN NOT NULL DEFAULT FALSE,
    "XpBonus"         INTEGER NOT NULL DEFAULT 10,
    UNIQUE("MissionId", "OrderIndex")
);

CREATE INDEX IF NOT EXISTS idx_mission_subtasks_mission_id
    ON "MissionSubTasks"("MissionId");

-- Tạo bảng theo dõi completion của subtask theo user
CREATE TABLE IF NOT EXISTS "UserSubTaskProgress" (
    "Id"          SERIAL PRIMARY KEY,
    "UserId"      INTEGER NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "MissionId"   INTEGER NOT NULL REFERENCES "Missions"("Id") ON DELETE CASCADE,
    "SubTaskId"   INTEGER NOT NULL REFERENCES "MissionSubTasks"("Id") ON DELETE CASCADE,
    "CompletedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE("UserId", "SubTaskId")
);

CREATE INDEX IF NOT EXISTS idx_user_subtask_progress_user_mission
    ON "UserSubTaskProgress"("UserId", "MissionId");

-- =============================================================================
-- SEED DATA: Sub-tasks cho Mission 1 — Quán Cà phê
-- =============================================================================
INSERT INTO "MissionSubTasks" ("MissionId","OrderIndex","Label","LabelEn","HintPhrase","TriggerKeywords","IsOptional","XpBonus")
VALUES
(1, 1, 'Gọi món cà phê', 'Order a coffee',
 'I would like to order a... / Could I have a...?',
 ARRAY['order','coffee','latte','cappuccino','espresso','drink','cup','would like','have a'],
 FALSE, 15),

(1, 2, 'Hỏi chỗ ngồi', 'Ask for a seat',
 'Is there a seat available? / Could I sit here?',
 ARRAY['seat','sit','table','available','place','seating','where can i sit'],
 FALSE, 10),

(1, 3, 'Hỏi mật khẩu WiFi', 'Ask for WiFi password',
 'Could I get the WiFi password, please?',
 ARRAY['wifi','wi-fi','password','internet','network','connection','wireless'],
 TRUE, 15),

(1, 4, 'Gọi thêm món', 'Order an additional item',
 'I would also like to have... / Could I also order...?',
 ARRAY['also','additionally','another','extra','pastry','cake','snack','more','and also'],
 TRUE, 10),

(1, 5, 'Tính tiền / Thanh toán', 'Ask for the bill',
 'Could I get the bill, please? / How much is it?',
 ARRAY['bill','check','pay','payment','how much','total','receipt','settle'],
 FALSE, 20)
ON CONFLICT DO NOTHING;

-- =============================================================================
-- SEED DATA: Sub-tasks cho Mission 2 — Làm theo Chỉ dẫn
-- =============================================================================
INSERT INTO "MissionSubTasks" ("MissionId","OrderIndex","Label","LabelEn","HintPhrase","TriggerKeywords","IsOptional","XpBonus")
VALUES
(2, 1, 'Xác nhận đã hiểu nhiệm vụ', 'Confirm understanding',
 'Understood. I will... / Got it, I will proceed with...',
 ARRAY['understood','got it','confirm','i will','acknowledged','proceed','clear'],
 FALSE, 10),

(2, 2, 'Hỏi làm rõ yêu cầu', 'Clarify instructions',
 'Could you clarify...? / What exactly do you mean by...?',
 ARRAY['clarify','explain','mean','could you','what exactly','specify','elaborate'],
 TRUE, 15),

(2, 3, 'Báo cáo tiến độ', 'Report task progress',
 'I have completed... / The task is now done...',
 ARRAY['completed','finished','done','progress','ready','task is','have done'],
 FALSE, 15)
ON CONFLICT DO NOTHING;

-- =============================================================================
-- SEED DATA: Sub-tasks cho Mission 3 — Tranh luận & Đàm phán
-- =============================================================================
INSERT INTO "MissionSubTasks" ("MissionId","OrderIndex","Label","LabelEn","HintPhrase","TriggerKeywords","IsOptional","XpBonus")
VALUES
(3, 1, 'Trình bày lập luận ban đầu', 'Present initial argument',
 'I believe that... / In my opinion...',
 ARRAY['believe','opinion','think','argue','position','perspective','view','i feel'],
 FALSE, 10),

(3, 2, 'Phản biện quan điểm đối phương', 'Counter opposing view',
 'However, I would argue... / On the contrary...',
 ARRAY['however','contrary','disagree','counter','but','although','while','on the other hand'],
 FALSE, 15),

(3, 3, 'Đề xuất điều khoản', 'Propose terms',
 'What if we could agree on...? / Perhaps we can find a middle ground...',
 ARRAY['propose','suggest','compromise','agree','deal','terms','middle ground','offer'],
 FALSE, 20),

(3, 4, 'Đạt được thỏa thuận', 'Reach an agreement',
 'I think we can agree on... / We have a deal!',
 ARRAY['agree','deal','accept','settled','finalize','shake hands','in agreement'],
 FALSE, 25)
ON CONFLICT DO NOTHING;

-- =============================================================================
-- SEED DATA: Sub-tasks cho Mission 4 — Phỏng vấn Xin việc
-- =============================================================================
INSERT INTO "MissionSubTasks" ("MissionId","OrderIndex","Label","LabelEn","HintPhrase","TriggerKeywords","IsOptional","XpBonus")
VALUES
(4, 1, 'Giới thiệu bản thân', 'Self introduction',
 'My name is... and I have experience in...',
 ARRAY['my name','i am','background','experience','graduated','worked','studied'],
 FALSE, 10),

(4, 2, 'Nêu lý do ứng tuyển', 'State motivation',
 'I am applying because... / I am passionate about...',
 ARRAY['applying','reason','because','passionate','interested','drawn to','inspired'],
 FALSE, 15),

(4, 3, 'Mô tả thành tích nổi bật', 'Describe key achievement',
 'One of my key achievements was... / I successfully...',
 ARRAY['achievement','accomplished','successfully','led','managed','increased','improved','delivered'],
 FALSE, 20),

(4, 4, 'Đặt câu hỏi ngược lại', 'Ask a reverse question',
 'Could you tell me more about...? / What does success look like in this role?',
 ARRAY['could you tell me','what does','how does','what is','question','wondering','curious about'],
 TRUE, 15)
ON CONFLICT DO NOTHING;

-- Verify migration
SELECT
    st."MissionId",
    m."Title",
    COUNT(st."Id") AS "SubTaskCount"
FROM "MissionSubTasks" st
JOIN "Missions" m ON m."Id" = st."MissionId"
GROUP BY st."MissionId", m."Title"
ORDER BY st."MissionId";
