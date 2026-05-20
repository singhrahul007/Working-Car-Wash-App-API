-- ─────────────────────────────────────────────────────────────────────────────
-- Seed: SofaServices default service catalogue
-- Run this ONCE after applying the AddSofaTables migration.
-- ─────────────────────────────────────────────────────────────────────────────

INSERT INTO [dbo].[SofaServices]
    ([Name], [Description], [Category], [Price], [DurationInMinutes], [DurationDisplay], [Includes], [IsPopular], [DisplayOrder], [IsActive], [CreatedAt])
VALUES
    ('Basic Sofa Cleaning',
     'Essential sofa cleaning to remove dust and surface dirt',
     'Basic', 799.00, 90, '90 mins',
     '["Vacuuming","Spot Cleaning","Deodorizing"]',
     0, 1, 1, GETUTCDATE()),

    ('Deep Sofa Cleaning',
     'Thorough deep cleaning with steam to remove tough stains',
     'Deep', 1299.00, 150, '150 mins',
     '["Steam Cleaning","Stain Removal","Fabric Protection"]',
     1, 2, 1, GETUTCDATE()),

    ('Premium Sofa Cleaning',
     'Complete restoration and treatment for a like-new finish',
     'Premium', 1899.00, 180, '180 mins',
     '["Complete Restoration","Odor Removal","UV Treatment"]',
     1, 3, 1, GETUTCDATE()),

    ('Leather Sofa Care',
     'Specialised conditioning and protection for leather sofas',
     'Leather', 1499.00, 120, '120 mins',
     '["Leather Conditioning","Polish","Protection Coating"]',
     1, 4, 1, GETUTCDATE()),

    ('Sofa Sanitization',
     'Anti-bacterial treatment to keep your sofa germ-free',
     'Sanitization', 599.00, 60, '60 mins',
     '["Germ Protection","Anti-bacterial Treatment"]',
     0, 5, 1, GETUTCDATE()),

    ('Stain Removal',
     'Targeted treatment to eliminate stubborn stains',
     'Stain', 399.00, 45, '45 mins',
     '["Targeted Stain Treatment","Pre-treatment Spray"]',
     0, 6, 1, GETUTCDATE());

GO
PRINT 'SofaServices seeded successfully — 6 rows inserted.';
