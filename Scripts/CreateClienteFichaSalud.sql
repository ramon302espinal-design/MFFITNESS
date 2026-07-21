-- Ficha de salud / emergencia (se crea también automáticamente al primer alta).
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ClienteFichaSalud')
BEGIN
    CREATE TABLE dbo.ClienteFichaSalud
    (
        ClienteId INT NOT NULL PRIMARY KEY
            CONSTRAINT FK_ClienteFichaSalud_Clientes
            REFERENCES dbo.Clientes(ID) ON DELETE CASCADE,
        EmergenciaNombre NVARCHAR(120) NULL,
        EmergenciaParentesco NVARCHAR(80) NULL,
        EmergenciaTelefono NVARCHAR(40) NULL,
        EmergenciaTelefonoAlt NVARCHAR(40) NULL,
        Diabetes BIT NOT NULL CONSTRAINT DF_CFS_Diabetes DEFAULT(0),
        Hipertension BIT NOT NULL CONSTRAINT DF_CFS_Hipertension DEFAULT(0),
        Asma BIT NOT NULL CONSTRAINT DF_CFS_Asma DEFAULT(0),
        ProblemasCardiacos BIT NOT NULL CONSTRAINT DF_CFS_Cardiacos DEFAULT(0),
        ColesterolAlto BIT NOT NULL CONSTRAINT DF_CFS_Colesterol DEFAULT(0),
        Artritis BIT NOT NULL CONSTRAINT DF_CFS_Artritis DEFAULT(0),
        Hernia BIT NOT NULL CONSTRAINT DF_CFS_Hernia DEFAULT(0),
        Epilepsia BIT NOT NULL CONSTRAINT DF_CFS_Epilepsia DEFAULT(0),
        Embarazo BIT NOT NULL CONSTRAINT DF_CFS_Embarazo DEFAULT(0),
        NingunaEnfermedad BIT NOT NULL CONSTRAINT DF_CFS_Ninguna DEFAULT(0),
        EnfermedadOtra NVARCHAR(200) NULL,
        LesionHombro BIT NOT NULL CONSTRAINT DF_CFS_Hombro DEFAULT(0),
        LesionRodilla BIT NOT NULL CONSTRAINT DF_CFS_Rodilla DEFAULT(0),
        LesionEspalda BIT NOT NULL CONSTRAINT DF_CFS_Espalda DEFAULT(0),
        LesionCuello BIT NOT NULL CONSTRAINT DF_CFS_Cuello DEFAULT(0),
        LesionTobillo BIT NOT NULL CONSTRAINT DF_CFS_Tobillo DEFAULT(0),
        LesionCadera BIT NOT NULL CONSTRAINT DF_CFS_Cadera DEFAULT(0),
        LesionDescripcion NVARCHAR(500) NULL,
        TomaMedicamentos BIT NOT NULL CONSTRAINT DF_CFS_TomaMeds DEFAULT(0),
        ListaMedicamentos NVARCHAR(500) NULL,
        TieneAlergias BIT NOT NULL CONSTRAINT DF_CFS_Alergias DEFAULT(0),
        AlergiasDescripcion NVARCHAR(500) NULL,
        TieneCirugias BIT NOT NULL CONSTRAINT DF_CFS_Cirugias DEFAULT(0),
        CirugiasDescripcion NVARCHAR(500) NULL,
        CirugiasFecha DATE NULL,
        FechaIngreso DATE NULL,
        FechaActualizacion DATETIME2 NOT NULL CONSTRAINT DF_CFS_FechaAct DEFAULT(SYSDATETIME())
    );
END
GO

IF COL_LENGTH('dbo.ClienteFichaSalud', 'TieneAlergias') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD TieneAlergias BIT NOT NULL CONSTRAINT DF_CFS_Alergias DEFAULT(0);
IF COL_LENGTH('dbo.ClienteFichaSalud', 'AlergiasDescripcion') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD AlergiasDescripcion NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.ClienteFichaSalud', 'TieneCirugias') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD TieneCirugias BIT NOT NULL CONSTRAINT DF_CFS_Cirugias DEFAULT(0);
IF COL_LENGTH('dbo.ClienteFichaSalud', 'CirugiasDescripcion') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD CirugiasDescripcion NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.ClienteFichaSalud', 'CirugiasFecha') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD CirugiasFecha DATE NULL;
IF COL_LENGTH('dbo.ClienteFichaSalud', 'ObjPerderGrasa') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD ObjPerderGrasa BIT NOT NULL CONSTRAINT DF_CFS_ObjGrasa DEFAULT(0);
IF COL_LENGTH('dbo.ClienteFichaSalud', 'ObjGanarMasa') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD ObjGanarMasa BIT NOT NULL CONSTRAINT DF_CFS_ObjMasa DEFAULT(0);
IF COL_LENGTH('dbo.ClienteFichaSalud', 'ObjTonificar') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD ObjTonificar BIT NOT NULL CONSTRAINT DF_CFS_ObjToni DEFAULT(0);
IF COL_LENGTH('dbo.ClienteFichaSalud', 'ObjMejorarCondicion') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD ObjMejorarCondicion BIT NOT NULL CONSTRAINT DF_CFS_ObjCond DEFAULT(0);
IF COL_LENGTH('dbo.ClienteFichaSalud', 'ObjRehabilitacion') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD ObjRehabilitacion BIT NOT NULL CONSTRAINT DF_CFS_ObjRehab DEFAULT(0);
IF COL_LENGTH('dbo.ClienteFichaSalud', 'ObjSalud') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD ObjSalud BIT NOT NULL CONSTRAINT DF_CFS_ObjSalud DEFAULT(0);
IF COL_LENGTH('dbo.ClienteFichaSalud', 'ObjCompetencia') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD ObjCompetencia BIT NOT NULL CONSTRAINT DF_CFS_ObjComp DEFAULT(0);
IF COL_LENGTH('dbo.ClienteFichaSalud', 'ObjOtro') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD ObjOtro BIT NOT NULL CONSTRAINT DF_CFS_ObjOtro DEFAULT(0);
IF COL_LENGTH('dbo.ClienteFichaSalud', 'ObjOtroDescripcion') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD ObjOtroDescripcion NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.ClienteFichaSalud', 'ExperienciaNivel') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD ExperienciaNivel NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.ClienteFichaSalud', 'HorarioPreferido') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD HorarioPreferido NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.ClienteFichaSalud', 'HorarioVariadoDetalle') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD HorarioVariadoDetalle NVARCHAR(200) NULL;
GO
