-- Alteracao pequena e retrocompativel: tres novas marcacoes de rede no cadastro do ator
-- (Parceiro, Operador par, Recurso relacional). Os atores existentes comecam desmarcados.
-- Aplicar em bancos ja criados com Empodera.schema.sql (instalacoes novas ja trazem as colunas).
ALTER TABLE atores
    ADD COLUMN Parceiro          TINYINT UNSIGNED NOT NULL DEFAULT 0 AFTER Mcomunidade,
    ADD COLUMN OperadorPar       TINYINT UNSIGNED NOT NULL DEFAULT 0 AFTER Parceiro,
    ADD COLUMN RecursoRelacional TINYINT UNSIGNED NOT NULL DEFAULT 0 AFTER OperadorPar,
    ADD CONSTRAINT CK_Atores_Parceiro CHECK (Parceiro IN (0, 1)),
    ADD CONSTRAINT CK_Atores_OperadorPar CHECK (OperadorPar IN (0, 1)),
    ADD CONSTRAINT CK_Atores_RecursoRelacional CHECK (RecursoRelacional IN (0, 1));
