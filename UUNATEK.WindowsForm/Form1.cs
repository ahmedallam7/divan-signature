using UUNATRK.Application.Enums;
using UUNATRK.Application.Models;
using UUNATRK.Application.Services.Printer;
using Svg;

namespace UUNATEK.WindowsForm
{
    public partial class Form1 : Form
    {
        private readonly PrinterService _printer;
        private string? _selectedFilePath;

        public Form1(PrinterService printer)
        {
            _printer = printer;
            InitializeComponent();

            // Populate paper dropdown (exclude Custom for now)
            foreach (Paper p in Enum.GetValues<Paper>())
            {
                if (p == Paper.Custom) continue;
                cboPaper.Items.Add(p);
            }
            cboPaper.SelectedItem = Paper.A4;

            UpdateSimulation();
        }

        // ── Connection ─────────────────────────────────────────────

        private void btnConnect_Click(object? sender, EventArgs e)
        {
            try
            {
                var comPort = txtComPort.Text.Trim();
                if (!int.TryParse(txtBaudRate.Text.Trim(), out var baudRate))
                {
                    MessageBox.Show("Invalid baud rate.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                btnConnect.Enabled = false;
                btnConnect.Text = "Connecting...";

                _printer.OpenPort(comPort, baudRate);

                btnConnect.Text = "Connected";
                RefreshStatus();
            }
            catch (Exception ex)
            {
                btnConnect.Text = "Connect";
                btnConnect.Enabled = true;
                MessageBox.Show($"Failed to connect: {ex.Message}", "Connection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Status ─────────────────────────────────────────────────

        private void btnGetStatus_Click(object? sender, EventArgs e)
        {
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            var status = _printer.GetStatus();
            lblIsOpen.Text = $"IsOpen: {status.IsOpen}";
            lblPortName.Text = $"Port: {status.PortName}";
            lblIsPrinting.Text = $"Printing: {status.IsPrinting}";
        }

        // ── Browse SVG ─────────────────────────────────────────────

        private void btnBrowse_Click(object? sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            _selectedFilePath = openFileDialog.FileName;
            lblFileName.Text = Path.GetFileName(_selectedFilePath);
            UpdateSimulation();
        }

        // ── Settings changed / resize → re-render simulation ─────────

        private void OnSettingChanged(object? sender, EventArgs e)
        {
            UpdateSimulation();
        }

        private void OnSimulationResize(object? sender, EventArgs e)
        {
            UpdateSimulation();
        }

        // ── Simulation rendering ───────────────────────────────────

        private void UpdateSimulation()
        {
            var picWidth = picSimulation.Width;
            var picHeight = picSimulation.Height;
            if (picWidth <= 0 || picHeight <= 0) return;

            // Get paper dimensions in mm
            var paper = cboPaper.SelectedItem is Paper p ? p : Paper.A4;
            var (paperW, paperH) = PaperSizes.GetSizeMm(paper);

            // Scale paper to fit inside the PictureBox with some margin
            const int margin = 20;
            var availW = picWidth - margin * 2;
            var availH = picHeight - margin * 2;
            var fitScale = Math.Min(availW / paperW, availH / paperH);

            var drawW = (int)(paperW * fitScale);
            var drawH = (int)(paperH * fitScale);
            var paperX = (picWidth - drawW) / 2;
            var paperY = (picHeight - drawH) / 2;

            var bmp = new Bitmap(picWidth, picHeight);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Gray background
            g.Clear(Color.LightGray);

            // White paper
            g.FillRectangle(Brushes.White, paperX, paperY, drawW, drawH);
            g.DrawRectangle(Pens.DarkGray, paperX, paperY, drawW, drawH);

            // Render SVG onto the paper if a file is loaded
            if (!string.IsNullOrEmpty(_selectedFilePath) && File.Exists(_selectedFilePath))
            {
                try
                {
                    var svgDoc = SvgDocument.Open(_selectedFilePath);
                    var svgBitmap = svgDoc.Draw();
                    if (svgBitmap != null)
                    {
                        // Parse position values (strip "mm" suffix)
                        var xPos = ParseMm(txtXPosition.Text);
                        var yPos = ParseMm(txtYPosition.Text);
                        var scale = (float)nudScale.Value;
                        var rotation = (float)nudRotation.Value;
                        var invertX = chkInvertX.Checked;
                        var invertY = chkInvertY.Checked;

                        // Convert SVG pixel size to mm (SVG default is 96 DPI)
                        var svgWidthMm = svgBitmap.Width / 96.0 * 25.4;
                        var svgHeightMm = svgBitmap.Height / 96.0 * 25.4;

                        // Scaled SVG size in mm then to screen pixels
                        var scaledW = (float)(svgWidthMm * scale * fitScale);
                        var scaledH = (float)(svgHeightMm * scale * fitScale);

                        // Position on paper in screen pixels
                        var svgScreenX = paperX + (float)(xPos * fitScale);
                        var svgScreenY = paperY + (float)(yPos * fitScale);

                        // Center point for rotation
                        var centerX = svgScreenX + scaledW / 2f;
                        var centerY = svgScreenY + scaledH / 2f;

                        var state = g.Save();

                        // Apply transforms: translate to center, rotate, invert, then draw
                        g.TranslateTransform(centerX, centerY);
                        g.RotateTransform(rotation);
                        g.ScaleTransform(invertX ? -1f : 1f, invertY ? -1f : 1f);
                        g.TranslateTransform(-centerX, -centerY);

                        // Clip to paper bounds
                        g.SetClip(new Rectangle(paperX, paperY, drawW, drawH));

                        g.DrawImage(svgBitmap, svgScreenX, svgScreenY, scaledW, scaledH);

                        g.Restore(state);
                        svgBitmap.Dispose();
                    }
                }
                catch
                {
                    // Silently ignore rendering errors in simulation
                }
            }

            var old = picSimulation.Image;
            picSimulation.Image = bmp;
            old?.Dispose();
        }

        private static double ParseMm(string text)
        {
            var clean = text.Trim().Replace("mm", "", StringComparison.OrdinalIgnoreCase);
            return double.TryParse(clean, out var val) ? val : 0;
        }

        // ── Generate G-code Preview ──────────────────────────────

        private void btnGenerateGCode_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                MessageBox.Show("Please select an SVG file first.", "No File Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var paper = cboPaper.SelectedItem is Paper p ? p : Paper.A4;
                var (pw, ph) = PaperSizes.GetSizeMm(paper);

                var printRequest = new PrintRequest
                {
                    Width = $"{pw}mm",
                    Height = $"{ph}mm",
                    XPosition = txtXPosition.Text,
                    YPosition = txtYPosition.Text,
                    Scale = (int)nudScale.Value,
                    Rotation = (int)nudRotation.Value,
                    InvertX = chkInvertX.Checked,
                    InvertY = chkInvertY.Checked
                };

                using var stream = File.OpenRead(_selectedFilePath);
                var gcode = SvgConverter.ConvertToGCode(stream, printRequest);

                if (gcode.Count == 0)
                {
                    txtGCodePreview.Text = "";
                    lblResult.ForeColor = Color.OrangeRed;
                    lblResult.Text = "No drawable paths found. Convert text to path in Inkscape first.";
                    return;
                }

                txtGCodePreview.Text = string.Join(Environment.NewLine, gcode);
                lblResult.ForeColor = Color.Green;
                lblResult.Text = $"Generated {gcode.Count} G-code commands.";
            }
            catch (Exception ex)
            {
                txtGCodePreview.Text = "";
                lblResult.ForeColor = Color.Red;
                lblResult.Text = $"Error: {ex.Message}";
            }
        }

        // ── Print ──────────────────────────────────────────────────

        private async void btnPrint_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                MessageBox.Show("Please select an SVG file first.", "No File Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_printer.IsOpen)
            {
                MessageBox.Show("Printer is not connected. Please connect first.", "Not Connected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnPrint.Enabled = false;
                btnPrint.Text = "Printing...";
                lblResult.Text = "";
                lblResult.ForeColor = SystemColors.ControlText;

                // Width/Height come from the selected paper size
                var paper = cboPaper.SelectedItem is Paper p ? p : Paper.A4;
                var (pw, ph) = PaperSizes.GetSizeMm(paper);

                var printRequest = new PrintRequest
                {
                    Width = $"{pw}mm",
                    Height = $"{ph}mm",
                    XPosition = txtXPosition.Text,
                    YPosition = txtYPosition.Text,
                    Scale = (int)nudScale.Value,
                    Rotation = (int)nudRotation.Value,
                    InvertX = chkInvertX.Checked,
                    InvertY = chkInvertY.Checked
                };

                using var stream = File.OpenRead(_selectedFilePath);
                var gcode = SvgConverter.ConvertToGCode(stream, printRequest);

                if (gcode.Count == 0)
                {
                    lblResult.ForeColor = Color.OrangeRed;
                    lblResult.Text = "No drawable paths found. If SVG has text, convert to path in Inkscape first (Path > Object to Path).";
                    return;
                }

                var response = await _printer.Print(gcode);

                lblResult.ForeColor = Color.Green;
                lblResult.Text = $"{response.Message} ({response.CommandsSent} cmds)";
            }
            catch (Exception ex)
            {
                lblResult.ForeColor = Color.Red;
                lblResult.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnPrint.Enabled = true;
                btnPrint.Text = "Print";
                RefreshStatus();
            }
        }
    }
}
