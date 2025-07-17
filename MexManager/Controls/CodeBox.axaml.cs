using Avalonia.Controls;
using Avalonia.Media;
using mexLib.Types;
using System.Linq;

namespace MexManager.Controls;

public partial class CodeBox : UserControl
{
    private readonly static IBrush CompileSuccess = Brushes.Green;

    private readonly static IBrush CompileFailed = Brushes.Red;

    /// <summary>
    /// 
    /// </summary>
    public CodeBox()
    {
        InitializeComponent();

        UpdateLineNumbers();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    public void SetError(string? error)
    {
        ErrorBlock.Text = error;

        if (string.IsNullOrEmpty(error))
        {
            ErrorBlock.Foreground = CompileSuccess;
        }
        else
        {
            ErrorBlock.Foreground = CompileFailed;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateLineNumbers();

        SetError(null);
        if (DataContext is MexCode code)
        {
            if (code.CompileError != null)
            {
                SetError(code.CompileError.ToString());
            }
            else if (Global.Workspace != null)
            {
                SetError(Global.Workspace.Project.CheckCodeConflict(code)?.ToString());
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private void UpdateLineNumbers()
    {
        if (MainTextBox.Text == null)
            return;

        string[] lines = MainTextBox.Text.Split('\n');
        LineNumbers.Text = string.Join("\n", Enumerable.Range(0, lines.Length));
    }
}