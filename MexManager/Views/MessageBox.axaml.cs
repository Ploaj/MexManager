using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace MexManager;

public partial class MessageBox : Window
{
    public enum MessageBoxButtons
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel
    }

    public enum MessageBoxResult
    {
        Ok,
        Cancel,
        Yes,
        No
    }

    private MessageBoxResult res = MessageBoxResult.Cancel;

    public MessageBox()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            res = MessageBoxResult.Yes;
            this.Close();
        }
    }

    public static Task<MessageBoxResult> Show(string text, string title, MessageBoxButtons buttons)
    {
        if (App.MainWindow != null)
            return Show(App.MainWindow, text, title, buttons);

        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public static Task<MessageBoxResult> Show(Window parent, string text, string title, MessageBoxButtons buttons)
    {
        var msgbox = new MessageBox()
        {
            Title = title
        };

        var textblock = msgbox.FindControl<TextBlock>("Text");
        if (textblock != null)
            textblock.Text = text;
        var buttonPanel = msgbox.FindControl<StackPanel>("Buttons");

        void AddButton(string caption, MessageBoxResult r, bool def = false)
        {
            if (buttonPanel != null)
            {
                var btn = new Button { Content = caption };
                btn.Click += (_, __) => {
                    msgbox.res = r;
                    msgbox.Close();
                };
                buttonPanel.Children.Add(btn);
                if (def)
                    msgbox.res = MessageBoxResult.Cancel;
            }
        }

        if (buttons == MessageBoxButtons.Ok || buttons == MessageBoxButtons.OkCancel)
            AddButton("Ok", MessageBoxResult.Ok, true);

        if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel)
        {
            AddButton("Yes", MessageBoxResult.Yes);
            AddButton("No", MessageBoxResult.No, true);
        }

        if (buttons == MessageBoxButtons.OkCancel || buttons == MessageBoxButtons.YesNoCancel)
            AddButton("Cancel", MessageBoxResult.Cancel, true);


        var tcs = new TaskCompletionSource<MessageBoxResult>();
        msgbox.Closed += delegate { tcs.TrySetResult(msgbox.res); };
        if (parent != null)
            msgbox.ShowDialog(parent);
        else msgbox.Show();
        return tcs.Task;
    }

}