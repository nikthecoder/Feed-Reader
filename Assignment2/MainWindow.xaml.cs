using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
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
using System.Xml.Linq;

namespace Assignment2
{
    // Article class to facilitate article printing
    public class Article
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Feed { get; set; }
        public string Url { get; set; }
    }
    
    public partial class MainWindow : Window
    {
        // Global variables for access among methods
        private Dictionary<string, string> feedUrls = new Dictionary<string, string>(); // dictionary to keep feedname and url connected
        private List<Article> articleList = new List<Article>();
        private Thickness spacing = new Thickness(5);
        private HttpClient http = new HttpClient();
        // We will need these as instance variables to access in event handlers.
        private TextBox addFeedTextBox;
        private Button addFeedButton;
        private ComboBox selectFeedComboBox;
        private Button loadArticlesButton;
        private StackPanel articlePanel;

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            // Window options
            Title = "Feed Reader";
            Width = 800;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Scrolling
            var root = new ScrollViewer();
            root.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Content = root;

            // Main grid
            var grid = new Grid();
            root.Content = grid;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var addFeedLabel = new Label
            {
                Content = "Feed URL:",
                Margin = spacing
            };
            grid.Children.Add(addFeedLabel);

            addFeedTextBox = new TextBox
            {
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(addFeedTextBox);
            Grid.SetColumn(addFeedTextBox, 1);

            addFeedButton = new Button
            {
                Content = "Add Feed",
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(addFeedButton);
            Grid.SetColumn(addFeedButton, 2);
            addFeedButton.Click += AddFeedButton_Click;

            var selectFeedLabel = new Label
            {
                Content = "Select Feed:",
                Margin = spacing
            };
            grid.Children.Add(selectFeedLabel);
            Grid.SetRow(selectFeedLabel, 1);

            selectFeedComboBox = new ComboBox
            {
                Margin = spacing,
                Padding = spacing,
                IsEditable = false
            };
            grid.Children.Add(selectFeedComboBox);
            Grid.SetRow(selectFeedComboBox, 1);
            Grid.SetColumn(selectFeedComboBox, 1);
            selectFeedComboBox.Items.Add("All Feeds");
            selectFeedComboBox.SelectedItem = "All Feeds";

            loadArticlesButton = new Button
            {
                Content = "Load Articles",
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(loadArticlesButton);
            Grid.SetRow(loadArticlesButton, 1);
            Grid.SetColumn(loadArticlesButton, 2);
            loadArticlesButton.Click += LoadArticlesButton_Click;

            articlePanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = spacing
            };
            grid.Children.Add(articlePanel);
            Grid.SetRow(articlePanel, 2);
            Grid.SetColumnSpan(articlePanel, 3);
        }

        private async void LoadArticlesButton_Click(object sender, RoutedEventArgs e)
        {
            // disable load articles button, clear articles in list and panel
            loadArticlesButton.IsEnabled = false;
            articlePanel.Children.Clear();
            articleList.Clear();

            // if "All Feeds" is selected, load all feeds asynchronously and add articles
            if (selectFeedComboBox.SelectedItem.ToString() == "All Feeds")
            {
                var tasks = feedUrls.Values.Select(LoadDocumentAsync).ToList();
                var results = await Task.WhenAll(tasks);
                for (int i = 0; i < tasks.Count; i++)
                {
                    // Get all titles and dates as an array of strings.
                    string[] titles = results[i].Descendants("item").Elements("title").Select(t => t.Value).Take(5).ToArray();
                    string[] dates = results[i].Descendants("pubDate").Select(t => t.Value).Take(5).ToArray();

                    for (int j = 0; j < 5; j++)
                    {
                        Article article = new Article
                        {
                            Title = titles[j],
                            Date = DateTime.ParseExact(dates[j].Substring(0, 25), "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                            Feed = results[i].Descendants("title").First().Value,
                            Url = feedUrls.ElementAt(i).Value
                        };
                        articleList.Add(article);
                    }
                }
            }

            // if a specific feed is selected, load all feeds sequentially and add articles
            else
            {
                string url = feedUrls[selectFeedComboBox.SelectedItem.ToString()];
                XDocument document = await LoadDocumentAsync(url);
                // Get all titles and dates as an array of strings.
                string[] allTitles = document.Descendants("item").Elements("title").Select(t => t.Value).Take(5).ToArray();
                string[] allDates = document.Descendants("pubDate").Select(t => t.Value).Take(5).ToArray();

                for (int i = 0; i < 5; i++)
                {
                    Article article = new Article
                    {
                        Title = allTitles[i],
                        Date = DateTime.ParseExact(allDates[i].Substring(0, 25), "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        Feed = document.Descendants("title").First().Value,
                        Url = url
                    };
                    articleList.Add(article);
                }
            }

            // print articles
            foreach (var article in articleList.OrderByDescending(d => d.Date))
            {
                Print(article);
            }

            // enable load articles button
            loadArticlesButton.IsEnabled = true; 
        }

        private async void AddFeedButton_Click(object sender, RoutedEventArgs e)
        {
            // disable add feed button
            addFeedButton.IsEnabled = false;

            // load url and document
            string url = addFeedTextBox.Text;
            XDocument document = await LoadDocumentAsync(url);

            // Get the title of the xmlFile as a string.
            string feedName = document.Descendants("title").First().Value;

            // if feed already exists, show error message, else add feed and url
            if (feedUrls.ContainsKey(feedName))
            {
                MessageBox.Show("Feed already exists.");
                addFeedButton.IsEnabled = true;
                return;
            }
            else
            {
                selectFeedComboBox.Items.Add(feedName);
                selectFeedComboBox.SelectedItem = feedName;

                feedUrls.Add(feedName, url);
            }

            // enable add feed button
            addFeedButton.IsEnabled = true;
        }

        private void Print(Article article)
        {
            // Print all articles in panel

                var articleBox = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = spacing
                };
                articlePanel.Children.Add(articleBox);

                var articleTitle = new TextBlock
                {
                    Text = Convert.ToString(article.Date + "  -  " + article.Title),
                    FontWeight = FontWeights.Bold,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                articleBox.Children.Add(articleTitle);

                var articleWebsite = new TextBlock
                {
                    Text = article.Feed
                };
                articleBox.Children.Add(articleWebsite);
        }

        private async Task<XDocument> LoadDocumentAsync(string url)
        {
            // This is just to simulate a slow/large data transfer and make testing easier.
            // Remove it if you want to.
            await Task.Delay(1000);
            var response = await http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var feed = XDocument.Load(stream);
            return feed;
        }
    }
}
