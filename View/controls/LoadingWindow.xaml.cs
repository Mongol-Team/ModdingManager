using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using View;

namespace ViewControls
{
    public partial class LoadingWindow : BaseWindow
    {
        public LoadingWindow()
        {
            InitializeComponent();
        }
        public string Message
        {
            get => ProgressMessageBox.Text;
            set => ProgressMessageBox.Text = value;
        }
        public double Progress
        {
            get => LoadingProgressBar.Value;
            set => LoadingProgressBar.Value = value;
        }
        public void SetProgressBounds(int min, int total)
        {
            LoadingProgressBar.Maximum = total;
            LoadingProgressBar.Minimum = min;
        }
        public void EndLoading()
        {
            LoadingProgressBar.Value = LoadingProgressBar.Maximum;
            ProgressMessageBox.Text = "Загрузка завершена";
            Close();
        }
    }
}
