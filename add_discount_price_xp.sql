-- ============================================================
-- Script: Add DiscountPriceXp to ShopItems
-- Database: Supabase (PostgreSQL)
-- Date: 2026-06-08
-- ============================================================

-- Thêm cột DiscountPriceXp (giá VIP cho Premium user)
-- Mặc định = 0 nghĩa là không có giá VIP
ALTER TABLE "ShopItems"
ADD COLUMN IF NOT EXISTS "DiscountPriceXp" INTEGER NOT NULL DEFAULT 0;

-- Cập nhật giá VIP cho các item seed hiện có
UPDATE "ShopItems" SET "DiscountPriceXp" = 160 WHERE "Id" = 1;
UPDATE "ShopItems" SET "DiscountPriceXp" = 400 WHERE "Id" = 2;
UPDATE "ShopItems" SET "DiscountPriceXp" = 800 WHERE "Id" = 3;

-- Xóa migration EF Core đã tạo (không cần nữa vì dùng Supabase)
-- (chỉ xóa file, không chạy lệnh này trong DB)
-- rm BackEnd/TheUnraveller.Infrastructure/Migrations/20260608123535_AddDiscountPriceXpToShopItem.cs
