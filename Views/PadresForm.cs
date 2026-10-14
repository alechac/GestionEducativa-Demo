using GestionEducativa.Models;
using GestionEducativa.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    /// <summary>
    /// Pantalla de gestión de Padres/Tutores: alta, edición, baja (lógica) y vinculación
    /// con alumnos. Sigue el mismo criterio que la pestaña de Alumnos.
    /// </summary>
    public class PadresForm : Form
    {
        private readonly PadreService _padreService = new PadreService();
        private readonly AlumnoService _alumnoService = new AlumnoService();

        // Búsqueda / listado
        private TextBox _txtBuscarPadre;
        private Button _btnBuscarPadre;
        private Button _btnNuevo;
        private DataGridView _gridPadres;

        // Ficha del padre
        private Label _lblFicha;
        private TextBox _txtNombre, _txtApellido, _txtDni, _txtCuit, _txtTelefono, _txtEmail;
        private Button _btnGuardar, _btnEliminar, _btnCancelar;

        // Alumnos vinculados
        private Label _lblVinculados;
        private DataGridView _gridHijos;
        private Button _btnQuitarVinculo;

        // Vincular alumno nuevo
        private TextBox _txtBuscarAlumno;
        private Button _btnBuscarAlumno;
        private DataGridView _gridBusquedaAlumnos;
        private Button _btnVincular;

        private Label _lblEstado;

        private Padre _padreActual; // null = alta nueva

        public PadresForm()
        {
            Text = "Padres / Tutores";
            Width = 1020;
            Height = 900;
            AutoScroll = true; // por si en pantallas chicas no entra todo: aparece scroll, nunca queda tapado
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);

            Construir();
            BuscarPadres();
            HabilitarFicha(false);
        }

        private void Construir()
        {
            var lblTitulo = new Label
            {
                Text = "👪 Padres / Tutores",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 60, 117),
                Location = new Point(16, 12),
                AutoSize = true
            };
            Controls.Add(lblTitulo);

            // ── Búsqueda ─────────────────────────────────────────────────────────
            var lblBuscar = new Label { Text = "Buscar:", Location = new Point(16, 48), AutoSize = true };
            Controls.Add(lblBuscar);

            _txtBuscarPadre = new TextBox { Location = new Point(75, 45), Size = new Size(280, 26) };
            _txtBuscarPadre.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BuscarPadres(); } };
            Controls.Add(_txtBuscarPadre);

            _btnBuscarPadre = new Button
            {
                Text = "🔍 Buscar",
                Location = new Point(365, 44),
                Size = new Size(95, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White
            };
            _btnBuscarPadre.FlatAppearance.BorderSize = 0;
            _btnBuscarPadre.Click += (s, e) => BuscarPadres();
            Controls.Add(_btnBuscarPadre);

            _btnNuevo = new Button
            {
                Text = "➕ Nuevo padre/tutor",
                Location = new Point(800, 44),
                Size = new Size(170, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White
            };
            _btnNuevo.FlatAppearance.BorderSize = 0;
            _btnNuevo.Click += (s, e) => IniciarAltaNueva();
            Controls.Add(_btnNuevo);

            // ── Grilla de padres ─────────────────────────────────────────────────
            _gridPadres = new DataGridView
            {
                Location = new Point(16, 80),
                Size = new Size(954, 170),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            _gridPadres.Columns.Add(new DataGridViewTextBoxColumn { Name = "colApellido", HeaderText = "Apellido", Width = 130 });
            _gridPadres.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNombre", HeaderText = "Nombre", Width = 130 });
            _gridPadres.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDni", HeaderText = "DNI", Width = 90 });
            _gridPadres.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCuit", HeaderText = "CUIT", Width = 100 });
            _gridPadres.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTelefono", HeaderText = "Teléfono", Width = 100 });
            _gridPadres.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmail", HeaderText = "Email", Width = 180 });
            _gridPadres.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) CargarFicha((Padre)_gridPadres.Rows[e.RowIndex].Tag); };
            Controls.Add(_gridPadres);

            // ── Ficha del padre ──────────────────────────────────────────────────
            _lblFicha = new Label
            {
                Text = "Nuevo padre/tutor",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(16, 262),
                AutoSize = true
            };
            Controls.Add(_lblFicha);

            _txtNombre = CampoConEtiqueta("Nombre:", 16, 292, 280);
            _txtApellido = CampoConEtiqueta("Apellido:", 310, 292, 280);
            _txtDni = CampoConEtiqueta("DNI:", 604, 292, 160);
            _txtCuit = CampoConEtiqueta("CUIT:", 776, 292, 194);
            _txtTelefono = CampoConEtiqueta("Teléfono:", 16, 352, 280);
            _txtEmail = CampoConEtiqueta("Email:", 310, 352, 400);

            _btnGuardar = new Button
            {
                Text = "💾 Guardar",
                Location = new Point(16, 396),
                Size = new Size(120, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 60, 117),
                ForeColor = Color.White
            };
            _btnGuardar.FlatAppearance.BorderSize = 0;
            _btnGuardar.Click += (s, e) => Guardar();
            Controls.Add(_btnGuardar);

            _btnEliminar = new Button
            {
                Text = "🗑 Eliminar",
                Location = new Point(146, 396),
                Size = new Size(120, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White
            };
            _btnEliminar.FlatAppearance.BorderSize = 0;
            _btnEliminar.Click += (s, e) => Eliminar();
            Controls.Add(_btnEliminar);

            _btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(276, 396),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 230, 230)
            };
            _btnCancelar.FlatAppearance.BorderSize = 0;
            _btnCancelar.Click += (s, e) => { HabilitarFicha(false); BuscarPadres(); };
            Controls.Add(_btnCancelar);

            // ── Alumnos vinculados ───────────────────────────────────────────────
            _lblVinculados = new Label
            {
                Text = "Alumnos vinculados a este padre/tutor:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(16, 442),
                AutoSize = true
            };
            Controls.Add(_lblVinculados);

            _gridHijos = new DataGridView
            {
                Location = new Point(16, 466),
                Size = new Size(700, 110),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            _gridHijos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colHijoNombre", HeaderText = "Alumno", Width = 260 });
            _gridHijos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colHijoNivel", HeaderText = "Nivel", Width = 160 });
            Controls.Add(_gridHijos);

            _btnQuitarVinculo = new Button
            {
                Text = "✂ Quitar vínculo",
                Location = new Point(726, 466),
                Size = new Size(150, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 230, 230)
            };
            _btnQuitarVinculo.FlatAppearance.BorderSize = 0;
            _btnQuitarVinculo.Click += (s, e) => QuitarVinculo();
            Controls.Add(_btnQuitarVinculo);

            // ── Vincular alumno nuevo ────────────────────────────────────────────
            var lblVincular = new Label
            {
                Text = "Vincular un alumno (buscar por nombre):",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(16, 588),
                AutoSize = true
            };
            Controls.Add(lblVincular);

            _txtBuscarAlumno = new TextBox { Location = new Point(16, 612), Size = new Size(280, 26) };
            _txtBuscarAlumno.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BuscarAlumnos(); } };
            Controls.Add(_txtBuscarAlumno);

            _btnBuscarAlumno = new Button
            {
                Text = "🔍 Buscar alumno",
                Location = new Point(306, 611),
                Size = new Size(140, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White
            };
            _btnBuscarAlumno.FlatAppearance.BorderSize = 0;
            _btnBuscarAlumno.Click += (s, e) => BuscarAlumnos();
            Controls.Add(_btnBuscarAlumno);

            _gridBusquedaAlumnos = new DataGridView
            {
                Location = new Point(16, 646),
                Size = new Size(700, 130),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            _gridBusquedaAlumnos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBusqNombre", HeaderText = "Alumno", Width = 260 });
            _gridBusquedaAlumnos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBusqNivel", HeaderText = "Nivel", Width = 140 });
            _gridBusquedaAlumnos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBusqDni", HeaderText = "DNI", Width = 100 });
            _gridBusquedaAlumnos.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Vincular((Alumno)_gridBusquedaAlumnos.Rows[e.RowIndex].Tag); };
            Controls.Add(_gridBusquedaAlumnos);

            _btnVincular = new Button
            {
                Text = "🔗 Vincular\nseleccionado",
                Location = new Point(726, 646),
                Size = new Size(150, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White
            };
            _btnVincular.FlatAppearance.BorderSize = 0;
            _btnVincular.Click += (s, e) =>
            {
                if (_gridBusquedaAlumnos.SelectedRows.Count > 0)
                    Vincular((Alumno)_gridBusquedaAlumnos.SelectedRows[0].Tag);
                else
                    MessageBox.Show("Seleccioná primero un alumno de la lista.", "Padres", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            Controls.Add(_btnVincular);

            _lblEstado = new Label
            {
                Location = new Point(16, 800),
                Size = new Size(954, 40),
                Font = new Font("Segoe UI", 9F)
            };
            Controls.Add(_lblEstado);
        }

        private TextBox CampoConEtiqueta(string etiqueta, int x, int y, int ancho)
        {
            Controls.Add(new Label { Text = etiqueta, Location = new Point(x, y), AutoSize = true });
            var txt = new TextBox { Location = new Point(x, y + 20), Size = new Size(ancho, 26) };
            Controls.Add(txt);
            return txt;
        }

        // ══════════════════════════════════════════════════════════════════════

        private void BuscarPadres()
        {
            try
            {
                var padres = _padreService.Buscar(_txtBuscarPadre.Text);
                _gridPadres.Rows.Clear();
                foreach (var p in padres)
                {
                    int idx = _gridPadres.Rows.Add(p.Apellido, p.Nombre, p.DNI, p.CUIT, p.Telefono, p.Email);
                    _gridPadres.Rows[idx].Tag = p;
                }
                MostrarEstado($"{padres.Count} padre(s)/tutor(es) encontrado(s).", false);
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al buscar: " + ex.Message, true);
            }
        }

        private void IniciarAltaNueva()
        {
            _padreActual = null;
            _txtNombre.Clear();
            _txtApellido.Clear();
            _txtDni.Clear();
            _txtCuit.Clear();
            _txtTelefono.Clear();
            _txtEmail.Clear();
            _gridHijos.Rows.Clear();
            _lblFicha.Text = "Nuevo padre/tutor";
            HabilitarFicha(true);
            _txtNombre.Focus();
        }

        private void CargarFicha(Padre padre)
        {
            _padreActual = padre;
            _txtNombre.Text = padre.Nombre;
            _txtApellido.Text = padre.Apellido;
            _txtDni.Text = padre.DNI;
            _txtCuit.Text = padre.CUIT;
            _txtTelefono.Text = padre.Telefono;
            _txtEmail.Text = padre.Email;
            _lblFicha.Text = $"Editando: {padre.NombreCompleto}";
            HabilitarFicha(true);
            CargarHijos();
        }

        private void HabilitarFicha(bool habilitar)
        {
            _txtNombre.Enabled = habilitar;
            _txtApellido.Enabled = habilitar;
            _txtDni.Enabled = habilitar;
            _txtCuit.Enabled = habilitar;
            _txtTelefono.Enabled = habilitar;
            _txtEmail.Enabled = habilitar;
            _btnGuardar.Enabled = habilitar;
            _btnCancelar.Enabled = habilitar;
            _btnEliminar.Enabled = habilitar && _padreActual != null;

            bool tieneVinculo = habilitar && _padreActual != null;
            _btnQuitarVinculo.Enabled = tieneVinculo;
            _txtBuscarAlumno.Enabled = tieneVinculo;
            _btnBuscarAlumno.Enabled = tieneVinculo;
            _btnVincular.Enabled = tieneVinculo;

            if (!habilitar)
            {
                _lblFicha.Text = "Elegí un padre/tutor de la lista, o creá uno nuevo";
                _gridHijos.Rows.Clear();
                _gridBusquedaAlumnos.Rows.Clear();
            }
        }

        private void Guardar()
        {
            if (string.IsNullOrWhiteSpace(_txtNombre.Text) || string.IsNullOrWhiteSpace(_txtApellido.Text))
            {
                MostrarEstado("Nombre y Apellido son obligatorios.", true);
                return;
            }

            var padre = new Padre
            {
                Id = _padreActual?.Id ?? 0,
                Nombre = _txtNombre.Text.Trim(),
                Apellido = _txtApellido.Text.Trim(),
                DNI = string.IsNullOrWhiteSpace(_txtDni.Text) ? null : _txtDni.Text.Trim(),
                CUIT = string.IsNullOrWhiteSpace(_txtCuit.Text) ? null : _txtCuit.Text.Trim(),
                Telefono = string.IsNullOrWhiteSpace(_txtTelefono.Text) ? null : _txtTelefono.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim()
            };

            try
            {
                if (_padreActual == null)
                {
                    int id = _padreService.Insertar(padre);
                    padre.Id = id;
                    MostrarEstado($"✓ Padre/tutor '{padre.NombreCompleto}' creado correctamente.", false);
                }
                else
                {
                    _padreService.Actualizar(padre);
                    MostrarEstado($"✓ Padre/tutor '{padre.NombreCompleto}' actualizado correctamente.", false);
                }

                CargarFicha(padre);
                BuscarPadres();
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al guardar: " + ex.Message, true);
            }
        }

        private void Eliminar()
        {
            if (_padreActual == null) return;

            var confirmacion = MessageBox.Show(
                $"¿Eliminar a '{_padreActual.NombreCompleto}'?\n\n" +
                "No se borra el historial de comprobantes ya emitidos a su nombre, solo deja " +
                "de aparecer en las búsquedas. Se puede reactivar después si hace falta.",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                _padreService.EliminarLogico(_padreActual.Id);
                MostrarEstado($"✓ '{_padreActual.NombreCompleto}' eliminado.", false);
                HabilitarFicha(false);
                BuscarPadres();
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al eliminar: " + ex.Message, true);
            }
        }

        private void CargarHijos()
        {
            if (_padreActual == null) return;
            try
            {
                var vinculados = _padreService.ObtenerAlumnosPorPadre(_padreActual.Id);
                _gridHijos.Rows.Clear();
                foreach (var a in vinculados)
                {
                    int idx = _gridHijos.Rows.Add(a.Nombre, a.Nivel);
                    _gridHijos.Rows[idx].Tag = a;
                }
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al cargar los alumnos vinculados: " + ex.Message, true);
            }
        }

        private void QuitarVinculo()
        {
            if (_padreActual == null) return;
            if (_gridHijos.SelectedRows.Count == 0)
            {
                MostrarEstado("Seleccioná primero un alumno de la lista de vinculados.", true);
                return;
            }

            var alumno = (Alumno)_gridHijos.SelectedRows[0].Tag;
            var confirmacion = MessageBox.Show(
                $"¿Quitar el vínculo entre '{_padreActual.NombreCompleto}' y '{alumno.Nombre}'?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmacion != DialogResult.Yes) return;

            try
            {
                _padreService.DesvincularAlumno(_padreActual.Id, alumno.Id);
                MostrarEstado("✓ Vínculo eliminado.", false);
                CargarHijos();
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al quitar el vínculo: " + ex.Message, true);
            }
        }

        private void BuscarAlumnos()
        {
            try
            {
                var resultados = _alumnoService.ObtenerTodos(seccion: 0, busqueda: _txtBuscarAlumno.Text);
                _gridBusquedaAlumnos.Rows.Clear();
                foreach (var a in resultados)
                {
                    int idx = _gridBusquedaAlumnos.Rows.Add(a.Nombre, a.Nivel, a.Dni);
                    _gridBusquedaAlumnos.Rows[idx].Tag = a;
                }
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al buscar alumnos: " + ex.Message, true);
            }
        }

        private void Vincular(Alumno alumno)
        {
            if (_padreActual == null || alumno == null) return;
            try
            {
                _padreService.VincularAlumno(_padreActual.Id, alumno.Id);
                MostrarEstado($"✓ '{alumno.Nombre}' vinculado a '{_padreActual.NombreCompleto}'.", false);
                CargarHijos();
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al vincular: " + ex.Message, true);
            }
        }

        private void MostrarEstado(string mensaje, bool esError)
        {
            _lblEstado.Text = mensaje;
            _lblEstado.ForeColor = esError ? Color.FromArgb(192, 57, 43) : Color.FromArgb(39, 174, 96);
        }
    }
}
