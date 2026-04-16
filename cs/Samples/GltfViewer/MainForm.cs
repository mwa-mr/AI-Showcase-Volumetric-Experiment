using System;
using System.IO;
using System.Windows.Forms;

namespace Sample
{
    public interface IGltfViewer
    {
        void SetGltfFile(Uri uri);
        bool HandleKeyDown(Keys keyData);
        string HelpString();

        public Action RefreshUIText { get; set; }
        public string DisplayName { get; }
    }

    public class MainForm : Form
    {
        private IGltfViewer _viewer;
        private Button _reloadButton;

        private const string _titleText = "Gltf Viewer";
        private const string _pendingText = "Connecting to volumetric system ...";
        private const string _readyText = "Click to open a GLTF file ...\n\n";

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public IGltfViewer Viewer
        {
            get => _viewer;
            set
            {
                _viewer = value;
                _viewer.RefreshUIText += OnRefreshUIText;
                UpdateText(_readyText + _viewer.HelpString());
            }
        }

        public MainForm()
        {
            InitializeComponent();

            this.KeyDown += OnKeyDown;
            this.KeyPreview = true; // Allow the form to receive key events

            _reloadButton = new Button
            {
                Text = _pendingText,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(100, 0, 100, 0)
            };
            _reloadButton.Click += ReloadButton_Click;
            this.Controls.Add(_reloadButton);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewer != null)
            {
                _viewer.HandleKeyDown(e.KeyCode);
                UpdateText(_readyText + _viewer.HelpString());
            }
        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            var path = OpenGltfFileDialog();
            if (File.Exists(path))
            {
                Viewer?.SetGltfFile(new Uri(path));
                this.Text = $"{_titleText}: {Viewer.DisplayName}";
            }
        }

        private void OnRefreshUIText()
        {
            UpdateText(_readyText + _viewer.HelpString());
        }

        private void UpdateText(string text)
        {
            if (_reloadButton.InvokeRequired)   // Ensure to update on the UI thread
            {
                _reloadButton.Invoke(new Action(() => _reloadButton.Text = text));
            }
            else
            {
                _reloadButton.Text = text;
            }
        }

        private string OpenGltfFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "GLTF Files (*.gltf;*.glb)|*.gltf;*.glb",
                Title = "Select a GLTF File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return null;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            //
            // MainForm
            //
            AllowDrop = true;
            ClientSize = new System.Drawing.Size(800, 400);
            Font = new System.Drawing.Font("Consolas", 12F);
            MaximizeBox = false;
            Text = _titleText;
            ResumeLayout(false);
        }
    }
}
