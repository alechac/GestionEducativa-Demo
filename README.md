# Gestión Educativa

Aplicación de escritorio para administrar alumnos, cuotas y pagos de una institución educativa, con emisión de facturas electrónicas a través de ARCA (ex AFIP).

Lo desarrollé como proyecto freelance para un colegio con dos secciones (escuela e instituto de idiomas) y lo adapté para publicarlo: se quitaron los datos del cliente y toda la configuración sensible.

## Qué hace

- **Alumnos y padres:** alta, edición y búsqueda; niveles, divisiones y preinscriptos para el año siguiente.
- **Cuotas:** generación mensual y masiva, matrículas, cálculo de recargos por mora y excepciones de recargo por período.
- **Pagos:** cobro total o parcial, distribución del pago entre cuotas, vuelto y ajustes de saldo.
- **Facturación electrónica ARCA:** autenticación con certificado, solicitud de CAE, Factura C y Nota de Crédito C, con QR en el comprobante.
- **Comprobantes:** generación en HTML y conversión a PDF; envío por email.
- **Gastos:** registro de egresos con los datos del comprobante.
- **Informes:** ingresos por fecha, nivel y mes; proyección mensual; matriculados; estado de pago; alumnos sin pagos; cierre de caja.
- **Registro de actividad:** deja constancia de quién realizó cada operación sensible.

## Stack

| Área | Tecnología |
|---|---|
| Lenguaje | C# |
| Interfaz | Windows Forms (.NET Framework) |
| Arquitectura | MVP (Model-View-Presenter) con capa de servicios |
| Base de datos | MySQL, con ADO.NET (`MySql.Data`) y consultas parametrizadas |
| Facturación | Servicios web SOAP de ARCA (WSAA y WSFEv1) |
| PDF | HTML a PDF con Chromium headless (PuppeteerSharp) |
| QR | QRCoder |
| Email | `SmtpClient` |
| Seguridad | Contraseñas cifradas con DPAPI (`ProtectedData`) |
| Tests | xUnit |

## Estructura

```
Models/        Entidades y enums
Views/         Formularios WinForms e interfaces de las vistas
Presenters/    Lógica de presentación (MVP)
Services/      Reglas de negocio y acceso a datos
Database/      Conexión y scripts de migración SQL
Utils/         PDF, certificados, cifrado, utilidades
Tests/         Pruebas unitarias y de integración
```

## Autor

Alejandro, estudiante de Ingeniería en Sistemas. [LinkedIn](https://www.linkedin.com/in/TU_PERFIL)
