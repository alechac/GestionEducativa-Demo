using GestionEducativa.Models;
using GestionEducativa.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    public partial class ArcaPreEmisionForm : Form
    {
        private readonly Pago _pago;
        private readonly PagoService _pagoService;

        // Controles de entrada CAE
        private TextBox txtCAE;
        private TextBox txtNroComprobante;
        private DateTimePicker dtpVencCAE;
        private Button btnGuardarCAE;
        private Button btnOmitir;
        private Button btnAbrirArca;
        private Label lblEstado;

        public ArcaPreEmisionForm(Pago pago)
        {
            _pago = pago;
            _pagoService = new PagoService();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Emitir Factura C en ARCA";
            this.Size = new Size(680, 720);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 248, 250);
            this.Font = new Font("Segoe UI", 9F);

            // ── Scroll externo por si la pantalla es chica
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            this.Controls.Add(scroll);

            int y = 0;

            // ══════════════════════════════════════════
            // CABECERA
            // ══════════════════════════════════════════
            var cab = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(680, 64),
                BackColor = Color.FromArgb(26, 82, 118)
            };

            cab.Controls.Add(new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 10),
                Text = "📄  Factura C — ARCA"
            });
            cab.Controls.Add(new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(174, 214, 241),
                Location = new Point(18, 38),
                Text = "Usá estos datos para cargar el comprobante en ARCA. Copiá cada campo con el botón 📋."
            });
            scroll.Controls.Add(cab);
            y += 72;

            // ══════════════════════════════════════════
            // SECCIÓN 1 — Configuración del comprobante
            // ══════════════════════════════════════════
            y = AgregarSeccion(scroll, y, "1. Tipo de comprobante", Color.FromArgb(52, 73, 94));

            AgregarFila(scroll, ref y, "Tipo", "Factura C", copiable: true);
            AgregarFila(scroll, ref y, "Concepto", "Servicios", copiable: true);
            AgregarFila(scroll, ref y, "Punto de venta", "(el habitual del colegio)", copiable: false, nota: true);

            // ══════════════════════════════════════════
            // SECCIÓN 2 — Datos del receptor (pagador)
            // ══════════════════════════════════════════
            y = AgregarSeccion(scroll, y, "2. Datos del receptor", Color.FromArgb(39, 109, 163));

            string condIva = "Consumidor Final";
            string docTipo = "DNI";
            string docNum = _pago.DocumentoPagador ?? "";

            if (_pago.TipoPagador == "CUIT")
            {
                condIva = "IVA Exento";
                docTipo = "CUIT";
            }

            AgregarFila(scroll, ref y, "Condición frente al IVA", condIva, copiable: true);
            AgregarFila(scroll, ref y, "Tipo documento", docTipo, copiable: true);
            AgregarFila(scroll, ref y, "Nro. documento", docNum, copiable: true);
            AgregarFila(scroll, ref y, "Nombre / Razón Social", _pago.NombrePagador, copiable: true);

            // ══════════════════════════════════════════
            // SECCIÓN 3 — Período del servicio
            // ══════════════════════════════════════════
            y = AgregarSeccion(scroll, y, "3. Período del servicio", Color.FromArgb(22, 160, 133));

            // Calcular período de servicio a partir de las cuotas
            string periodoDesde = "";
            string periodoHasta = "";
            string fechaVto = "";

            if (_pago.Detalles != null && _pago.Detalles.Count > 0)
            {
                // El período que ARCA pide es el del servicio (ej: el mes de la cuota más antigua)
                var periodos = _pago.Detalles
                    .Select(d => d.PeriodoCuota)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList();

                // Intentar parsear "Mes YYYY" para armar fecha real
                var fechasPeriodo = periodos
                    .Select(p => ParsearPeriodo(p))
                    .Where(f => f.HasValue)
                    .OrderBy(f => f)
                    .ToList();

                if (fechasPeriodo.Count > 0)
                {
                    var primerDia = new DateTime(fechasPeriodo.First().Value.Year,
                                                  fechasPeriodo.First().Value.Month, 1);
                    var ultimoDia = new DateTime(fechasPeriodo.Last().Value.Year,
                                                  fechasPeriodo.Last().Value.Month,
                                                  DateTime.DaysInMonth(fechasPeriodo.Last().Value.Year,
                                                                       fechasPeriodo.Last().Value.Month));
                    periodoDesde = primerDia.ToString("dd/MM/yyyy");
                    periodoHasta = ultimoDia.ToString("dd/MM/yyyy");
                    fechaVto = ultimoDia.ToString("dd/MM/yyyy");
                }
            }

            if (string.IsNullOrEmpty(periodoDesde))
            {
                periodoDesde = new DateTime(_pago.FechaPago.Year, _pago.FechaPago.Month, 1).ToString("dd/MM/yyyy");
                periodoHasta = new DateTime(_pago.FechaPago.Year, _pago.FechaPago.Month,
                    DateTime.DaysInMonth(_pago.FechaPago.Year, _pago.FechaPago.Month)).ToString("dd/MM/yyyy");
                fechaVto = periodoHasta;
            }

            AgregarFila(scroll, ref y, "Fecha desde", periodoDesde, copiable: true);
            AgregarFila(scroll, ref y, "Fecha hasta", periodoHasta, copiable: true);
            AgregarFila(scroll, ref y, "Fecha vencimiento", fechaVto, copiable: true);

            // ══════════════════════════════════════════
            // SECCIÓN 4 — Detalle de servicios (los ítems)
            // ══════════════════════════════════════════
            y = AgregarSeccion(scroll, y, "4. Servicios a agregar en el comprobante", Color.FromArgb(142, 68, 173));

            if (_pago.Detalles != null)
            {
                var porAlumno = _pago.Detalles.GroupBy(d => d.AlumnoNombre ?? "Alumno");
                foreach (var grupo in porAlumno)
                {
                    foreach (var det in grupo)
                    {
                        string nombre = !string.IsNullOrWhiteSpace(det.AlumnoNombre)
                            ? det.AlumnoNombre : grupo.Key;

                        // Ítem 1: cuota base
                        string descBase = $"{det.PeriodoCuota} — {nombre}";
                        AgregarFila(scroll, ref y,
                            descBase,
                            $"$ {det.MontoAplicado:N2}",
                            copiable: true, esItem: true);

                        // Ítem 2: recargo (solo si existe)
                        if (det.TieneRecargo)
                        {
                            AgregarFila(scroll, ref y,
                                $"Recargo mora — {det.PeriodoCuota} — {nombre}",
                                $"$ {det.RecargoAplicado:N2}",
                                copiable: true, esItem: true);
                        }
                    }
                }
            }

            // Total
            AgregarFilaTotal(scroll, ref y, _pago.TotalAplicado);

            // ══════════════════════════════════════════
            // SECCIÓN 5 — Método de pago
            // ══════════════════════════════════════════
            y = AgregarSeccion(scroll, y, "5. Forma de pago", Color.FromArgb(243, 156, 18));
            AgregarFila(scroll, ref y, "Forma de pago", "Efectivo", copiable: true,
                nota: false, notaTexto: "Cambiar si pagó con transferencia o cheque");

            // ══════════════════════════════════════════
            // BOTÓN ABRIR ARCA
            // ══════════════════════════════════════════
            y += 10;
            btnAbrirArca = new Button
            {
                Location = new Point(16, y),
                Size = new Size(640, 42),
                Text = "🌐  Abrir ARCA — Comprobantes en Línea",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(26, 82, 118),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnAbrirArca.FlatAppearance.BorderSize = 0;
            btnAbrirArca.Click += (s, e) =>
                Process.Start(new ProcessStartInfo(
                    "https://auth.afip.gob.ar/contribuyente_/login.xhtml")
                { UseShellExecute = true });
            scroll.Controls.Add(btnAbrirArca);
            y += 50;

            // ══════════════════════════════════════════
            // SECCIÓN 6 — Ingresar CAE devuelto por ARCA
            // ══════════════════════════════════════════
            y = AgregarSeccion(scroll, y, "6. Ingresar datos devueltos por ARCA", Color.FromArgb(39, 174, 96));

            var panelCae = new Panel
            {
                Location = new Point(16, y),
                Size = new Size(640, 110),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            panelCae.Controls.Add(new Label
            {
                AutoSize = true,
                Location = new Point(10, 12),
                Text = "Nro. comprobante ARCA:",
                ForeColor = Color.Gray
            });
            txtNroComprobante = new TextBox
            {
                Location = new Point(180, 9),
                Size = new Size(180, 23),
                Text = "ej: 00001-00000042",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            panelCae.Controls.Add(txtNroComprobante);

            panelCae.Controls.Add(new Label
            {
                AutoSize = true,
                Location = new Point(10, 44),
                Text = "CAE:",
                ForeColor = Color.Gray
            });
            txtCAE = new TextBox
            {
                Location = new Point(180, 41),
                Size = new Size(200, 23),
                Text = "14 dígitos",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                MaxLength = 14
            };
            panelCae.Controls.Add(txtCAE);

            panelCae.Controls.Add(new Label
            {
                AutoSize = true,
                Location = new Point(10, 76),
                Text = "Vencimiento CAE:",
                ForeColor = Color.Gray
            });
            dtpVencCAE = new DateTimePicker
            {
                Location = new Point(180, 73),
                Size = new Size(160, 23),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(10)
            };
            panelCae.Controls.Add(dtpVencCAE);

            scroll.Controls.Add(panelCae);
            y += 118;

            // ══════════════════════════════════════════
            // BOTONES FINALES
            // ══════════════════════════════════════════
            btnGuardarCAE = new Button
            {
                Location = new Point(16, y),
                Size = new Size(310, 40),
                Text = "✔  Guardar CAE y cerrar",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnGuardarCAE.FlatAppearance.BorderSize = 0;
            btnGuardarCAE.Click += BtnGuardarCAE_Click;

            btnOmitir = new Button
            {
                Location = new Point(346, y),
                Size = new Size(310, 40),
                Text = "⏭  Omitir (registrar sin CAE)",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnOmitir.FlatAppearance.BorderSize = 0;
            btnOmitir.Click += BtnOmitir_Click;

            scroll.Controls.Add(btnGuardarCAE);
            scroll.Controls.Add(btnOmitir);
            y += 48;

            lblEstado = new Label
            {
                AutoSize = false,
                Size = new Size(640, 20),
                Location = new Point(16, y),
                Text = "",
                ForeColor = Color.FromArgb(39, 174, 96),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };
            scroll.Controls.Add(lblEstado);
            y += 28;

            scroll.AutoScrollMinSize = new Size(0, y);
        }

        // ─────────────────────────────────────────────────────────
        // Helpers para construir las filas del formulario
        // ─────────────────────────────────────────────────────────

        private int AgregarSeccion(Panel scroll, int y, string titulo, Color color)
        {
            y += 6;
            var header = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(680, 26),
                BackColor = color
            };
            header.Controls.Add(new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 4),
                Text = titulo
            });
            scroll.Controls.Add(header);
            return y + 28;
        }

        private void AgregarFila(Panel scroll, ref int y,
            string etiqueta, string valor,
            bool copiable = false,
            bool nota = false, string notaTexto = null,
            bool esItem = false)
        {
            // Los ítems de servicio tienen dos botones (descripción + importe): necesitan más alto
            int altoFila = esItem ? 48 : 28;

            var fila = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(680, altoFila),
                BackColor = esItem ? Color.FromArgb(248, 249, 250) : Color.White
            };

            fila.Paint += (s, e) =>
                e.Graphics.DrawLine(Pens.LightGray, 0, fila.Height - 1, fila.Width, fila.Height - 1);

            int xEtiqueta = esItem ? 30 : 16;
            int anchoEtiqueta = 260;

            if (esItem)
            {
                // Bullet point
                fila.Controls.Add(new Label
                {
                    AutoSize = false,
                    Size = new Size(20, altoFila),
                    Location = new Point(14, 0),
                    Text = "•",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(142, 68, 173),
                    TextAlign = ContentAlignment.MiddleCenter
                });

                // ── Modo ítem: descripción arriba, importe abajo, cada uno con su botón ──
                var lblDescripcion = new Label
                {
                    AutoSize = false,
                    Size = new Size(380, 22),
                    Location = new Point(xEtiqueta, 3),
                    Text = etiqueta,
                    Font = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                fila.Controls.Add(lblDescripcion);

                var btnCopiarDesc = new Button
                {
                    Size = new Size(90, 20),
                    Location = new Point(416, 3),
                    Text = "📋 Descripción",
                    Tag = etiqueta,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(142, 68, 173),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 7.5F),
                    Cursor = Cursors.Hand
                };
                btnCopiarDesc.FlatAppearance.BorderSize = 0;
                btnCopiarDesc.Click += (s2, e2) =>
                {
                    Clipboard.SetText(btnCopiarDesc.Tag?.ToString() ?? "");
                    btnCopiarDesc.Text = "✓ Copiado";
                    btnCopiarDesc.BackColor = Color.FromArgb(39, 174, 96);
                    var t = new System.Windows.Forms.Timer { Interval = 1500 };
                    t.Tick += (ts, te) =>
                    {
                        btnCopiarDesc.Text = "📋 Descripción";
                        btnCopiarDesc.BackColor = Color.FromArgb(142, 68, 173);
                        t.Stop();
                    };
                    t.Start();
                };
                fila.Controls.Add(btnCopiarDesc);

                var lblImporte = new Label
                {
                    AutoSize = false,
                    Size = new Size(140, 20),
                    Location = new Point(xEtiqueta, 26),
                    Text = valor,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(26, 82, 118),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                fila.Controls.Add(lblImporte);

                var btnCopiarImporte = new Button
                {
                    Size = new Size(90, 20),
                    Location = new Point(416, 26),
                    Text = "📋 Importe",
                    Tag = valor,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 7.5F),
                    Cursor = Cursors.Hand
                };
                btnCopiarImporte.FlatAppearance.BorderSize = 0;
                btnCopiarImporte.Click += (s2, e2) =>
                {
                    // Copiar solo el número sin el símbolo $
                    string importeLimpio = (btnCopiarImporte.Tag?.ToString() ?? "").Replace("$ ", "").Trim();
                    Clipboard.SetText(importeLimpio);
                    btnCopiarImporte.Text = "✓ Copiado";
                    btnCopiarImporte.BackColor = Color.FromArgb(39, 174, 96);
                    var t = new System.Windows.Forms.Timer { Interval = 1500 };
                    t.Tick += (ts, te) =>
                    {
                        btnCopiarImporte.Text = "📋 Importe";
                        btnCopiarImporte.BackColor = Color.FromArgb(52, 152, 219);
                        t.Stop();
                    };
                    t.Start();
                };
                fila.Controls.Add(btnCopiarImporte);

                scroll.Controls.Add(fila);
                y += fila.Height;
                return;
            }

            fila.Controls.Add(new Label
            {
                AutoSize = false,
                Size = new Size(anchoEtiqueta, fila.Height),
                Location = new Point(xEtiqueta, 0),
                Text = etiqueta + (nota ? " ⚠" : ""),
                Font = new Font("Segoe UI", 9F, nota ? FontStyle.Italic : FontStyle.Regular),
                ForeColor = nota ? Color.FromArgb(180, 100, 0) : Color.FromArgb(80, 80, 80),
                TextAlign = ContentAlignment.MiddleLeft
            });

            var lblValor = new Label
            {
                AutoSize = false,
                Size = new Size(copiable ? 220 : 280, fila.Height),
                Location = new Point(xEtiqueta + anchoEtiqueta, 0),
                Text = valor,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 82, 118),
                TextAlign = ContentAlignment.MiddleLeft
            };
            fila.Controls.Add(lblValor);

            if (copiable)
            {
                var btnCopiar = new Button
                {
                    Size = new Size(54, 22),
                    Location = new Point(xEtiqueta + anchoEtiqueta + 224, 3),
                    Text = "📋 Copiar",
                    Tag = valor,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 7.5F),
                    Cursor = Cursors.Hand
                };
                btnCopiar.FlatAppearance.BorderSize = 0;
                btnCopiar.Click += (s2, e2) =>
                {
                    Clipboard.SetText(btnCopiar.Tag?.ToString() ?? "");
                    btnCopiar.Text = "✓ Copiado";
                    btnCopiar.BackColor = Color.FromArgb(39, 174, 96);
                    var t = new System.Windows.Forms.Timer { Interval = 1500 };
                    t.Tick += (ts, te) =>
                    {
                        btnCopiar.Text = "📋 Copiar";
                        btnCopiar.BackColor = Color.FromArgb(52, 152, 219);
                        t.Stop();
                    };
                    t.Start();
                };
                fila.Controls.Add(btnCopiar);
            }

            if (!string.IsNullOrEmpty(notaTexto))
            {
                fila.Height += 14;
                fila.Controls.Add(new Label
                {
                    AutoSize = true,
                    Location = new Point(xEtiqueta + anchoEtiqueta, fila.Height - 14),
                    Text = notaTexto,
                    Font = new Font("Segoe UI", 7.5F, FontStyle.Italic),
                    ForeColor = Color.Gray
                });
            }

            scroll.Controls.Add(fila);
            y += fila.Height;
        }

        private void AgregarFilaTotal(Panel scroll, ref int y, decimal total)
        {
            var fila = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(680, 32),
                BackColor = Color.FromArgb(26, 82, 118)
            };

            fila.Controls.Add(new Label
            {
                AutoSize = false,
                Size = new Size(300, 32),
                Location = new Point(16, 0),
                Text = "TOTAL DEL COMPROBANTE",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            });

            var lblTotal = new Label
            {
                AutoSize = false,
                Size = new Size(160, 32),
                Location = new Point(316, 0),
                Text = $"$ {total:N2}",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(130, 224, 170),
                TextAlign = ContentAlignment.MiddleLeft
            };
            fila.Controls.Add(lblTotal);

            var btnCopiarTotal = new Button
            {
                Size = new Size(54, 24),
                Location = new Point(480, 4),
                Text = "📋 Copiar",
                Tag = total.ToString("N2"),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 7.5F),
                Cursor = Cursors.Hand
            };
            btnCopiarTotal.FlatAppearance.BorderSize = 0;
            btnCopiarTotal.Click += (s2, e2) =>
            {
                Clipboard.SetText(total.ToString("N2").Replace(",", "").Replace(".", ","));
                btnCopiarTotal.Text = "✓";
            };
            fila.Controls.Add(btnCopiarTotal);

            scroll.Controls.Add(fila);
            y += 34;
        }

        // ─────────────────────────────────────────────────────────
        // Eventos botones
        // ─────────────────────────────────────────────────────────

        private void BtnGuardarCAE_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCAE.Text) || txtCAE.Text.Length < 10)
            {
                MessageBox.Show("Ingresá el CAE (14 dígitos) antes de guardar.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNroComprobante.Text))
            {
                MessageBox.Show("Ingresá el número de comprobante (ej: 00001-00000042).", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _pagoService.GuardarDatosArca(
                    _pago.Id,
                    txtCAE.Text.Trim(),
                    dtpVencCAE.Value.Date,
                    txtNroComprobante.Text.Trim()
                );

                lblEstado.Text = $"✓ CAE {txtCAE.Text.Trim()} guardado correctamente.";
                lblEstado.ForeColor = Color.FromArgb(39, 174, 96);

                btnGuardarCAE.Enabled = false;
                btnOmitir.Enabled = false;

                System.Threading.Thread.Sleep(800);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOmitir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "¿Confirmas que no vas a emitir la Factura C en ARCA para este cobro?\n" +
                "Podés hacerlo manualmente después.",
                "Omitir ARCA", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                _pagoService.MarcarPagoOmitidoArca(_pago.Id);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────────────────
        // Helper: parsear "Marzo 2025" → DateTime
        // ─────────────────────────────────────────────────────────
        private static DateTime? ParsearPeriodo(string periodo)
        {
            if (string.IsNullOrEmpty(periodo)) return null;
            var meses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                {"enero",1},{"febrero",2},{"marzo",3},{"abril",4},
                {"mayo",5},{"junio",6},{"julio",7},{"agosto",8},
                {"septiembre",9},{"octubre",10},{"noviembre",11},{"diciembre",12},
                {"matricula",1}
            };
            var partes = periodo.Trim().Split(' ');
            if (partes.Length >= 2
                && meses.TryGetValue(partes[0], out int mes)
                && int.TryParse(partes[partes.Length - 1], out int anio))
                return new DateTime(anio, mes, 1);
            return null;
        }
    }

}
