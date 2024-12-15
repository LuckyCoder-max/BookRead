using System;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Forms.Integration;
using PdfiumViewer;
using VersOne.Epub;
using PdfSharp.Pdf.Filters;

namespace BookRead
{
    public partial class MainWindow : Window
    {
        private PdfViewer _pdfViewer;

        public MainWindow()
        {
            InitializeComponent();

            _pdfViewer = new PdfViewer
            {
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            PdfHost.Child = _pdfViewer;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files|*.pdf|EPUB Files|*.epub",
                Title = "Выберите PDF, Epub файл"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string pdfFilePath = openFileDialog.FileName;

                try
                {

                    var document = PdfDocument.Load(pdfFilePath);
                    _pdfViewer.Document?.Dispose();
                    _pdfViewer.Document = document;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии PDF: {ex.Message}");
                }

                string epubFilePath = openFileDialog.FileName;

                try
                {
                    EpubBook book = EpubReader.ReadBook(epubFilePath);

                    var firstChapter = book.ReadingOrder.FirstOrDefault();

                    if (firstChapter != null)
                    {
                        myTextBox.Text = firstChapter.Content;
                    }
                    else
                    {
                        MessageBox.Show("Книга не содержит доступных глав.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии EPUB: {ex.Message}");
                }
            }

        }
    }
}
