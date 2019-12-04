﻿using System;
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
using System.Data.SqlClient;

namespace GameDatabase_App
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowGames();
        }

        // Получение списка игр по указанным параметрам поиска  
        private void ShowGames()
        {
            // Формирование строки подключения
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder() 
            { 
                DataSource = "DESKTOP-HOME", 
                InitialCatalog = "GameDatabase", 
                IntegratedSecurity = true 
            };
            // Текст запроса
            string sqlExpression =
                @"  SELECT
	                    dbo.Games.id,
	                    dbo.Games.title,
                        dbo.Games.summary,
	                    dbo.Games.release_date,
	                    GameScore.avg_score
                    FROM
	                    dbo.Games
	                    LEFT OUTER JOIN
	                    (
		                    SELECT 
			                    game_id, 
			                    AVG(score) AS avg_score
		                    FROM
			                    dbo.Reviews
		                    GROUP BY
			                    dbo.Reviews.game_id
	                    ) AS GameScore
		                    ON  dbo.Games.id = GameScore.game_id";
            // Подключение
            using (SqlConnection connection = new SqlConnection() { ConnectionString = connectionStringBuilder.ConnectionString})
            {
                // Открытие подключения
                connection.Open();
                // Команда sql
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                // Выполнение запроса   
                SqlDataReader dataReader = command.ExecuteReader();
                //Проверка наличия строк
                if (dataReader.HasRows)
                {
                    while(dataReader.Read())
                    {
                        // Добавление разделителя строк
                        // -----------------------------------------------
                        if (GamesList.Children.Count > 0)
                            GamesList.Children.Add(new Separator());
                        // -----------------------------------------------

                        // Создание Grid в который будет компоноваться Tile
                        // -----------------------------------------------
                        Grid gameTileGrid = new Grid()
                        {
                            Height = 210,
                            //ShowGridLines = true,
                            //Background = new SolidColorBrush(Colors.LightGray),
                            Margin = new Thickness(2)
                        };
                        GamesList.Children.Add(gameTileGrid);
                        // Столбцы
                        gameTileGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        gameTileGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        gameTileGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        // Строки
                        gameTileGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        gameTileGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        gameTileGrid.RowDefinitions.Add(new RowDefinition());
                        gameTileGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        // -----------------------------------------------

                        // Обложка игры
                        // -----------------------------------------------
                        Border gameCoverBorder = new Border()
                        {
                            BorderThickness = new Thickness(1),
                            BorderBrush = new SolidColorBrush(Colors.Black),
                            Margin = new Thickness(5),
                            Height = 180,
                            Width = 120,
                            VerticalAlignment = VerticalAlignment.Top
                        };
                        gameCoverBorder.Child = new Image()
                        {
                            Source = new BitmapImage(new Uri(@"C:\Users\antka\Downloads\51XiGWHvaZL.jpg", UriKind.RelativeOrAbsolute)),
                            Stretch = Stretch.Uniform
                        };
                        gameTileGrid.Children.Add(gameCoverBorder);
                        Grid.SetRowSpan(gameCoverBorder, 4);
                        // -----------------------------------------------

                        // Название игры
                        // -----------------------------------------------
                        Label gameTitleLabel = new Label()
                        {
                            Content = dataReader.GetString(1),
                            FontSize = 18
                        };
                        gameTileGrid.Children.Add(gameTitleLabel);
                        Grid.SetColumn(gameTitleLabel, 1);
                        // -----------------------------------------------

                        // Дата выхода игры
                        // -----------------------------------------------
                        Label gameReleaseDateLabel = new Label()
                        {
                            Content = $"Дата выхода: " +
                            $"{(dataReader.IsDBNull(3) ? "TBA" : DateTime.Parse(dataReader.GetDateTime(3).ToString()).ToShortDateString())}",
                            FontSize = 10
                        };
                        gameTileGrid.Children.Add(gameReleaseDateLabel);
                        Grid.SetColumn(gameReleaseDateLabel, 1);
                        Grid.SetRow(gameReleaseDateLabel, 1);
                        // -----------------------------------------------

                        // Оценка игры
                        // -----------------------------------------------
                        Border gameScoreBorder = new Border()
                        {
                            Background = dataReader.IsDBNull(4) || dataReader.GetInt32(4) <= 50 ? new SolidColorBrush(Colors.Red) : (dataReader.GetInt32(4) <= 70 ? new SolidColorBrush(Colors.Gold) : new SolidColorBrush(Colors.YellowGreen)),
                            Margin = new Thickness(5),
                            BorderThickness = new Thickness(1),
                            BorderBrush = new SolidColorBrush(Colors.Black),
                            Height = 50,
                            Width = 50
                        };
                        gameScoreBorder.Child = new Label()
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Content = "-"
                        };
                        gameTileGrid.Children.Add(gameScoreBorder);
                        Grid.SetColumn(gameScoreBorder, 2);
                        Grid.SetRow(gameScoreBorder, 0);
                        Grid.SetRowSpan(gameScoreBorder, 2);
                        // -----------------------------------------------

                        // Описание игры
                        // -----------------------------------------------
                        TextBlock gameSummaryTextBlock = new TextBlock()
                        {
                            Text = dataReader.GetString(2),
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(3)
                        };
                        Grid.SetColumn(gameSummaryTextBlock, 1);
                        Grid.SetRow(gameSummaryTextBlock, 2);
                        Grid.SetColumnSpan(gameSummaryTextBlock, 2);
                        gameTileGrid.Children.Add(gameSummaryTextBlock);
                        // -----------------------------------------------

                        // Кнопка
                        // -----------------------------------------------
                        Button gameMoreInfoButton = new Button()
                        {
                            Padding = new Thickness(3),
                            Margin = new Thickness(5),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Content = "Подробнее..."
                        };
                        gameTileGrid.Children.Add(gameMoreInfoButton);
                        Grid.SetColumn(gameMoreInfoButton, 1);
                        Grid.SetColumnSpan(gameMoreInfoButton, 3);
                        Grid.SetRow(gameMoreInfoButton, 4);
                    }
                }
            }
        }
    }
}
