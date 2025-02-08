using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Wpf;
using EpubCore;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BookReader
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); 
            ContentGrid.Children.Add(new WebView2 { VerticalAlignment = VerticalAlignment.Stretch });
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "PDF Files|*.pdf|EPUB Files|*.epub",
                Title = "Choose a PDF or EPUB file"
            };

            if (openDialog.ShowDialog() == true)
            {
                string filePath = openDialog.FileName;
                string extension = Path.GetExtension(filePath).ToLower();

                if (extension == ".pdf")
                {
                    ShowPdf(filePath);
                }
                else if (extension == ".epub")
                {
                    ShowEpub(filePath);
                }
            }
        }

        private void ShowPdf(string filePath)
        {
            WebView2 webView = new WebView2 { Source = new Uri(filePath) };
            ContentGrid.Children.Clear();
            ContentGrid.Children.Add(webView);
        }

        private void ShowEpub(string filePath)
        {
            try
            {
                EpubBook epubBook = EpubReader.Read(filePath);
                StackPanel mainPanel = new StackPanel();

                string title = epubBook.Title;
                string authors = string.Join(", ", epubBook.Authors ?? new[] { "Unknown" });
                TextBlock metaInfo = new TextBlock { Text = $"Title: {title}\nAuthor(s): {authors}" };
                mainPanel.Children.Add(metaInfo);

                TabControl tabControl = new TabControl();

                AddFullTextTab(epubBook, tabControl);
                AddChaptersTab(epubBook, tabControl);

                mainPanel.Children.Add(tabControl);
                ContentGrid.Children.Clear();
                ContentGrid.Children.Add(mainPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void AddFullTextTab(EpubBook epubBook, TabControl tabControl)
        {
            string fullText = epubBook.ToPlainText();
            FlowDocument fullTextDoc = new FlowDocument(new Paragraph(new Run(fullText)));
            TabItem fullTextTab = new TabItem { Header = "Full Book", Content = new FlowDocumentReader { Document = fullTextDoc } };
            tabControl.Items.Add(fullTextTab);
        }

        private void AddChaptersTab(EpubBook epubBook, TabControl tabControl)
        {
            var htmlList = epubBook.Resources.Html?.ToList() ?? new System.Collections.Generic.List<EpubTextFile>();

            if (epubBook.TableOfContents != null && epubBook.TableOfContents.Any())
            {
                int chapterIndex = 0;
                foreach (var chapter in epubBook.TableOfContents)
                {
                    string chapterTitle = chapter.Title;
                    string chapterText = GetChapterText(chapterIndex, htmlList);
                    FlowDocument chapterDoc = new FlowDocument();
                    chapterDoc.Blocks.Add(new Paragraph(new Run(chapterTitle)));
                    chapterDoc.Blocks.Add(new Paragraph(new Run(chapterText)));
                    TabItem chapterTab = new TabItem { Header = chapterTitle, Content = new FlowDocumentReader { Document = chapterDoc } };
                    tabControl.Items.Add(chapterTab);
                    chapterIndex++;
                }
            }
            else
            {
                TabItem emptyTab = new TabItem { Header = "Chapter", Content = new TextBlock { Text = "No chapters" } };
                tabControl.Items.Add(emptyTab);
            }
        }

        private string GetChapterText(int chapterIndex, System.Collections.Generic.List<EpubTextFile> htmlList)
        {
            if (chapterIndex < htmlList.Count)
            {
                string htmlContent = Encoding.UTF8.GetString(htmlList[chapterIndex].Content);
                return Regex.Replace(htmlContent, "<.*?>", "");
            }
            else
            {
                return "[No content]";
            }
        }
    }
}
