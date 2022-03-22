# Script para analisar qualidade de código no SonarQube localmente
#
# Certifique-se antes de roda-lo que os seguintes requisitos estejam cumpridos:
# -> Git bash esteja instalado;
# -> Docker Desktop for windows esteja instalado;
# -> coverlet.collector adicionado nos projetos de testes
# -> Container com servidor do SonarQube que esteja ativo e pronto para receber requests
#    na porta 9000. Para tal fim, utilize o comando abaixo.
#    $ docker run -d -p 9000:9000 sonarqube


# Instala command line tool para analisar codigo no sonarqube
dotnet tool install -g dotnet-sonarscanner
# Instala command line tool para gerar relatorio de cobertura de codigo
dotnet tool install -g dotnet-reportgenerator-globaltool

# Workaround para evitar que seja exigido a mudança da senha do administrador
curl -u admin:admin -X POST "http://localhost:9000/api/users/change_password?login=admin&previousPassword=admin&password=admin1"
curl -u admin:admin1 -X POST "http://localhost:9000/api/users/change_password?login=admin&previousPassword=admin1&password=admin"

# Inicia análise
MSYS2_ARG_CONV_EXCL="*" dotnet sonarscanner begin \
/k:"session-azure-bus-service-example" \
/d:sonar.login=admin \
/d:sonar.password=admin \
/d:"sonar.host.url=http://localhost:9000" \
/d:sonar.verbose=true

# Builda aplicação
dotnet build SessionAzureBusServiceExample.sln

# Finaliza análise no SonarQube
MSYS2_ARG_CONV_EXCL="*" dotnet sonarscanner end \
/d:sonar.login=admin \
/d:sonar.password=admin

# Remove diretorio .sonarqube que, por algum motivo, impede que uma segunda analise seja executada
rm -rf .sonarqube

# Abre site com a análise
start http://localhost:9000/dashboard?id=session-azure-bus-service-example

read -p "Pressione enter para sair"
