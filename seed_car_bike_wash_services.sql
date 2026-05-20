-- ─────────────────────────────────────────────────────────────────────────────
-- Seed: CarWashServices & BikeWashServices default service catalogue
-- Run this ONCE after applying database migrations or running the app.
-- ─────────────────────────────────────────────────────────────────────────────

-- 1. Seed Car Wash Services
INSERT INTO [dbo].[CarWashServices]
    ([Name], [Description], [Category], [Price], [DurationInMinutes], [DurationDisplay], [Includes], [IsPopular], [DisplayOrder], [IsActive], [CreatedAt])
VALUES
    ('Basic Wash (Bucket wash)',
     'Essential bucket wash cleaning to remove surface dirt and mud',
     'Basic', 299.00, 45, '45 mins',
     '["External Body Wash","Bucket Wash","Microfiber Drying"]',
     0, 1, 1, GETUTCDATE()),

    ('Premium Wash (Water Wash)',
     'High pressure water wash for a deep external clean',
     'Premium', 499.00, 60, '1 hour',
     '["High Pressure Wash","Foam Treatment","Underbody Cleaning","Tire Polish"]',
     1, 2, 1, GETUTCDATE()),

    ('Interior Cleaning',
     'Thorough vacuuming and dry-cleaning of dashboard, seats, and mats',
     'Interior', 499.00, 60, '1 hour',
     '["Vacuuming","Dashboard Dressing","Seat Dry Cleaning","Mat Washing"]',
     0, 3, 1, GETUTCDATE()),

    ('Full Service (Water Wash + Interior)',
     'Complete premium wash combined with detailed interior cleaning',
     'Full', 699.00, 120, '2 hours',
     '["High Pressure Wash","Foam Treatment","Full Interior Vacuuming","Dashboard Dressing","Perfume Spray"]',
     1, 4, 1, GETUTCDATE()),

    ('Engine Wash',
     'Safe and professional engine compartment cleaning and degreasing',
     'Polish', 399.00, 30, '30 mins',
     '["Engine Degreasing","Dust Blowing","Dressing"]',
     0, 5, 1, GETUTCDATE()),

    ('Waxing & Polishing',
     'High-grade polymer wax coating for a glossy shine and paint protection',
     'Polish', 899.00, 90, '1.5 hours',
     '["Body Polishing","Wax Application","Scratch Micro-Reduction"]',
     1, 6, 1, GETUTCDATE()),

    ('AC Service & Cleaning',
     'AC filter cleaning and vent sanitisation for clean air delivery',
     'Polish', 799.00, 60, '1 hour',
     '["Filter Cleaning","Vent Sanitisation","Cooling Check"]',
     0, 7, 1, GETUTCDATE()),

    ('Tire Shine & Cleaning',
     'Dedicated alloy/wheel rim cleaning and deep tire gloss shining',
     'Basic', 199.00, 20, '20 mins',
     '["Wheel Rim Cleaning","Tire Polish"]',
     0, 8, 1, GETUTCDATE());

-- 2. Seed Bike Wash Services
INSERT INTO [dbo].[BikeWashServices]
    ([Name], [Description], [Category], [Price], [DurationInMinutes], [DurationDisplay], [Includes], [IsPopular], [DisplayOrder], [IsActive], [CreatedAt])
VALUES
    ('Basic Wash (Bucket wash)',
     'Quick external hand bucket wash with high-foaming shampoo',
     'Basic', 99.00, 30, '30 mins',
     '["Body Wash","Drying"]',
     0, 1, 1, GETUTCDATE()),

    ('Premium Wash (Water Wash)',
     'Pressure wash with snow foam shampoo and chain lube treatment',
     'Premium', 199.00, 45, '45 mins',
     '["Pressure Wash","Snow Foam Treatment","Tire Shine"]',
     1, 2, 1, GETUTCDATE()),

    ('Chain Cleaning & Lubrication',
     'Specialised deep cleaning of chain to remove grit, followed by high-quality lube',
     'Chain', 149.00, 30, '30 mins',
     '["Chain Degreasing","Pressure Cleaning","Motul Chain Lube Application"]',
     1, 3, 1, GETUTCDATE()),

    ('Complete Bike Service',
     'Complete deep wash, engine cleaning, waxing, and full lubrication service',
     'Complete', 599.00, 120, '2 hours',
     '["Premium Pressure Wash","Engine Cleaning","Chain Lube","Wax Polish"]',
     1, 4, 1, GETUTCDATE()),

    ('Engine Cleaning',
     'Targeted removal of grease, tar, and oil deposits from the engine area',
     'Polish', 249.00, 45, '45 mins',
     '["Engine Degreasing","Detailing Brush Cleaning"]',
     0, 5, 1, GETUTCDATE()),

    ('Polish & Wax',
     'Liquid polish treatment for fuel tank and chrome elements',
     'Polish', 349.00, 60, '1 hour',
     '["Tank Polish","Chrome Polish","Microfiber Buffing"]',
     0, 6, 1, GETUTCDATE());

GO
PRINT 'CarWashServices and BikeWashServices seeded successfully!';
