-- =====================================================================
-- UniMove - Script de criação do banco (SQLite)
-- Convenções: enums como TEXT (com CHECK), datas como TEXT
-- no formato 'yyyy-MM-dd HH:mm:ss'.
-- Sem PRAGMA foreign_keys: a conexão do C# liga isso.
-- =====================================================================

-- =====================================================================
-- LIMPEZA (filhas antes das pais). Só serve para recriar o banco em testes.
-- =====================================================================
DROP INDEX IF EXISTS IX_Reservas_Aluno_Viagem_Ativa;
DROP TABLE IF EXISTS Reservas;
DROP TABLE IF EXISTS Viagens;
DROP TABLE IF EXISTS PontosEmbarque;
DROP TABLE IF EXISTS Rotas;
DROP TABLE IF EXISTS Veiculos;
DROP TABLE IF EXISTS Motoristas;
DROP TABLE IF EXISTS Alunos;
DROP TABLE IF EXISTS Instituicoes;
DROP TABLE IF EXISTS Usuarios;

-- =====================================================================
-- TABELAS
-- =====================================================================
CREATE TABLE Usuarios (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome      TEXT    NOT NULL,
    Email     TEXT    NOT NULL UNIQUE,
    SenhaHash TEXT    NOT NULL,
    Perfil    TEXT    NOT NULL CHECK (Perfil IN ('Aluno', 'Motorista', 'Administrador')),
    Ativo     INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE Instituicoes (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome   TEXT NOT NULL,
    Cidade TEXT NOT NULL
);

CREATE TABLE Alunos (
    UsuarioId     INTEGER PRIMARY KEY,
    InstituicaoId INTEGER NOT NULL,
    Matricula     TEXT    NOT NULL UNIQUE,
    Curso         TEXT    NOT NULL,
    FOREIGN KEY (UsuarioId)     REFERENCES Usuarios (Id),
    FOREIGN KEY (InstituicaoId) REFERENCES Instituicoes (Id)
);

CREATE TABLE Motoristas (
    UsuarioId INTEGER PRIMARY KEY,
    Cnh       TEXT NOT NULL UNIQUE,
    Telefone  TEXT NOT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id)
);

CREATE TABLE Veiculos (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    Placa      TEXT    NOT NULL UNIQUE,
    Modelo     TEXT    NOT NULL,
    Capacidade INTEGER NOT NULL CHECK (Capacidade > 0)
);

CREATE TABLE Rotas (
    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome           TEXT    NOT NULL,
    DuracaoMinutos INTEGER NOT NULL CHECK (DuracaoMinutos > 0)
);

CREATE TABLE PontosEmbarque (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    RotaId     INTEGER NOT NULL,
    Nome       TEXT    NOT NULL,
    Referencia TEXT,
    Ordem      INTEGER NOT NULL CHECK (Ordem > 0),
    FOREIGN KEY (RotaId) REFERENCES Rotas (Id)
);

CREATE TABLE Viagens (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    RotaId          INTEGER NOT NULL,
    MotoristaId     INTEGER NOT NULL,
    VeiculoId       INTEGER NOT NULL,
    DataHoraPartida TEXT    NOT NULL,
    Tipo            TEXT    NOT NULL CHECK (Tipo IN ('Ida', 'Volta')),
    Status          TEXT    NOT NULL CHECK (Status IN ('Agendada', 'EmAndamento', 'Finalizada', 'Cancelada')),
    FOREIGN KEY (RotaId)      REFERENCES Rotas (Id),
    FOREIGN KEY (MotoristaId) REFERENCES Motoristas (UsuarioId),
    FOREIGN KEY (VeiculoId)   REFERENCES Veiculos (Id)
);

CREATE TABLE Reservas (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    ViagemId        INTEGER NOT NULL,
    AlunoId         INTEGER NOT NULL,
    PontoEmbarqueId INTEGER NOT NULL,
    Status          TEXT    NOT NULL CHECK (Status IN ('Confirmada', 'Embarcado', 'Ausente', 'Cancelada')),
    CriadaEm        TEXT    NOT NULL,
    FOREIGN KEY (ViagemId)        REFERENCES Viagens (Id),
    FOREIGN KEY (AlunoId)         REFERENCES Alunos (UsuarioId),
    FOREIGN KEY (PontoEmbarqueId) REFERENCES PontosEmbarque (Id)
);

-- Impede reserva ativa duplicada, mas permite reservar de novo após cancelar.
CREATE UNIQUE INDEX IX_Reservas_Aluno_Viagem_Ativa
    ON Reservas (AlunoId, ViagemId)
    WHERE Status <> 'Cancelada';

