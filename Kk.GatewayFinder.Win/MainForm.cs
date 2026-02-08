using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kk.GatewayFinder.Win
{
    public partial class MainForm : Form
    {
        private readonly GatewayDiscoveryService _discoveryService = new();
        private readonly string _stateDirectory;
        private readonly string _stateFilePath;
        private List<GatewayCandidate> _foundGateways = new();
        private GatewayCandidate? _currentCandidate;
        private CancellationTokenSource? _cts;

        public MainForm()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _stateDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KropKontrol", "GatewayFinder");
            _stateFilePath = Path.Combine(_stateDirectory, "last-ip.txt");

            txtLastKnown.Text = LoadLastKnownIp() ?? string.Empty;
            UpdateUiState();
        }

        private async void BtnDetect_Click(object sender, EventArgs e)
        {
            await RunDiscoveryAsync();
        }

        private void BtnOpenBrowser_Click(object sender, EventArgs e)
        {
            if (_currentCandidate == null)
            {
                MessageBox.Show(this, "Aucune passerelle détectée pour le moment.", "Navigation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                //var url = $"http://{_currentCandidate.IpAddress}:8080";
                var url = $"http://{_currentCandidate.IpAddress}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Impossible d'ouvrir le navigateur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RunDiscoveryAsync()
        {
            if (_cts != null)
            {
                return;
            }

            lstLog.Items.Clear();
            _currentCandidate = null;
            UpdateUiState();

            btnDetect.Enabled = false;
            btnOpenBrowser.Enabled = false;
            progress.Style = ProgressBarStyle.Marquee;
            lblStatus.Text = "Scan en cours...";

            _cts = new CancellationTokenSource();
            var lastKnown = string.IsNullOrWhiteSpace(txtLastKnown.Text) ? null : txtLastKnown.Text.Trim();

            try
            {
                var found = await _discoveryService.FindGatewaysAsync(lastKnown, !chkContinueAfterFirst.Checked, AppendLog, _cts.Token);
                _foundGateways = found;

                if (found.Count > 0)
                {
                    _currentCandidate = found[0];
                    txtLastKnown.Text = _currentCandidate.IpAddress;
                    SaveLastKnownIp(_currentCandidate.IpAddress);

                    var detail = string.IsNullOrWhiteSpace(_currentCandidate.VerificationDetails)
                        ? "signature Milesight"
                        : _currentCandidate.VerificationDetails;
                    
                    if (found.Count == 1)
                    {
                        lblStatus.Text = $"Gateway confirmée sur {_currentCandidate.IpAddress} ({detail})";
                    }
                    else
                    {

                        lblStatus.BackColor = Color.Orange;                      
                        lblStatus.Text = $"{found.Count} gateways détectées. Sélectionnez une dans la liste.";
                        lblStatus.Font = new Font(lblStatus.Font.FontFamily,11f,FontStyle.Bold);

                    }

                    AppendLog($"Gateway confirmée : {_currentCandidate.IpAddress} ({detail})");
                    lblStatus.BackColor = SystemColors.Control;
                    lblStatus.Font = new Font(lblStatus.Font, FontStyle.Regular);
                }
                else
                {
                    lblStatus.Text = "Aucune passerelle détectée";
                    lblStatus.BackColor = SystemColors.Control;
                    lblStatus.Font = new Font(lblStatus.Font, FontStyle.Regular);
                    _currentCandidate = null;
                }

                UpdateGatewayList();
            }
            catch (OperationCanceledException)
            {
                AppendLog("Scan annulé.");
                lblStatus.Text = "Scan annulé";
            }
            catch (Exception ex)
            {
                AppendLog($"Erreur: {ex.Message}");
                lblStatus.Text = "Erreur durant le scan";
                MessageBox.Show(this, ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                progress.Style = ProgressBarStyle.Blocks;
                progress.Value = 0;
                UpdateUiState();
            }
        }

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendLog), message);
                return;
            }

            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            lstLog.Items.Insert(0, line);
        }

        private void UpdateUiState()
        {
            btnOpenBrowser.Enabled = lstGateways.SelectedItem != null || _currentCandidate != null;
            btnDetect.Enabled = _cts == null;
        }

        private void UpdateGatewayList()
        {
            lstGateways.Items.Clear();
            foreach (var gateway in _foundGateways)
            {
                var display = $"{gateway.IpAddress} - {gateway.VerificationDetails}";
                lstGateways.Items.Add(display);
            }
        }

        private void LstGateways_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGateways.SelectedIndex >= 0 && lstGateways.SelectedIndex < _foundGateways.Count)
            {
                _currentCandidate = _foundGateways[lstGateways.SelectedIndex];
                txtLastKnown.Text = _currentCandidate.IpAddress;
                UpdateUiState();
            }
        }

        private string? LoadLastKnownIp()
        {
            try
            {
                if (File.Exists(_stateFilePath))
                {
                    return File.ReadAllText(_stateFilePath).Trim();
                }
            }
            catch
            {
                // Ignorer
            }

            return null;
        }

        private void SaveLastKnownIp(string ipAddress)
        {
            try
            {
                Directory.CreateDirectory(_stateDirectory);
                File.WriteAllText(_stateFilePath, ipAddress);
            }
            catch
            {
                // Ignorer les erreurs d'IO
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts?.Cancel();

            base.OnFormClosing(e);
        }
    }
}
