@echo off
setlocal EnableDelayedExpansion

REM --- CONFIGURACAO ---

REM Garante que o script execute no diretorio onde esta salvo
cd /D "%~dp0"

REM Nome do arquivo de saida
SET "OUTPUT_FILE=Relatorio_Projeto_V2.txt"

REM --- FIM DA CONFIGURACAO ---

echo.
echo ========================================================
echo GERADOR DE RELATORIO DE PROJETO ASP.NET
echo ========================================================
echo.
echo Diretorio atual: %cd%
echo Arquivo de saida: %OUTPUT_FILE%
echo.
echo Gerando relatorio... Isso pode demorar alguns segundos...
echo.

REM 1. Apaga o relatorio antigo, se existir
if exist "%OUTPUT_FILE%" del "%OUTPUT_FILE%"

REM 2. Adiciona um cabecalho ao arquivo
echo ========================================================== > "%OUTPUT_FILE%"
echo RELATORIO DE PROJETO ASP.NET - Gerado em %DATE% %TIME% >> "%OUTPUT_FILE%"
echo ========================================================== >> "%OUTPUT_FILE%"
echo Diretorio base: %cd% >> "%OUTPUT_FILE%"
echo ========================================================== >> "%OUTPUT_FILE%"
echo. >> "%OUTPUT_FILE%"

REM 3. Gera a estrutura de arvore
echo ========================================================== >> "%OUTPUT_FILE%"
echo ESTRUTURA DE ARQUIVOS (ARVORE) >> "%OUTPUT_FILE%"
echo ========================================================== >> "%OUTPUT_FILE%"
echo. >> "%OUTPUT_FILE%"

tree "%cd%" /F /A >> "%OUTPUT_FILE%" 2>&1

echo. >> "%OUTPUT_FILE%"
echo. >> "%OUTPUT_FILE%"

REM 4. Lista de todos os arquivos encontrados (para diagnostico)
echo ========================================================== >> "%OUTPUT_FILE%"
echo DIAGNOSTICO: ARQUIVOS ENCONTRADOS >> "%OUTPUT_FILE%"
echo ========================================================== >> "%OUTPUT_FILE%"
echo. >> "%OUTPUT_FILE%"

SET "TOTAL_FILES=0"

REM Busca arquivos .cs (exceto pastas ignoradas)
for /f "delims=" %%F in ('dir /b /s /a-d "*.cs" 2^>nul ^| findstr /v /i "\\bin\\ \\obj\\ \\.git\\ \\.vs\\ \\node_modules\\ \\Migrations\\ .Designer.cs .g.cs .g.i.cs AssemblyInfo.cs GlobalUsings.g.cs"') do (
    echo [INCLUIDO] %%F >> "%OUTPUT_FILE%"
    SET /A TOTAL_FILES+=1
)

REM Busca arquivos .cshtml
for /f "delims=" %%F in ('dir /b /s /a-d "*.cshtml" 2^>nul ^| findstr /v /i "\\bin\\ \\obj\\ \\.git\\ \\.vs\\ \\node_modules\\"') do (
    echo [INCLUIDO] %%F >> "%OUTPUT_FILE%"
    SET /A TOTAL_FILES+=1
)

REM Busca arquivos .csproj
for /f "delims=" %%F in ('dir /b /s /a-d "*.csproj" 2^>nul ^| findstr /v /i "\\bin\\ \\obj\\ \\.git\\ \\.vs\\"') do (
    echo [INCLUIDO] %%F >> "%OUTPUT_FILE%"
    SET /A TOTAL_FILES+=1
)

REM Busca arquivos .sln
for /f "delims=" %%F in ('dir /b /s /a-d "*.sln" 2^>nul ^| findstr /v /i "\\.git\\ \\.vs\\"') do (
    echo [INCLUIDO] %%F >> "%OUTPUT_FILE%"
    SET /A TOTAL_FILES+=1
)

REM Busca arquivos appsettings*.json
for /f "delims=" %%F in ('dir /b /s /a-d "appsettings*.json" 2^>nul ^| findstr /v /i "\\bin\\ \\obj\\ \\.git\\ \\.vs\\"') do (
    echo [INCLUIDO] %%F >> "%OUTPUT_FILE%"
    SET /A TOTAL_FILES+=1
)

echo. >> "%OUTPUT_FILE%"
echo Total de arquivos a processar: !TOTAL_FILES! >> "%OUTPUT_FILE%"
echo. >> "%OUTPUT_FILE%"
echo. >> "%OUTPUT_FILE%"

REM 5. Loop para pegar o conteudo dos arquivos de codigo
echo ========================================================== >> "%OUTPUT_FILE%"
echo CONTEUDO DOS ARQUIVOS DE CODIGO >> "%OUTPUT_FILE%"
echo ========================================================== >> "%OUTPUT_FILE%"
echo. >> "%OUTPUT_FILE%"

