#Feed Reader Application

The Feed Reader is a simple desktop application developed in C# using Windows Presentation Foundation (WPF) that enables users to fetch and view articles from different RSS feed sources.

## Functionality

    Add Feed: Allows users to input a feed URL and add it to the application.
    Select Feed: Provides the option to choose from added feed sources or view articles from all feeds.
    Load Articles: Fetches and displays the latest articles from selected feeds or all feeds asynchronously.

## Implementation
### Technologies Used:

    C#: Programming language used for the application.
    WPF: Framework for creating desktop applications for Windows.

### Key Components:

    MainWindow: Main interface that includes options to add feeds, select specific feeds, and load articles.
    Article Class: Facilitates the storage of article information, including title, publication date, feed source, and URL.
    HTTP Client: Utilized to fetch XML data from provided feed URLs.

### Operation:

    Upon launch, the application provides an interface to add feed URLs, select specific feeds, and fetch articles from the selected or all added feeds.

## How to Use

    Adding a Feed: Enter a feed URL into the provided textbox and click the "Add Feed" button to include it in the feed list.
    Selecting a Feed: Choose a specific feed from the dropdown menu or select "All Feeds" to view articles from all added feeds.
    Loading Articles: Click the "Load Articles" button to retrieve and display the latest articles from the selected feed(s).

## Note

    The application asynchronously fetches articles from RSS feeds and displays them, allowing users to keep track of multiple feeds simultaneously.