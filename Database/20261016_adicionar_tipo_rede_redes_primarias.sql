-- Alteracao pequena e retrocompativel: os vinculos existentes passam a ser da Rede Primaria.
ALTER TABLE `RedesPrimarias`
    ADD COLUMN `TipoRede` VARCHAR(20) NOT NULL DEFAULT 'Primaria' AFTER `TipoRelacao`;