SET "PROCESSED=0"

REM Processa arquivos .cs
for /f "delims=" %%F in ('dir /b /s /a-d "*.cs" 2^>nul ^| findstr /v /i "\\bin\\ \\obj\\ \\.git\\ \\.vs\\ \\node_modules\\ \\Migrations\\ .Designer.cs .g.cs .g.i.cs AssemblyInfo.cs GlobalUsings.g.cs"') do (
    echo. >> "%OUTPUT_FILE%"
    echo ========================================================== >> "%OUTPUT_FILE%"
    echo ARQUIVO: %%F >> "%OUTPUT_FILE%"
    echo ========================================================== >> "%OUTPUT_FILE%"
    echo. >> "%OUTPUT_FILE%"
    
    type "%%F" >> "%OUTPUT_FILE%" 2>nul
    
    echo. >> "%OUTPUT_FILE%"
    echo. >> "%OUTPUT_FILE%"
    
    SET /A PROCESSED+=1
)

REM Processa arquivos .cshtml
for /f "delims=" %%F in ('dir /b /s /a-d "*.cshtml" 2^>nul ^| findstr /v /i "\\bin\\ \\obj\\ \\.git\\ \\.vs\\ \\node_modules\\"') do (
    echo. >> "%OUTPUT_FILE%"
    echo ========================================================== >> "%OUTPUT_FILE%"
    echo ARQUIVO: %%F >> "%OUTPUT_FILE%"
    echo ========================================================== >> "%OUTPUT_FILE%"
    echo. >> "%OUTPUT_FILE%"
    
    type "%%F" >> "%OUTPUT_FILE%" 2>nul
    
    echo. >> "%OUTPUT_FILE%"
    echo. >> "%OUTPUT_FILE%"
    
    SET /A PROCESSED+=1
)

REM Processa arquivos .csproj
for /f "delims=" %%F in ('dir /b /s /a-d "*.csproj" 2^>nul ^| findstr /v /i "\\bin\\ \\obj\\ \\.git\\ \\.vs\\"') do (
    echo. >> "%OUTPUT_FILE%"
    echo ========================================================== >> "%OUTPUT_FILE%"
    echo ARQUIVO: %%F >> "%OUTPUT_FILE%"
    echo ========================================================== >> "%OUTPUT_FILE%"
    echo. >> "%OUTPUT_FILE%"
    
    type "%%F" >> "%OUTPUT_FILE%" 2>nul
    
    echo. >> "%OUTPUT_FILE%"
    echo. >> "%OUTPUT_FILE%"
    
    SET /A PROCESSED+=1
)

REM Processa arquivos .sln
for /f "delims=" %%F in ('dir /b /s /a-d "*.sln" 2^>nul ^| findstr /v /i "\\.git\\ \\.vs\\"') do (
    echo. >> "%OUTPUT_FILE%"
    echo ========================================================== >> "%OUTPUT_FILE%"
    echo ARQUIVO: %%F >> "%OUTPUT_FILE%"
    echo ========================================================== >> "%OUTPUT_FILE%"
    echo. >> "%OUTPUT_FILE%"
    
    type "%%F" >> "%OUTPUT_FILE%" 2>nul
    
    echo. >> "%OUTPUT_FILE%"
    echo. >> "%OUTPUT_FILE%"
    
    SET /A PROCESSED+=1
)

REM Processa arquivos appsettings*.json
for /f "delims=" %%F in ('dir /b /s /a-d "appsettings*.json" 2^>nul ^| findstr /v /i "\\bin\\ \\obj\\ \\.git\\ \\.vs\\"') do (
    echo. >> "%OUTPUT_FILE%"
    echo ========================================================== >> "%OUTPUT_FILE%"
    echo ARQUIVO: %%F >> "%OUTPUT_FILE%"
    echo ========================================================== >> "%OUTPUT_FILE%"
    echo. >> "%OUTPUT_FILE%"
    
    type "%%F" >> "%OUTPUT_FILE%" 2>nul
    
    echo. >> "%OUTPUT_FILE%"
    echo. >> "%OUTPUT_FILE%"
    
    SET /A PROCESSED+=1
)

echo ========================================================== >> "%OUTPUT_FILE%"
echo FIM DO RELATORIO >> "%OUTPUT_FILE%"
echo Total de arquivos processados: !PROCESSED! >> "%OUTPUT_FILE%"
echo ========================================================== >> "%OUTPUT_FILE%"

echo.
echo ========================================================
echo CONCLUIDO!
echo ========================================================
echo.
echo Relatorio salvo em: %OUTPUT_FILE%
echo Arquivos processados: !PROCESSED!
echo.
echo Abra o arquivo para verificar o conteudo.
echo.

pause
endlocal