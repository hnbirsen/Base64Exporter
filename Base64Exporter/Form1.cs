using System.Security.Cryptography;

namespace Base64Exporter
{
    public partial class Form1 : Form
    {
        private string _inputPath;
        private Button _btnConvert;

        public Form1()
        {
            InitializeComponentCustom();
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Base64 Exporter";
            this.Width = 700;
            this.Height = 180;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                Padding = new Padding(10)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var lbl = new Label { Text = "Input File:", AutoSize = true, Anchor = AnchorStyles.Left, TabIndex = 0 };
            var btnSelect = new Button { Name = "btnSelect", Text = "Select File", AutoSize = true, TabIndex = 2 };
            _btnConvert = new Button { Name = "btnConvert", Text = "Convert to Base64 and save as .txt", AutoSize = true, TabIndex = 4 };
            var txtInput = new TextBox { Name = "txtInput", ReadOnly = true, Anchor = AnchorStyles.Left | AnchorStyles.Right, Width = 450, TabIndex = 1 };
            var progress = new ProgressBar { Name = "progressBar1", Minimum = 0, Maximum = 100, Dock = DockStyle.Fill, TabIndex = 3 };

            btnSelect.Click += (s, e) => SelectFile(txtInput);
            _btnConvert.Click += async (s, e) => await ConvertAndSaveAsync(txtInput, progress);

            panel.Controls.Add(lbl, 0, 0);
            panel.Controls.Add(txtInput, 1, 0);
            panel.Controls.Add(btnSelect, 2, 0);
            panel.SetColumnSpan(progress, 3);
            panel.Controls.Add(progress, 0, 1);
            panel.SetColumnSpan(_btnConvert, 3);
            panel.Controls.Add(_btnConvert, 0, 2);

            this.Controls.Add(panel);
        }

        private void SelectFile(TextBox txtInput)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select file to convert",
                CheckFileExists = true,
                Filter = "All Files (*.*)|*.*"
            };
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                _inputPath = ofd.FileName;
                txtInput.Text = _inputPath;
            }
        }

        private async Task ConvertAndSaveAsync(TextBox txtInput, ProgressBar progress)
        {
            if (string.IsNullOrWhiteSpace(_inputPath) || !File.Exists(_inputPath))
            {
                MessageBox.Show(this, "Please select a valid file.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Title = "Select .txt file to save",
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = Path.GetFileNameWithoutExtension(_inputPath) + ".txt"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            progress.Value = 0;
            _btnConvert.Enabled = false;
            try
            {
                await EncodeFileToBase64Async(_inputPath, sfd.FileName, progress).ConfigureAwait(false);
                MessageBox.Show(this, "Conversion completed!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogException(ex);
                MessageBox.Show(this, "Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progress.Value = 0;
                _btnConvert.Enabled = true;
            }
        }

        // Produces Base64 in a streaming way (without bloating memory).
        private static async Task EncodeFileToBase64Async(string inputPath, string outputPath, ProgressBar progress)
        {
            const int bufferSize = 1024 * 1024; // 1 MB blocks
            var fileInfo = new FileInfo(inputPath);
            long total = fileInfo.Length;
            long processed = 0;

            await using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
            await using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true);
            using var transform = new ToBase64Transform();
            using var crypto = new CryptoStream(output, transform, CryptoStreamMode.Write);

            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            if (progress != null && total == 0)
            {
                progress.Invoke(new Action(() => progress.Style = ProgressBarStyle.Marquee));
            }
            while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                await crypto.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                processed += bytesRead;

                if (progress != null && total > 0)
                {
                    int pct = (int)Math.Clamp((processed * 100L) / total, 0, 100);
                    progress.Invoke(new Action(() => progress.Value = pct));
                }
            }
            crypto.FlushFinalBlock();
            if (progress != null)
            {
                progress.Invoke(new Action(() => progress.Style = ProgressBarStyle.Blocks));
            }
        }

        private static void LogException(Exception ex)
        {
            try
            {
                File.AppendAllText("error.log", $"[{DateTime.Now}] {ex}\n");
            }
            catch { /* ignore logging errors */ }
        }
    }
}
