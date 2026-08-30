-- Alteracao pequena e retrocompativel: os dados atuais permanecem inalterados.
ALTER TABLE `Comunidades`
    ADD COLUMN `LocalSecundario` LONGTEXT NULL AFTER `LocalMapa`,
    ADD COLUMN `LocalMapaSecundario` LONGTEXT NULL AFTER `LocalSecundario`;