-- =====================================================================
-- DADOS FICTÍCIOS (pais antes dos filhos)
-- =====================================================================

-- Usuarios: 1 administrador (Id 1), 5 motoristas (Ids 2 a 6), 5 alunos (Ids 7 a 11)
INSERT INTO Usuarios (Id, Nome, Email, SenhaHash, Perfil) VALUES
    (1,  'Marcos Antônio Ribeiro',   'admin@unimove.com',            'HASH_PROVISORIO', 'Administrador'),
    (2,  'Carlos Eduardo Santos',    'carlos.santos@unimove.com',    'HASH_PROVISORIO', 'Motorista'),
    (3,  'José Roberto Lima',        'jose.lima@unimove.com',        'HASH_PROVISORIO', 'Motorista'),
    (4,  'Antônio Carlos Pereira',   'antonio.pereira@unimove.com',  'HASH_PROVISORIO', 'Motorista'),
    (5,  'Paulo Henrique Souza',     'paulo.souza@unimove.com',      'HASH_PROVISORIO', 'Motorista'),
    (6,  'Sérgio Luiz Almeida',      'sergio.almeida@unimove.com',   'HASH_PROVISORIO', 'Motorista'),
    (7,  'Ana Beatriz Oliveira',     'ana.oliveira@unimove.com',     'HASH_PROVISORIO', 'Aluno'),
    (8,  'Lucas Gabriel Ferreira',   'lucas.ferreira@unimove.com',   'HASH_PROVISORIO', 'Aluno'),
    (9,  'Mariana Costa Rodrigues',  'mariana.rodrigues@unimove.com','HASH_PROVISORIO', 'Aluno'),
    (10, 'Rafael Augusto Martins',   'rafael.martins@unimove.com',   'HASH_PROVISORIO', 'Aluno'),
    (11, 'Juliana Pereira Barbosa',  'juliana.barbosa@unimove.com',  'HASH_PROVISORIO', 'Aluno');

INSERT INTO Instituicoes (Id, Nome, Cidade) VALUES
    (1, 'FATEC Ribeirão Preto',                     'Ribeirão Preto'),
    (2, 'USP Ribeirão Preto',                       'Ribeirão Preto'),
    (3, 'Universidade de Ribeirão Preto (UNAERP)',  'Ribeirão Preto'),
    (4, 'Centro Universitário Barão de Mauá',       'Ribeirão Preto'),
    (5, 'UNIP Ribeirão Preto',                      'Ribeirão Preto');

-- Cada instituição com exatamente um aluno
INSERT INTO Alunos (UsuarioId, InstituicaoId, Matricula, Curso) VALUES
    (7,  1, '2025ADS001',    'Análise e Desenvolvimento de Sistemas'),
    (8,  2, 'USP2024-1187',  'Engenharia Química'),
    (9,  3, 'UNA2023-4521',  'Direito'),
    (10, 4, 'BM2025-0310',   'Administração'),
    (11, 5, 'UNIP2024-7788', 'Enfermagem');

INSERT INTO Motoristas (UsuarioId, Cnh, Telefone) VALUES
    (2, '04512378901', '(16) 99101-2201'),
    (3, '05623489012', '(16) 99102-3302'),
    (4, '06734590123', '(16) 99103-4403'),
    (5, '07845601234', '(16) 99104-5504'),
    (6, '08956712345', '(16) 99105-6605');

INSERT INTO Veiculos (Id, Placa, Modelo, Capacidade) VALUES
    (1, 'QRS1A23', 'Van Mercedes-Benz Sprinter',     15),
    (2, 'QRT2B34', 'Micro-ônibus Volare V8',         20),
    (3, 'QRU3C45', 'Micro-ônibus Agrale MA 9.2',     30),
    (4, 'QRV4D56', 'Ônibus Marcopolo Paradiso',      40),
    (5, 'QRW5E67', 'Ônibus Busscar Vissta Buss',     46),
    (6, 'QRX6F78', 'Van Executiva',                   4);

INSERT INTO Rotas (Id, Nome, DuracaoMinutos) VALUES
    (1, 'Santa Rosa de Viterbo - FATEC Ribeirão Preto',  70),
    (2, 'Santa Rosa de Viterbo - USP Ribeirão Preto',    80),
    (3, 'Santa Rosa de Viterbo - UNAERP',                75),
    (4, 'Santa Rosa de Viterbo - Barão de Mauá',         70),
    (5, 'Santa Rosa de Viterbo - UNIP Ribeirão Preto',   65);

