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
            // Вход в приложение через логин
            LoginWindow loginWindow = new LoginWindow();
            if (!(bool)loginWindow.ShowDialog())
                this.Close();
            InitializeComponent();
            Tag = loginWindow.Tag;
            if((bool)Tag)
            {
                SettingsMenu.Visibility = Visibility.Visible;
            }
            ShowGames();
            UpdateSearchParameters();
        }

        // Получение списка игр по указанным параметрам поиска  
        private void ShowGames()
        {
            // Очистка списка
            GamesList.Children.Clear();
            try
            {
                // Подключение
                using (SqlConnection connection = new SqlConnection() { ConnectionString = Properties.Settings.Default.userConnection })
                {
                    // Открытие подключения
                    connection.Open();
                    // Команда sql
                    SqlCommand command = GenerateSqlCommand(connection);
                    // Выполнение запроса   
                    SqlDataReader dataReader = command.ExecuteReader();
                    //Проверка наличия строк
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
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
                                FontSize = 24,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                Content = dataReader.IsDBNull(4) ? "-" : dataReader.GetInt32(4).ToString()
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

                            // -----------------------------------------------
                            StackPanel buttons = new StackPanel()
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Margin = new Thickness(5),
                            };
                            Grid.SetColumn(buttons, 1);
                            Grid.SetColumnSpan(buttons, 3);
                            Grid.SetRow(buttons, 4);
                            gameTileGrid.Children.Add(buttons);
                            // -----------------------------------------------

                            if ((bool)Tag)
                            {
                                // Кнопка удалить
                                // -----------------------------------------------
                                Button DeleteButton = new Button()
                                {
                                    Tag = dataReader.GetInt32(0),
                                    Padding = new Thickness(3),
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    Content = "Удалить"
                                };
                                DeleteButton.Click += DeleteGame_Click;
                                buttons.Children.Add(DeleteButton);
                                // -----------------------------------------------

                                // Кнопка редактировать
                                // -----------------------------------------------
                                Button EditButton = new Button()
                                {
                                    Tag = dataReader.GetInt32(0),
                                    Padding = new Thickness(3),
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    Content = "Редактировать"
                                };
                                EditButton.Click += GameEditButton_Click;
                                buttons.Children.Add(EditButton);
                                // -----------------------------------------------
                            }

                            // Кнопка подробнее
                            // -----------------------------------------------
                            Button gameMoreInfoButton = new Button()
                            {
                                Tag = dataReader.GetInt32(0),
                                Padding = new Thickness(3),
                                Content = "Подробнее..."
                            };
                            gameMoreInfoButton.Click += GameMoreInfoButton_Click;
                            buttons.Children.Add(gameMoreInfoButton);
                            // -----------------------------------------------
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"В процессе обработки данных произошла ошибка:\n{ex}", "Ошибка обработки данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    
        // Формирование поискового запроса для SQL Server
        private SqlCommand GenerateSqlCommand(SqlConnection connection)
        {
            // Количество условий поиска
            int terms = 0;
            // Основной текст запроса, в случае отсутствие условий поиска выполняется только он
            SqlCommand command = new SqlCommand()
            {
                CommandText =
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
		                    ON  dbo.Games.id = GameScore.game_id",
                Connection = connection
            };
            // Количество выделенных объектов
            int count = 0;
            // Поиск по разработчикам
            foreach (CheckBox developer in SearchDevelopersList.Children)
            {
                if ((bool)developer.IsChecked)
                {
                    command.Parameters.Add(new SqlParameter($"@developer{developer.Tag}", developer.Tag));
                    if (count == 0)
                        command.CommandText +=
                            $@"  INNER JOIN 
	                            (
		                            SELECT 
                                        dbo.Games_Developers.game_id
		                            FROM 
                                        dbo.Games_Developers 
                                    WHERE 
                                        dbo.Games_Developers.developer_id IN (@developer{developer.Tag}";
                    else
                        command.CommandText += $@", @developer{developer.Tag}";
                    count++;
                }
            }
            if (count != 0)
            {
                command.CommandText +=
                    @")
		            GROUP BY dbo.Games_Developers.game_id
	            ) AS GameDevelopers
		            ON dbo.Games.id = GameDevelopers.game_id";
            }

            count = 0;
            // Поиск по издателям
            foreach (CheckBox publisher in SearchPublishersList.Children)
            {
                if ((bool)publisher.IsChecked)
                {
                    command.Parameters.Add(new SqlParameter($"@publisher{publisher.Tag}", publisher.Tag));
                    if (count == 0)
                        command.CommandText +=
                            $@"  INNER JOIN 
	                            (
		                            SELECT 
                                        dbo.Games_Publishers.game_id
		                            FROM 
                                        dbo.Games_Publishers 
                                    WHERE 
                                        dbo.Games_Publishers.publisher_id IN (@publisher{publisher.Tag}";
                    else
                        command.CommandText += $@", @publisher{publisher.Tag}";
                    count++;
                }
            }
            if (count != 0)
            {
                command.CommandText +=
                    @")
		            GROUP BY dbo.Games_Publishers.game_id
	            ) AS GamePublishers
		            ON dbo.Games.id = GamePublishers.game_id";
            }

            count = 0;
            // Поиск по жанрам
            foreach (CheckBox genre in SearchGenresList.Children)
            {
                if ((bool)genre.IsChecked)
                {
                    command.Parameters.Add(new SqlParameter($"@genre{genre.Tag}", genre.Tag));
                    if (count == 0)
                        command.CommandText +=
                            $@"  INNER JOIN 
	                            (
		                            SELECT 
                                        dbo.Games_Genres.game_id
		                            FROM 
                                        dbo.Games_Genres 
                                    WHERE 
                                        dbo.Games_Genres.genre_id IN (@genre{genre.Tag}";
                    else
                        command.CommandText += $@", @genre{genre.Tag}";
                    count++;
                }
            }
            if (count != 0)
            {
                command.CommandText +=
                    @")
		            GROUP BY dbo.Games_Genres.game_id
	            ) AS GameGenres
		            ON dbo.Games.id = GameGenres.game_id";
            }

            count = 0;
            // Поиск по платформам
            foreach (CheckBox platform in SearchPlatformsList.Children)
            {
                if ((bool)platform.IsChecked)
                {
                    command.Parameters.Add(new SqlParameter($"@platform{platform.Tag}", platform.Tag));
                    if (count == 0)
                        command.CommandText +=
                            $@"  INNER JOIN 
	                            (
		                            SELECT 
                                        dbo.Games_Platforms.game_id
		                            FROM 
                                        dbo.Games_Platforms 
                                    WHERE 
                                        dbo.Games_Platforms.platform_id IN (@platform{platform.Tag}";
                    else
                        command.CommandText += $@", @platform{platform.Tag}";
                    count++;
                }
            }
            if (count != 0)
            {
                command.CommandText +=
                    @")
		            GROUP BY dbo.Games_Platforms.game_id
	            ) AS GamePlatforms
		            ON dbo.Games.id = GamePlatforms.game_id";
            }

            // Поиск по названию игры
            if (GameTitleSearchTextBlock.Text.Length > 0)
            {
                command.Parameters.Add(new SqlParameter("@title", '%' + GameTitleSearchTextBlock.Text + '%'));
                command.CommandText += 
                    @" WHERE dbo.Games.Title LIKE @title";
                terms++;
            }

            // Поиск по дате выхода (с)
            if(GameReleaseFromDatePicker.SelectedDate != null)
            {
                if (terms == 0)
                    command.CommandText += @" WHERE";
                else
                    command.CommandText += @" AND";
                command.Parameters.Add(new SqlParameter("@releaseDateFrom", GameReleaseFromDatePicker.SelectedDate));
                command.CommandText += $@" dbo.Games.release_date >= @releaseDateFrom";
                terms++;
            }

            // Поиск по дате выхода (до)
            if (GameReleaseToDatePicker.SelectedDate != null)
            {
                if (terms == 0)
                    command.CommandText += @" WHERE";
                else
                    command.CommandText += @" AND";
                command.Parameters.Add(new SqlParameter("@releaseDateTo", GameReleaseToDatePicker.SelectedDate));
                command.CommandText += $@" dbo.Games.release_date <= @releaseDateTo";
                terms++;
            }

            // Поиск по средней оценке (>)
            if (GameScoreFromSlider.Value > 0)
            {
                if (terms == 0)
                    command.CommandText += @" WHERE";
                else
                    command.CommandText += @" AND";
                command.Parameters.Add(new SqlParameter("@scoreFrom", GameScoreFromSlider.Value));
                command.CommandText += $@" GameScore.avg_score >= @scoreFrom";
                terms++;
            }

            // Поиск по средней оценке (<)
            if (GameScoreToSlider.Value < 100)
            {
                if (terms == 0)
                    command.CommandText += @" WHERE";
                else
                    command.CommandText += @" AND";
                command.Parameters.Add(new SqlParameter("@scoreTo", GameScoreToSlider.Value));
                command.CommandText += $@" GameScore.avg_score <= @scoreTo";
                terms++;
            }

            switch (((ComboBoxItem)SortByComboBox.SelectedItem).Tag)
            {
                case "0":
                    command.CommandText += " ORDER BY dbo.Games.title ASC";
                    break;
                case "1":
                    command.CommandText += " ORDER BY dbo.Games.title DESC";
                    break;
                case "2":
                    command.CommandText += " ORDER BY dbo.Games.release_date ASC";
                    break;
                case "3":
                    command.CommandText += " ORDER BY dbo.Games.release_date DESC";
                    break;
                case "4":
                    command.CommandText += " ORDER BY GameScore.avg_score ASC";
                    break;
                case "5":
                    command.CommandText += " ORDER BY GameScore.avg_score DESC";
                    break;
            }
            return command;
        }

        // Поиск
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGames();
        }

        // Очистка параметров поиска
        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            GameTitleSearchTextBlock.Clear();
            GameReleaseFromDatePicker.SelectedDate = null;
            GameReleaseToDatePicker.SelectedDate = null;
            GameScoreFromSlider.Value = 0;
            GameScoreToSlider.Value = 100;
            foreach (CheckBox developer in SearchDevelopersList.Children)
            {
                developer.IsChecked = false;
            }
            foreach (CheckBox publisher in SearchPublishersList.Children)
            {
                publisher.IsChecked = false;
            }
            foreach (CheckBox genre in SearchGenresList.Children)
            {
                genre.IsChecked = false;
            }
            foreach (CheckBox platform in SearchPlatformsList.Children)
            {
                platform.IsChecked = false;
            }
            SortByComboBox.SelectedIndex = 0;
        }

        // Значение слайдера не может быть выше значения второго слайдера
        private void GameScoreFromSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue > GameScoreToSlider.Value)
                GameScoreFromSlider.Value = e.OldValue;
        }

        // Значение слайдера не может быть ниже значения первого слайдера
        private void GameScoreToSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < GameScoreFromSlider.Value)
                GameScoreToSlider.Value = e.OldValue;
        }

        // Загрузка в раздел поиска информации о разработчиках, издателях, жанрах и платформах
        private void UpdateSearchParameters()
        {
            // Очистка списка
            SearchDevelopersList.Children.Clear();
            try
            {
                // Подключение
                using (SqlConnection connection = new SqlConnection() { ConnectionString = Properties.Settings.Default.userConnection })
                {
                    // Открытие подключения
                    connection.Open();
                    // Команда sql
                    SqlCommand command = new SqlCommand("SELECT id, title FROM dbo.Developers ORDER BY title ASC", connection);
                    // Выполнение запроса   
                    SqlDataReader dataReader = command.ExecuteReader();
                    //Проверка наличия строк
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            CheckBox developer = new CheckBox()
                            {
                                Tag = dataReader.GetInt32(0),
                                Content = dataReader.GetString(1),
                                FontSize = 11
                            };
                            SearchDevelopersList.Children.Add(developer);
                        }
                        dataReader.Close();
                    }
                    command.CommandText = "SELECT id, title FROM dbo.Publishers ORDER BY title ASC";
                    // Выполнение запроса   
                    dataReader = command.ExecuteReader();
                    //Проверка наличия строк
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            CheckBox publisher = new CheckBox()
                            {
                                Tag = dataReader.GetInt32(0),
                                Content = dataReader.GetString(1),
                                FontSize = 11
                            };
                            SearchPublishersList.Children.Add(publisher);
                        }
                        dataReader.Close();
                    }
                    command.CommandText = "SELECT id, title FROM dbo.Genres ORDER BY title ASC";
                    // Выполнение запроса   
                    dataReader = command.ExecuteReader();
                    //Проверка наличия строк
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            CheckBox genre = new CheckBox()
                            {
                                Tag = dataReader.GetInt32(0),
                                Content = dataReader.GetString(1),
                                FontSize = 11
                            };
                            SearchGenresList.Children.Add(genre);
                        }
                        dataReader.Close();
                    }
                    command.CommandText = "SELECT id, title FROM dbo.Platforms ORDER BY title ASC";
                    // Выполнение запроса   
                    dataReader = command.ExecuteReader();
                    //Проверка наличия строк
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            CheckBox genre = new CheckBox()
                            {
                                Tag = dataReader.GetInt32(0),
                                Content = dataReader.GetString(1),
                                FontSize = 11
                            };
                            SearchPlatformsList.Children.Add(genre);
                        }
                        dataReader.Close();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"В процессе получения данных произошла ошибка:\n{ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Открытие окна игры
        private void GameMoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            GameWindow gameWindow = new GameWindow((int)((Button)sender).Tag, (bool)Tag ? true : false);
            gameWindow.Show();
        }

        // Открытие окна редактирования игры
        private void GameEditButton_Click(object sender, RoutedEventArgs e)
        {
            EditGameWindow window = new EditGameWindow((int)((Button)sender).Tag);
            window.ShowDialog();
            ShowGames();
        }

        // Открытие окна добавления игры
        private void AddGame_Click(object sender, RoutedEventArgs e)
        {
            EditGameWindow window = new EditGameWindow();
            window.ShowDialog();
            ShowGames();
        }

        // Удаление игры
        private void DeleteGame_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show($"Вы действительно хотите удалить {((Label)(((Grid)(((StackPanel)(((Button)sender).Parent)).Parent)).Children[1])).Content}?", "Удаление игры", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.userConnection))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand($@"DELETE FROM dbo.Games WHERE dbo.Games.id = @id", connection))
                        {
                            command.Parameters.Add(new SqlParameter("@id", ((Button)sender).Tag));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"В процессе обработки данных произошла ошибка:\n{ex}", "Ошибка обработки данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            ShowGames();
        }
    }
}