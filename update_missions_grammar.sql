-- =============================================================================
-- THE UNRAVELLER - DATABASE SCHEMA UPDATE FOR WRITING & GRAMMAR (SUPABASE)
-- =============================================================================

-- 1. Bổ sung cột "GrammarTarget" (Mục tiêu ngữ pháp) vào bảng "Missions" nếu chưa tồn tại
ALTER TABLE "Missions" 
ADD COLUMN IF NOT EXISTS "GrammarTarget" TEXT DEFAULT '';

-- 2. Cập nhật dữ liệu mẫu (Seed Data) các mục tiêu ngữ pháp cho 6 màn chơi hiện có
UPDATE "Missions" 
SET "GrammarTarget" = 'Sử dụng câu nói lịch sự với ''Would like'' hoặc động từ khuyết thiếu ''Could/May''.' 
WHERE "Id" = 1;

UPDATE "Missions" 
SET "GrammarTarget" = 'Sử dụng câu mệnh lệnh (Imperatives) hoặc thể bị động (Passive voice) để xác nhận nhiệm vụ.' 
WHERE "Id" = 2;

UPDATE "Missions" 
SET "GrammarTarget" = 'Sử dụng câu điều kiện loại 1 (If... will...) hoặc loại 2 (If... would...) để đàm phán.' 
WHERE "Id" = 3;

UPDATE "Missions" 
SET "GrammarTarget" = 'Sử dụng câu phức chứa mệnh đề quan hệ (Relative Clauses) hoặc liên từ (Because, Although).' 
WHERE "Id" = 4;

UPDATE "Missions" 
SET "GrammarTarget" = 'Sử dụng trạng từ mô tả (Descriptive Adverbs) và thì Quá khứ đơn (Past Simple) để báo cáo chứng cứ.' 
WHERE "Id" = 5;

UPDATE "Missions" 
SET "GrammarTarget" = 'Sử dụng câu giả định (Subjunctive Mood) hoặc lối nói gián tiếp (Reported Speech) ở trình độ cao.' 
WHERE "Id" = 6;
