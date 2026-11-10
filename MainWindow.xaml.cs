using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LoginApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => TokenBox.Focus();
    }

    // Arrastar a janela pela barra de cima
    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Enter_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TokenBox.Password))
        {
            Fail("Digite o token de acesso para continuar.");
            return;
        }

        if (!AuthService.IsValid(TokenBox.Password))
        {
            Fail("Token inválido. Verifique e tente novamente.");
            TokenBox.Clear();
            return;
        }

        // Login ok: abre o painel e fecha esta janela
        var app = new AppWindow();
        app.Show();
        Close();
    }

    private void Fail(string text)
    {
        Shake();
        MsgText.Text = text;
        MsgText.Foreground = new SolidColorBrush(Color.FromRgb(0xF8, 0x71, 0x71));
        TokenBox.Focus();
    }

    // Tremidinha no campo quando há erro
    private void Shake()
    {
        var anim = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(300) };
        anim.KeyFrames.Add(new LinearDoubleKeyFrame(-6, KeyTime.FromPercent(0.25)));
        anim.KeyFrames.Add(new LinearDoubleKeyFrame(6, KeyTime.FromPercent(0.75)));
        anim.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1.0)));
        ShakeTransform.BeginAnimation(TranslateTransform.XProperty, anim);
    }
}
