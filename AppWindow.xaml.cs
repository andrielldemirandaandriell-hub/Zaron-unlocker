using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LoginApp;

public partial class AppWindow : Window
{
    private readonly ObservableCollection<string> _projects = new() { "Projeto Alpha", "Projeto Beta", "Projeto Gama" };
    private readonly ObservableCollection<string> _activity = new();
    private readonly DateTime _sessionStart = DateTime.Now;

    public AppWindow()
    {
        InitializeComponent();

        ProjectList.ItemsSource = _projects;
        ActivityList.ItemsSource = _activity;

        Log("Login realizado");
        RefreshStats();
    }

    // ---------- janela ----------
    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        new MainWindow().Show(); // volta para a tela de login
        Close();
    }

    // ---------- navegação ----------
    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (PageAbout == null) return; // ainda carregando o XAML

        PageDashboard.Visibility = NavDashboard.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        PageProjects.Visibility  = NavProjects.IsChecked  == true ? Visibility.Visible : Visibility.Collapsed;
        PageSettings.Visibility  = NavSettings.IsChecked  == true ? Visibility.Visible : Visibility.Collapsed;
        PageAbout.Visibility     = NavAbout.IsChecked     == true ? Visibility.Visible : Visibility.Collapsed;
    }

    // ---------- dashboard ----------
    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        Log("Painel atualizado");
        RefreshStats();
    }

    private void RefreshStats()
    {
        ProjectCountText.Text = _projects.Count.ToString();
        SessionText.Text = _sessionStart.ToString("HH:mm");
        LastUpdateText.Text = $"Atualizado às {DateTime.Now:HH:mm:ss}";
    }

    private void Log(string text) => _activity.Insert(0, $"{DateTime.Now:HH:mm:ss}   {text}");

    // ---------- projetos ----------
    private void AddProject_Click(object sender, RoutedEventArgs e)
    {
        var name = NewProjectBox.Text.Trim();
        if (name.Length == 0) return;

        _projects.Add(name);
        NewProjectBox.Clear();
        Log($"Projeto criado: {name}");
        RefreshStats();
    }

    private void NewProjectBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) AddProject_Click(sender, e);
    }

    private void RemoveProject_Click(object sender, RoutedEventArgs e)
    {
        if (ProjectList.SelectedItem is not string name) return;

        _projects.Remove(name);
        Log($"Projeto removido: {name}");
        RefreshStats();
    }

    // ---------- configurações ----------
    private void Option_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || sender is not CheckBox box) return;
        Log($"{box.Content}: {(box.IsChecked == true ? "ativado" : "desativado")}");
    }
}
