-- =============================================================================
-- THE UNRAVELLER - UPDATE MISSION DESCRIPTIONS WITH IMMERSIVE OPENERS
-- =============================================================================

-- Cập nhật bối cảnh hoặc lời thoại khởi đầu của nhân vật (Mô tả bối cảnh nhập vai) cho 6 màn chơi mặc định

UPDATE "Missions" 
SET "Description" = '*The hum of neon lights fills the cozy cyber-café. The Barista wipes down the metallic counter, looking up with a friendly smile.* "Welcome to Neon Mug! What can I get started for you today? We''ve got fresh cyber-brews and synthetic pastries."' 
WHERE "Id" = 1;

UPDATE "Missions" 
SET "Description" = '*The Supervisor taps their digital clipboard impatiently as you step into the assembly bay. The neon screens flicker behind them.* "You''re late. We have a heavy shipment of hover-car battery cores to calibrate today. Let me know when you''re ready for your instructions."' 
WHERE "Id" = 2;

UPDATE "Missions" 
SET "Description" = '*The glass walls of the boardroom overlook the sprawling city skyline. The CEO leans forward, folding their hands.* "Thank you for coming. We need to reach a deal on the technology sharing agreement. If you agree to our terms, we can sign today. What are your thoughts?"' 
WHERE "Id" = 3;

UPDATE "Missions" 
SET "Description" = '*You sit opposite the interviewer in a sleek high-tech office. The HR manager smiles warmly.* "Welcome. I''ve reviewed your credentials and they look impressive. To begin, could you tell me why you want to work here at CyberTech Industries?"' 
WHERE "Id" = 4;

UPDATE "Missions" 
SET "Description" = '*Rain beats against the dirty precinct window. Chief Detective Henderson tosses a case file containing glowing holograms onto the table.* "Grab a seat. The cyber-vault at Sector 7 was cracked wide open last night. Tell me exactly what you found at the crime scene."' 
WHERE "Id" = 5;

UPDATE "Missions" 
SET "Description" = '*You stand in the dim undercity market, surrounded by holographic advertisements. A shady merchant whispers from the shadows.* "Psst... I hear you''re looking for the decryption key. I might have it, but it''s going to cost you. What did you bring to trade?"' 
WHERE "Id" = 6;