-- Ordem 1 = embarque em Santa Rosa de Viterbo (viagens de Ida)
-- Ordem 2 = embarque em Ribeirão Preto, na instituição (viagens de Volta)
INSERT INTO PontosEmbarque (Id, RotaId, Nome, Referencia, Ordem) VALUES
    (1,  1, 'Praça da Matriz (Santa Rosa de Viterbo)',       'Em frente à igreja matriz',   1),
    (2,  1, 'Portaria da FATEC (Ribeirão Preto)',            'Entrada principal do câmpus', 2),
    (3,  2, 'Rodoviária de Santa Rosa de Viterbo',           'Plataforma 1',                1),
    (4,  2, 'Portão 1 da USP (Ribeirão Preto)',              'Ao lado da guarita',          2),
    (5,  3, 'Trevo de acesso (Santa Rosa de Viterbo)',       'Ponto coberto no trevo',      1),
    (6,  3, 'Portaria da UNAERP (Ribeirão Preto)',           NULL,                          2),
    (7,  4, 'Praça Central (Santa Rosa de Viterbo)',         'Ao lado do coreto',           1),
    (8,  4, 'Portaria do Barão de Mauá (Ribeirão Preto)',    'Entrada de pedestres',        2),
    (9,  5, 'Terminal Municipal (Santa Rosa de Viterbo)',    'Plataforma principal',        1),
    (10, 5, 'Portaria da UNIP (Ribeirão Preto)',             'Entrada do estacionamento',   2);

-- Viagens: 4 Finalizada, 1 EmAndamento, 4 Agendada, 1 Cancelada
-- Nenhum motorista ou veículo com viagens sobrepostas.
INSERT INTO Viagens (Id, RotaId, MotoristaId, VeiculoId, DataHoraPartida, Tipo, Status) VALUES
    (1,  1, 2, 1, '2026-11-03 06:30:00', 'Ida',   'Finalizada'),
    (2,  1, 2, 1, '2026-11-03 22:30:00', 'Volta', 'Finalizada'),
    (3,  2, 3, 2, '2026-11-04 06:00:00', 'Ida',   'Finalizada'),
    (4,  3, 4, 3, '2026-11-05 06:15:00', 'Ida',   'Finalizada'),
    (5,  4, 5, 4, '2026-11-10 06:30:00', 'Ida',   'EmAndamento'),
    (6,  1, 2, 1, '2026-11-19 06:30:00', 'Ida',   'Agendada'),
    (7,  2, 3, 5, '2026-11-19 22:30:00', 'Volta', 'Agendada'),
    (8,  5, 6, 5, '2026-11-23 06:30:00', 'Ida',   'Agendada'),
    (9,  4, 5, 6, '2026-11-26 22:30:00', 'Volta', 'Agendada'),
    (10, 3, 4, 3, '2026-11-24 06:15:00', 'Ida',   'Cancelada');

-- Reservas: o ponto de cada reserva pertence à rota da viagem.
-- Embarcado e Ausente só em viagens Finalizada ou EmAndamento.
-- Reservas 8 e 9: a aluna 7 cancelou e reservou de novo na mesma viagem 6
-- (permitido pelo índice único parcial).
INSERT INTO Reservas (Id, ViagemId, AlunoId, PontoEmbarqueId, Status, CriadaEm) VALUES
    (1,  1, 7,  1, 'Embarcado',  '2026-11-01 18:20:00'),
    (2,  1, 8,  1, 'Ausente',    '2026-11-01 20:05:00'),
    (3,  2, 7,  2, 'Embarcado',  '2026-11-02 09:10:00'),
    (4,  3, 8,  3, 'Embarcado',  '2026-11-02 21:40:00'),
    (5,  4, 9,  5, 'Embarcado',  '2026-11-03 20:00:00'),
    (6,  5, 10, 7, 'Confirmada', '2026-11-08 19:30:00'),
    (7,  5, 11, 7, 'Embarcado',  '2026-11-08 20:15:00'),
    (8,  6, 7,  1, 'Cancelada',  '2026-11-09 19:00:00'),
    (9,  6, 7,  1, 'Confirmada', '2026-11-10 09:15:00'),
    (10, 7, 8,  4, 'Confirmada', '2026-11-10 09:40:00'),
    (11, 9, 8,  8, 'Confirmada', '2026-11-12 08:10:00'),
    (12, 9, 9,  8, 'Confirmada', '2026-11-12 08:25:00'),
    (13, 9, 10, 8, 'Confirmada', '2026-11-12 08:40:00');