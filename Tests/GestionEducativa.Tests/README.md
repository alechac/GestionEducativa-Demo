# Tests automáticos — GestionEducativa

## Qué hay acá

| Archivo | Tipo | Necesita base de datos? |
|---|---|---|
| `CuotaServiceCalculosTests.cs` | Puro | No (pero App.config debe existir igual — ver abajo) |
| `NivelUtilsTests.cs` | Puro | No |
| `PagoServiceCalculosTests.cs` | Puro | No |
| `CuotaServiceIntegrationTests.cs` | Integración | Sí — una base de PRUEBA aparte |

## Pasos para dejarlo andando

1. **Agregar el proyecto a la solución**: en Visual Studio, clic derecho sobre la
   solución → Add → Existing Project → elegir
   `Tests/GestionEducativa.Tests/GestionEducativa.Tests.csproj`.

2. **Corregir la referencia al proyecto principal**: abrí el `.csproj` de este
   proyecto de tests y fijate la línea `<ProjectReference Include="..\..\GestionEducativa.csproj" />`
   — puse un nombre/ruta de ejemplo porque no tenía tu `.csproj` real a mano.
   Cambiala por el nombre y la ubicación reales (o simplemente borrala y
   agregá la referencia desde Visual Studio: clic derecho en el proyecto de
   tests → Add → Project Reference → tildar GestionEducativa).

3. **Verificar el TargetFramework**: en el mismo `.csproj`, la línea
   `<TargetFramework>net48</TargetFramework>` tiene que decir lo mismo que
   usa tu proyecto principal (ej. si el principal es .NET Framework 4.7.2,
   poné `net472`).

4. **Restaurar paquetes NuGet**: al abrir la solución, Visual Studio debería
   ofrecer restaurar automáticamente (xunit, NSubstitute, MySql.Data). Si no,
   clic derecho en la solución → Restore NuGet Packages.

5. **Para los tests de integración únicamente** — creá una base de datos de
   prueba aparte (ej. `gestioneducativa_test`) en tu MySQL local, corré ahí
   las mismas migraciones que usa la base real (carpeta `Database/` del
   proyecto principal), y editá `App.config` de este proyecto con el usuario/
   contraseña que corresponda. **Nunca apunten esto a la base del cliente.**

## Cómo correrlos

En Visual Studio: **Test → Test Explorer**, y ahí "Run All". Los tests puros
van a andar apenas compile, sin configurar nada de base de datos más que el
`App.config` (que ya viene armado). Los de integración van a fallar hasta que
completes el paso 5.

## Cómo se van a usar de acá en adelante

Cada vez que termine una versión nueva y te la pase, corré **Test Explorer →
Run All** antes de probar a mano. Si algo rompió un cálculo de recargo, una
regla de generación de cuotas, o la distribución de un pago, te va a aparecer
en rojo ahí — no hace falta que repitas todo el circuito manual para
enterarte. Lo manual queda para lo que no está cubierto todavía (informes,
ABM de padres, PDF, ARCA).
