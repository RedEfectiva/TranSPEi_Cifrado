# TranSPEi_ApiModGes_DbContext
Repositorio para versionar el código del DbContext  del proyecto TranSPEi_ApiModGes
Proyecto TranSPEi_ApiModGes_DbContext

	1.- Crear proyecto de libreria del DbContext:
		dotnet new classlib -n TranSPEi_ApiModGes_DbContext
	2.- Configurar dependencias:
		dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
		dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
		dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
	3.- Agregar repositorio para registar la libreria

		dotnet nuget add source --username FranciscoLM01 --password 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 --store-password-in-clear-text --name github "https://nuget.pkg.github.com/FranciscoLM01/index.json"

	3.- Compilar
		dotnet build
	4.- Publicar la libreria:
		dotnet pack --configuration Release
		Nota: esta libreria se encuentra en TdPlusDbContext.Gestion\bin\Release\TranSPEi_ApiModGes_DbContext.1.0.0.nupkg

	6.- Subir la libreria repositorio con githut

		dotnet nuget push "bin/Release/TranSPEi_ApiModGes_DbContext.1.0.0.nupkg" --api-key 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 --source "github"

Proyecto TranSPEi_ApiModGes
	
	1.- Añadir la libreria repositorio con githut

	dotnet nuget add source --username FranciscoLM01 --password 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 --store-password-in-clear-text --name github "https://nuget.pkg.github.com/FranciscoLM01/index.json"

	2.- Agregar la libreria a las dependencias 
		Ubicar el archivo Directory.Packages.props y agregar la linea:
		<ItemGroup>
				PackageVersion Include="TranSPEi_ApiModGes_DbContext" Version="1.0.0" />
		</ItemGroup>
	3.- Agregar la libreria TranSPEi_ApiModGes_DbContext.1.0.0.nupkg con:
		<ItemGroup>
		 	<PackageReference Include="TranSPEi_ApiModGes_DbContext" />
		</ItemGroup>

		a los proyectos:

		 TranSPEiApiModGes.Infrastructure, 
		 TranSPEiApiModGes.Domain
		 TranSPEiApiModGes.Api 

	4.- Ejecutar los siguientes comandos:
		Abrimos el cmd y nos dirigimos a D:\mipc\nube\OneDrive\PN\CODIGO\repo\TranSPEi_ApiModGes\src\TranSPEiApiModGes y ejecutamos los siguientes comandos:
			dotnet restore
			dotnet build
			dotnet run
