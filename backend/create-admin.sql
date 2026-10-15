-- Promote an existing registered user after verifying their email ownership.
-- Set the email to your real administrator account; do NOT expose this script publicly.
UPDATE Users SET Role = 'Admin' WHERE Email = 'you@example.com';
