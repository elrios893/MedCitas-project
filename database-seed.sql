-- Script para insertar datos de prueba en MedCitas
-- Ejecutar este script despu�s de aplicar las migraciones

-- 1. INSERTAR ESPECIALIDADES
INSERT INTO "Specialties" ("Id", "Nombre", "Descripcion", "DuracionConsultaMinutos", "EstaActiva")
VALUES
    ('11111111-1111-1111-1111-111111111111', 'Medicina General', 'Consulta m�dica general para diagn�stico y tratamiento de enfermedades comunes', 30, true),
    ('22222222-2222-2222-2222-222222222222', 'Cardiolog�a', 'Especialidad enfocada en el diagn�stico y tratamiento de enfermedades del coraz�n y sistema cardiovascular', 45, true),
    ('33333333-3333-3333-3333-333333333333', 'Pediatr�a', 'Atenci�n m�dica especializada para ni�os y adolescentes', 30, true),
    ('44444444-4444-4444-4444-444444444444', 'Dermatolog�a', 'Especialidad dedicada al diagn�stico y tratamiento de enfermedades de la piel', 30, true),
  ('55555555-5555-5555-5555-555555555555', 'Ginecolog�a', 'Especialidad m�dica enfocada en la salud del sistema reproductivo femenino', 40, true),
('66666666-6666-6666-6666-666666666666', 'Oftalmolog�a', 'Diagn�stico y tratamiento de enfermedades de los ojos', 30, true),
    ('77777777-7777-7777-7777-777777777777', 'Traumatolog�a', 'Especialidad en lesiones del sistema musculoesquel�tico', 40, true),
    ('88888888-8888-8888-8888-888888888888', 'Psicolog�a', 'Atenci�n en salud mental y bienestar emocional', 60, true);

-- 2. INSERTAR M�DICOS
INSERT INTO "Doctors" ("Id", "NombreCompleto", "SpecialtyId", "NumeroLicencia", "CorreoElectronico", "Telefono", "EstaActivo", "FechaRegistro")
VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Dr. Diego Armando Maradona', '11111111-1111-1111-1111-111111111111', 'MED-001', 'carlos.rodriguez@medcitas.com', '3001234567', true, NOW()),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Dra. Mar�a Gonz�lez', '22222222-2222-2222-2222-222222222222', 'MED-002', 'maria.gonzalez@medcitas.com', '3001234568', true, NOW()),
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Dr. Juan Mart�nez', '33333333-3333-3333-3333-333333333333', 'MED-003', 'juan.martinez@medcitas.com', '3001234569', true, NOW()),
    ('dddddddd-dddd-dddd-dddd-dddddddddddd', 'Dra. Ana L�pez', '44444444-4444-4444-4444-444444444444', 'MED-004', 'ana.lopez@medcitas.com', '3001234570', true, NOW()),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Dra. Laura Fern�ndez', '55555555-5555-5555-5555-555555555555', 'MED-005', 'laura.fernandez@medcitas.com', '3001234571', true, NOW()),
    ('ffffffff-ffff-ffff-ffff-ffffffffffff', 'Dr. Pedro Ram�rez', '66666666-6666-6666-6666-666666666666', 'MED-006', 'pedro.ramirez@medcitas.com', '3001234572', true, NOW()),
    ('11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Dr. Roberto Silva', '77777777-7777-7777-7777-777777777777', 'MED-007', 'roberto.silva@medcitas.com', '3001234573', true, NOW()),
    ('22222222-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Psic. Sandra Torres', '88888888-8888-8888-8888-888888888888', 'PSI-001', 'sandra.torres@medcitas.com', '3001234574', true, NOW()),
('33333333-cccc-cccc-cccc-cccccccccccc', 'Dr. Luis Herrera', '11111111-1111-1111-1111-111111111111', 'MED-008', 'luis.herrera@medcitas.com', '3001234575', true, NOW()),
 ('44444444-dddd-dddd-dddd-dddddddddddd', 'Dra. Patricia Morales', '22222222-2222-2222-2222-222222222222', 'MED-009', 'patricia.morales@medcitas.com', '3001234576', true, NOW());

-- 3. INSERTAR ADMINISTRADOR POR DEFECTO
-- Credenciales: correo: admin@medcitas.com / password: Admin2024*
-- PasswordHash generado con BCrypt factor 11
INSERT INTO "Admin" ("Id", "NombreCompleto", "CorreoElectronico", "Telefono", "PasswordHash", "FechaRegistro", "EstaActivo", "EstaVerificado", "IntentosOTPFallidos")
VALUES (
    'a0000000-0000-0000-0000-000000000001',
    'Administrador MedCitas',
    'admin@medcitas.com',
    '3000000000',
    '$2a$11$E7CtXsQHfM.4wxUwgbJsK.xjWWyrs.6mP5JM1QqbyYHQ1Eg6qjT9i',
    NOW(),
    true,
    true,
    0
)
ON CONFLICT ("CorreoElectronico") DO NOTHING;

-- Verificar datos insertados
SELECT 'Especialidades insertadas:' as tipo, COUNT(*) as cantidad FROM "Specialties";
SELECT 'M�dicos insertados:' as tipo, COUNT(*) as cantidad FROM "Doctors";

-- Mostrar especialidades y sus m�dicos
SELECT 
    s."Nombre" as Especialidad,
    d."NombreCompleto" as Medico,
    d."NumeroLicencia" as Licencia,
    d."Telefono"
FROM "Doctors" d
JOIN "Specialties" s ON d."SpecialtyId" = s."Id"
WHERE d."EstaActivo" = true
ORDER BY s."Nombre", d."NombreCompleto";
